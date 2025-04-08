using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

namespace _Scripts.Player
{
    public class Player : NetworkBehaviour
    {
        [SerializeField]
        private NetworkObject inGamePlayerPrefab;
        private string _lobbyPlayerId;
        public int team;

        private void Start()
        {
            if (!IsOwner)
            {
                return;
            }
            _lobbyPlayerId = AuthenticationService.Instance.PlayerId;
            ParentThisRpc();
            SendUlongIdToServerRpc(_lobbyPlayerId);
            // NetworkManager.SceneManager.OnLoadComplete += GameManager.Instance.SceneManagerOnOnLoadComplete;
        }

        [Rpc(SendTo.Server)]
        private void ParentThisRpc()
        {
            transform.parent = GameObject.Find("PlayerParent").transform;
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
            Debug.Log($"Player {playerId} joined.");
            LobbyManager.Instance.ConvertedIds.Add(ulongId, playerId);
        }

        public void SpawnInThisPlayer()
        {
            NetworkObject player = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
                inGamePlayerPrefab,
                NetworkObject.OwnerClientId,
                isPlayerObject: true
            );

            SetTeamRpc(player.OwnerClientId);
            player.GetComponent<PlayerTeamManager>().team = team;
            Debug.Log($"Player {LobbyUtil.GetName(NetworkObject.OwnerClientId)} {team} spawned in.");
            Debug.Log($"Spawning in player {LobbyUtil.GetName(NetworkObject.OwnerClientId)}");
        }

        [Rpc(SendTo.Everyone)]  
        private void SetTeamRpc(ulong playerId)
        {
            NetworkObject player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId);
            player.GetComponent<PlayerTeamManager>().team = team;
        }

        public void OnLeaving()
        {
            Destroy(gameObject);
        }

        [Rpc(SendTo.Everyone)]
        public void AssignTeamRpc(ulong clientId, int teamId)
        {
             team = teamId;
        }


    }
}
