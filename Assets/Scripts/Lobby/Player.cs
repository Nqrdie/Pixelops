using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

namespace _Scripts.Player
{
    public class Player : NetworkBehaviour
    {

        /// <summary>
        /// Made by Jesper Heese
        /// I only edited this to make it work in my game
        /// </summary>
        /// 

        [SerializeField]
        private NetworkObject inGamePlayerPrefab;
        private string _lobbyPlayerId;
        public int team;
        private ulong playerId;

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
            LobbyManager.Instance.ConvertedIds.Add(ulongId, playerId);
        }

        public void SpawnInThisPlayer()
        {
            NetworkObject player = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
                inGamePlayerPrefab,
                NetworkObject.OwnerClientId,
                isPlayerObject: true
            );
            SetTeamRpc();
        }

        [Rpc(SendTo.Everyone)]  
        private void SetTeamRpc()
        {
            NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<PlayerTeamManager>().team = team;
        }

        public void OnLeaving()
        {
            Destroy(gameObject);
        }

        [Rpc(SendTo.Everyone)]
        public void AssignTeamRpc(ulong clientId, int teamId)
        {
             team = teamId;
            playerId = clientId;
        }


    }
}
