using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using static _Scripts.LobbyUtil;
using static _Scripts.LobbyNetworking;
using System.Linq;

namespace _Scripts
{
    public class LobbyManager : NetworkBehaviour
    {
        /// <summary>
        /// Made by Jesper Heese
        /// i made the byte array region and edited some functions to fit my game
        /// </summary>
        public static LobbyManager Instance { get; private set; }
        public bool IsSignedIn { get; private set; }
        private bool GameStarted { get; set; }

        public Lobby Lobby { get; private set; }
        private Allocation _allocation;
        private string _playerId;
        private string _joinCode;

        public Dictionary<ulong, string> ConvertedIds = new Dictionary<ulong, string>();

        [SerializeField]
        private GameObject playerPrefab;

        [SerializeField]
        public List<GameObject> players = new();

        [SerializeField]
        private bool quickTest = false;

        public LobbyUi _lobbyUi;

        public bool teamSelected = false;



        [Rpc(SendTo.Everyone)]
        public void UpdatePlayerTeamRpc(string playerId, int teamId)
        {
            if (_lobbyUi != null)
            {
                _lobbyUi.UpdatePlayerParentRpc(playerId, teamId);
            }
        }

        #region ByteArray
        // Assisted by Github Copilot
        // Rpc couldn't send the dictionary directly so it needed to be serialized into a byte array but i had no clue how to do that

        [Rpc(SendTo.ClientsAndHost)]
        public void SyncConvertedIdsRpc(byte[] serializedDictionary)
        {
            ConvertedIds = DeserializeDictionary(serializedDictionary);

            for (int i = 0; i < ConvertedIds.Count; i++)
            {
                var kvp = ConvertedIds.ElementAt(i);
            }
        }

        public void AddConvertedId(ulong ownerClientId, string playerId)
        {
            if (IsServer)
            {
                ConvertedIds.Add(ownerClientId, playerId);

                SyncConvertedIdsRpc(SerializeDictionary(ConvertedIds));
            }
        }

        private byte[] SerializeDictionary(Dictionary<ulong, string> dictionary)
        {
            using (var writer = new FastBufferWriter(1024, Allocator.Temp))
            {
                writer.WriteValueSafe(dictionary.Count);
                foreach (var kvp in dictionary)
                {
                    writer.WriteValueSafe(kvp.Key);
                    writer.WriteValueSafe(kvp.Value);
                }
                return writer.ToArray();
            }
        }

        private Dictionary<ulong, string> DeserializeDictionary(byte[] data)
        {
            var dictionary = new Dictionary<ulong, string>();
            using (var reader = new FastBufferReader(data, Allocator.Temp))
            {
                reader.ReadValueSafe(out int count);
                for (int i = 0; i < count; i++)
                {
                    reader.ReadValueSafe(out ulong key);
                    reader.ReadValueSafe(out string value);
                    dictionary.Add(key, value);
                }
            }
            return dictionary;
        }

        #endregion



        #region Insance
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion
        #region Initialization
        private async void Start()
        {
            try
            {
                _lobbyUi = GetComponent<LobbyUi>();
                LobbyUtil.LobbyUi = _lobbyUi; // set the static reference
                NetworkManager.Singleton.ConnectionApprovalCallback += ApproveConnection;
                await UnityServices.InitializeAsync();
                await SignIn();
                if (quickTest)
                    CreateLobby("TestLobby");
            }
            catch (Exception e)
            {
                Log("Failed to initialize lobby manager: " + e.Message, LogType.Error);
                Status("Failed to initialize lobby manager.", Color.red);
            }
        }

