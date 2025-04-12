using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Scripts
{
    public class LobbyUi : MonoBehaviour // add status to the in game UI (for when clicking on create lobby etc.)
    {

        /// <summary>
        /// Made by Jesper Heese
        /// I edited a lot to make it work in my game
        /// </summary>
        /// 

        [Header("Parent Objects")]
        [SerializeField]
        private GameObject mainMenu;

        [SerializeField]
        private GameObject hostMenu;

        [SerializeField]
        private GameObject joinMenu;

        [SerializeField]
        private GameObject lobbyMenu;

        // references to all the UI elements
        [Header("Buttons")]
        [SerializeField]
        private Button mainHost;

        [SerializeField]
        private Button mainJoin;

        [SerializeField]
        private Button hostCreate;

        [SerializeField]
        private Button lobbyDelete;

        [SerializeField]
        private Button lobbyDisconnect;

        [SerializeField]
        private Button startGame;

        [Header("Input Fields")]
        [SerializeField]
        private TMP_InputField mainName;

        [SerializeField]
        private TMP_InputField hostLobbyName;

        [SerializeField]
        private GameObject newLobbyPrefab;

        [SerializeField]
        private GameObject newPlayerPrefab;

        [SerializeField] private GameObject uiPlayerList;

        [SerializeField] private GameObject team1PlayerList;

        [SerializeField] private GameObject team2PlayerList;

        [SerializeField] private GameObject lobbyList;

        private const int YDownAmount = 100;
        private int _currentYDownAmount;
        public List<GameObject> _playerList = new List<GameObject>();
        private List<GameObject> _lobbies = new();

        [SerializeField]
        private TextMeshProUGUI statusText;

        private void Start() // Always starts in main menu
        {
            ChangeView();
            if (LobbyManager.Instance.IsSignedIn == false)
            {
                ChangeStatus("Signing in...");
                StartCoroutine(WaitForSignIn());
                return;
            }
            ChangeView(mainMenu);
            mainHost.interactable = false;
            mainJoin.interactable = false;
            SetupInputFields();
            ChangeStatus();
        }

        private IEnumerator WaitForSignIn()
        {
            while (LobbyManager.Instance.IsSignedIn == false)
            {
                yield return null;
            }
            Start();
        }

        private void SetupInputFields()
        {
            mainName.text = "";
            hostLobbyName.text = "";
            mainName.onValueChanged.AddListener(
                delegate
                {
                    CheckName();
                }
            );
        }

        private void CheckName()
        {
            if (!string.IsNullOrEmpty(mainName.text))
            {
                mainHost.interactable = true;
                mainJoin.interactable = true;
            }
            else
            {
                mainHost.interactable = false;
                mainJoin.interactable = false;
            }
        }

        public async void HostMenu()
        {
            try
            {
                ChangeStatus("Updating name...");
                await ChangeName(mainName.text);
                ChangeView(hostMenu);
                ChangeStatus();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to change lobby: {e.Message}");
                ChangeStatus("Failed to change name.", Color.red);
            }
        }

        public async void JoinMenu()
        {
            try
            {
                ChangeStatus("Updating name...");
                await ChangeName(mainName.text);
                ChangeView(joinMenu);
                ChangeStatus("Getting lobbies...");
                GetLobbies();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to join lobby: {e.Message}");
                ChangeStatus("Failed to change name.", Color.red);
            }
        }

        private async Task ChangeName(string newName)
        {
            try
            {
                await LobbyUtil.ChangeName(newName);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to change name: {e.Message}");
            }
        }

        private void ChangeView(GameObject view = null)
        {
            mainMenu.SetActive(false);
            hostMenu.SetActive(false);
            joinMenu.SetActive(false);
            lobbyMenu.SetActive(false);
            view?.SetActive(true);
        }

        public async void GetLobbies()
        {
            try
            {
                QueryResponse lobbies = await LobbyManager.Instance.GetLobbies();
                if (lobbies.Results.Count == 0)
                {
                    Debug.Log("No lobbies found");
                    return;
                }
                if (_lobbies != null)
                {
                    foreach (GameObject obj in _lobbies)
                    {
                        Destroy(obj);
                    }
                }
                lobbies.Results.ForEach(lobby =>
                {
                    Debug.Log($"Lobby: {lobby.Name} - {lobby.Id}");
                    GameObject real = CreateLobbyUi(lobby);
                    _lobbies.Add(real);
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get lobbies: {e.Message}");
                ChangeStatus("Failed to get lobbies.", Color.red);
            }
        }

        /// <summary>
        /// This method creates a new UI for each lobby.
        /// This adds a listener to the button to join the lobby.
        /// </summary>
        /// <param name="lobby">Specifies which lobby is being used.</param>
        /// <exception cref="Exception">Thrown when the UI fails to create a lobby.</exception>
        private GameObject CreateLobbyUi(Lobby lobby) // Spawns a new Prefab for each lobby
        {
            try
            {
                GameObject newLobby = Instantiate(newLobbyPrefab, lobbyList.transform);
                newLobby.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    $"{lobby.Name}\n{lobby.Players[0].Data["PlayerName"].Value}"; // should be lobby name
                newLobby
                    .transform.GetChild(0)
                    .GetChild(1)
                    .GetComponent<Button>()
                    .onClick.AddListener(() => JoinLobby(lobby));
                return newLobby;
            }
            catch (Exception e)
            {
                ChangeStatus("Failed to get new player.", Color.red);
                Debug.LogError($"Failed to create a UI for a lobby: {lobby.Name}");
                Debug.LogException(e);
            }
            return null;
        }

        private void JoinLobby(Lobby lobby)
        {
            Debug.Log($"Joining lobby: {lobby.Name}");
            LobbyManager.Instance.JoinLobby(lobby.Id);
        }

        public void CreateGame() => LobbyManager.Instance.CreateLobby(hostLobbyName.text);


        private Dictionary<string, GameObject> playerToGameObjectMap = new(); 

        public void OnNewPlayer()
        {
            Debug.Log("[Client] OnNewPlayer called.");
            var playerList = LobbyManager.Instance.Lobby.Players;
            foreach (GameObject obj in _playerList)
            {
                Destroy(obj);
            }

            _currentYDownAmount = 0;
            playerToGameObjectMap.Clear();

            foreach (Unity.Services.Lobbies.Models.Player player in playerList)
            {
                GameObject newPlayer = Instantiate(newPlayerPrefab, uiPlayerList.transform);
                newPlayer.GetComponent<RectTransform>().anchoredPosition = new Vector3(
                    0,
                    _currentYDownAmount,
                    0
                );
                newPlayer.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    player.Data["PlayerName"].Value;
                _currentYDownAmount -= YDownAmount;
                _playerList.Add(newPlayer);

                playerToGameObjectMap[player.Id] = newPlayer;
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void UpdatePlayerParentRpc(string playerId, int newTeam)
        {
            if (playerToGameObjectMap.TryGetValue(playerId, out GameObject playerObject))
            {
                Transform newParent = newTeam == 1 ? team1PlayerList.transform : team2PlayerList.transform;
                playerObject.transform.SetParent(newParent);
            }
        }

        public void RemovePlayerFromUI(string playerId)
        {
            if (playerToGameObjectMap.TryGetValue(playerId, out GameObject playerObject))
            {
                Destroy(playerObject);
                playerToGameObjectMap.Remove(playerId);
            }
        }

        public void GoToLobby()
        {
            ChangeView(lobbyMenu);
            bool isServer = NetworkManager.Singleton.IsServer;
            lobbyDelete.gameObject.SetActive(isServer);
            lobbyDisconnect.gameObject.SetActive(!isServer);
            startGame.gameObject.SetActive(isServer);
        }

        public void ChangeStatus(string status = "", Color color = default)
        {
            if(SceneManager.GetActiveScene().name != "Lobby")
            {
                return;
            }
            if (color == default)
                color = Color.white;
            statusText.text = status;
            statusText.color = color;
        }
    }
}
