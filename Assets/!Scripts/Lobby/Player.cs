using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts
{
    public class Player : NetworkBehaviour
    {


        [SerializeField]
        private NetworkObject inGamePlayerPrefab;
        public string _lobbyPlayerId;
        public PlayerTeamManager playerTeamManager;
        private ulong playerId;

        public int team;
        private NetworkObject inGamePlayer;

        private NetworkVariable<ulong> inGamePlayerId = new NetworkVariable<ulong>();

        public static List<ulong> playerIds = new List<ulong>();

        private LobbyUi lobbyUi;

        public UnityEvent<int> OnTeamChanged = new UnityEvent<int>(); // UnityEvent for team changes

        public void SetTeam(int newTeam)
        {
            if (team != newTeam)
            {
                team = newTeam;
                OnTeamChanged?.Invoke(team); // Ensure this is being called
            }
        }

        private void Start()
        {
            if (!IsOwner)
            {
                return;
            }
            _lobbyPlayerId = AuthenticationService.Instance.PlayerId;
            ParentThisRpc();
            SendUlongIdToServerRpc(_lobbyPlayerId);
            lobbyUi = FindAnyObjectByType<LobbyUi>();

            SetTeam(0);
            // Subscribe to the OnTeamChanged event
            OnTeamChanged.AddListener(newTeam =>
            {
                // Notify all clients about the parent change
                lobbyUi.UpdatePlayerParentRpc(_lobbyPlayerId, newTeam);
            });
            if (IsOwner)
            {
                _lobbyPlayerId = AuthenticationService.Instance.PlayerId;
            }
        }

        private void Update()
        {
            if (!IsOwner || inGamePlayer != null) return;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(inGamePlayerId.Value, out var networkObject))
            {
                inGamePlayer = networkObject;
            }
        }

        [Rpc(SendTo.Server)]
        private void ParentThisRpc()
        {
            // Apply the parenting change locally for the owner
            transform.parent = GameObject.Find("PlayerParent").transform;


            // Notify all clients about the parenting change
            LobbyManager.Instance.CheckForPlayersRpc();
            DDolThisRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void DDolThisRpc()
        {
            DontDestroyOnLoad(transform.parent.gameObject);
        }

        [Rpc(SendTo.Everyone)]
        private void SendUlongIdToServerRpc(string playerId)
        {
            _lobbyPlayerId = playerId;
            ulong ulongId = NetworkObject.OwnerClientId;
            LobbyManager.Instance.AddConvertedId(ulongId, playerId);
        }

        public void SpawnInThisPlayer()
        {
            if (IsServer)
            {
                var spawnedPlayer = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
                    inGamePlayerPrefab,
                    NetworkObject.OwnerClientId,
                    isPlayerObject: true
                );
                inGamePlayer = spawnedPlayer;
                var player = LobbyManager.Instance.Lobby.Players.Find(w => w.Id == _lobbyPlayerId);
                if (player != null)
                {
                    SetNameRpc(spawnedPlayer.OwnerClientId, player.Data["PlayerName"].Value);
                    SetTeamRpc(spawnedPlayer.OwnerClientId, team);
                }
            }
        }

        [Rpc(SendTo.Everyone)]
        private void SetNameRpc(ulong playerId, string playerName)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
            {
                client.PlayerObject.name = playerName;
            }
        }

        [Rpc(SendTo.Everyone)]
        private void SetTeamRpc(ulong playerId, int newTeam)
        {
            if (team != newTeam)
            {
                team = newTeam;
                OnTeamChanged?.Invoke(team); 

                LobbyUi lobbyUi = FindAnyObjectByType<LobbyUi>();
                if (lobbyUi != null)
                {
                    LobbyManager.Instance.UpdatePlayerTeamRpc(_lobbyPlayerId, newTeam);
                }
            }
            if(NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
            {
                client.PlayerObject.GetComponent<PlayerTeamManager>().team = team;
            }
        }

        public void OnLeaving()
        {
            Destroy(gameObject);
        }

        [Rpc(SendTo.Everyone)]
        public void AssignTeamRpc(ulong clientId, int teamId)
        {
            SetTeam(teamId);
            playerId = clientId;
        }


        public bool IsTeamAssigned()
        {
            if(team == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