        private async Task SignIn()
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                IsSignedIn = true;
                _playerId = AuthenticationService.Instance.PlayerId;
                return;
            }
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                if (AuthenticationService.Instance.IsSignedIn)
                    IsSignedIn = true;
                _playerId = AuthenticationService.Instance.PlayerId;
            }
            catch (Exception e)
            {
                IsSignedIn = false;
                Log($"Sign in failed: {e.Message}", LogType.Error);
            }
            //Log($"Signed in as {AuthenticationService.Instance.PlayerName}");
        }
        #endregion
        #region Joining
        public async Task<QueryResponse> GetLobbies()
        {
            try
            {
                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync();
                Log($"Got lobbies: {response.Results.Count}");
                return response;
            }
            catch (Exception e)
            {
                Log($"Failed to get lobbies: {e.Message}", LogType.Error);
                return null;
            }
        }

        #endregion
        #region Creating and Joining
        /// <summary>
        /// This method creates a lobby and makes this player the host.
        /// </summary>
        /// <exception cref="LobbyServiceException">The lobby service itself errored.</exception>
        /// <exception cref="NullReferenceException">Something is not assigned.</exception>
        /// <remarks>This is a public void which calls a private async method.</remarks>
        /// <param name="lobbyName">Name for the lobby</param>
        public void CreateLobby(string lobbyName) => CreateLobbyP(lobbyName);

        public void JoinLobby(string lobbyId) => JoinLobbyP(lobbyId);

        // private async void CreateLobbyP(string lobbyName) // Server Creating
        private async void CreateLobbyP(string lobbyName, bool isPrivate = false) // THIS IS GETTING SO COMPLICATED
        {
            try
            {
                Status("Starting Unity Relay service...");
                (Allocation, string, RelayServerData) tuple = await StartHostAllocation(10);

                Status("Starting Unity Networking...");
                StartNetworking(tuple.Item3, true);

                Status("Starting Unity Lobby service...");
                Lobby = await StartHostLobby(isPrivate, lobbyName, tuple.Item2);

                var callbacks = new LobbyEventCallbacks();
                callbacks.PlayerLeft += OnPlayerLeft;
                callbacks.PlayerJoined += ctx => StartCoroutine(CallbacksOnPlayerJoined(ctx));
                callbacks.KickedFromLobby += OnKickedFromLobby;
                await LobbyService.Instance.SubscribeToLobbyEventsAsync(Lobby.Id, callbacks);

                Status("Updating player Data...");
                await UpdatePlayerNameInLobby();

                //Log($"Successfully Created lobby {Lobby.Name}");
                StartHeartbeat();
                After();
                if (quickTest)
                    StartGameRpc();
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case LobbyServiceException:
                        Log($"Failed to create lobby: {e.Message}", LogType.Error);
                        break;
                    case NullReferenceException:
                        Log(
                            $"Failed to find something while creating lobby: {e.Message}",
                            LogType.Error
                        );
                        break;
                }
            }
        }

        private IEnumerator CallbacksOnPlayerJoined(List<LobbyPlayerJoined> obj)
        {
            while (obj[0].Player.Data?["PlayerName"] == null) // waits for the data to come in
                yield return null;
            _lobbyUi.OnNewPlayer();
        }

        private async void JoinLobbyP(string lobbyId) // Client Joining
        {
            try
            {
                Status("Joining Lobby...");
                Lobby = await StartClientLobby(lobbyId);

                Status("Joining Relay service...");
                string relayJoinCode = Lobby.Data["relayJoinCode"].Value;
                RelayServerData serverData = await StartClientAllocation(relayJoinCode);

                Status("Joining Unity Networking...");
                StartNetworking(serverData, false);

                var callbacks = new LobbyEventCallbacks();
                callbacks.PlayerLeft += OnPlayerLeft;
                callbacks.PlayerJoined += ctx => StartCoroutine(CallbacksOnPlayerJoined(ctx));
                callbacks.KickedFromLobby += OnKickedFromLobby;
                await LobbyService.Instance.SubscribeToLobbyEventsAsync(Lobby.Id, callbacks);

                Status("Updating Player Data...");
                await UpdatePlayerNameInLobby();

                Log($"Successfully Joined lobby {Lobby.Name}");
                After();
            }
            catch (Exception e)
            {
                Log($"Failed to join lobby: {e.Message}", LogType.Error);
            }
        }

        private void After()
        {
            Status();
            _lobbyUi.GoToLobby();
            _lobbyUi.OnNewPlayer();
        }

        #endregion
        #region Disconnection
        public void DisconnectLobby() => DisconnectLobbyP();

        private void DisconnectLobbyP()
        {
            try
            {
                Status("Leaving Lobby...");
                NetworkManager.Singleton.Shutdown();
                RemovePlayerRpc(_playerId);
                Log("Left lobby.");
            }
            catch (Exception e)
            {
                Log($"Failed to leave lobby: {e.Message}", LogType.Error);
            }
        }

        [Rpc(SendTo.Server)] // only the server can remove players.
        private void RemovePlayerRpc(string playerId)
        {
            RemovePlayer(playerId);
        }

        private async void RemovePlayer(string playerId)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(Lobby.Id, playerId);
                Lobby = null;
                Status("Left Lobby.", Color.yellow);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void DestroyLobby() => DestroyLobbyP();

        private async void DestroyLobbyP()
        {
            try
            {
                NetworkManager.Singleton.Shutdown();
                await LobbyService.Instance.DeleteLobbyAsync(Lobby.Id);
                Status("Lobby Closed.", Color.yellow);
                Lobby = null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to destroy lobby: {e.Message}");
            }
        }

        private void OnKickedFromLobby()
        {
            Log("Player kicking not implemented yet.", LogType.Warning);
        }

        private void OnPlayerLeft(List<int> wot)
        {
            Log("Player leaving Not implemented yet.", LogType.Warning);
            // output list of int to json in debug log
            // Debug.Log(JsonUtility.ToJson(wot)); // (because I have no idea what it gives.)
        }

        #endregion
        #region GameStarted

        [Rpc(SendTo.Server)]
        public void StartGameRpc()
        {
            if (Lobby.Players.Count != ConvertedIds.Count)
            {
                Debug.LogError(
                    $"Not all players are connected. {Lobby.Players.Count} != {ConvertedIds.Count}"
                );
                return;
            }
            StartGameAllRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void StartGameAllRpc()
        {
            StartingGameAsync();
        }

        private void StartingGameAsync()
        {
            try
            {
                StartCoroutine(GameStartCountdown());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void StartGamePRpc()
        {
            try
            {
                if (GameStarted)
                    return;

                GameStarted = true;
                GameManager.Instance.LoadInPlayers();
                NetworkManager.SceneManager.LoadScene("Main", LoadSceneMode.Single);
            }
            catch (Exception e)
            {
                Log($"Failed to start game: {e.Message}", LogType.Error);
                Status("Failed to start game.", Color.red);
            }
        }

        private IEnumerator GameStartCountdown()
        {
            // MatchIds();
            if (!quickTest)
            {
                Status("Game Starting in 3 seconds...");
                yield return new WaitForSeconds(1);
                Status("Game Starting in 2 seconds...");
                yield return new WaitForSeconds(1);
                Status("Game Starting in 1 seconds...");
                yield return new WaitForSeconds(1);
            }
            StartGamePRpc();
        }

        #endregion
        #region Misc

        public void ApproveConnection(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response
        )
        {
            if (GameStarted)
            {
                response.Approved = false;
                response.Reason = "Game has already started.";
                return;
            }
            response.Approved = true;
        }

        [Rpc(SendTo.Everyone)]
        public void CheckForPlayersRpc()
        {
            players.Clear();
            foreach (Transform trans in GameObject.Find("PlayerParent").transform)
            {
                if (trans is null)
                    continue;
                players.Add(trans.gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            if (!IsServer)
                return;
            LobbyService.Instance.DeleteLobbyAsync(Lobby.Id);
            NetworkManager.Shutdown();
        }

        public void StartHeartbeat()
        {
            StartCoroutine(SendHeartbeatCoroutine());
        }

        private IEnumerator SendHeartbeatCoroutine()
        {
            while (Lobby != null && !GameStarted)
            {
                Task heartbeatTask = LobbyService.Instance.SendHeartbeatPingAsync(Lobby.Id);
                if (heartbeatTask.IsFaulted)
                {
                    Debug.LogError("Failed to send heartbeat: " + heartbeatTask.Exception);
                }
                yield return new WaitForSeconds(15f);
            }
        }
        #endregion
    }
}

