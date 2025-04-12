using Unity.Netcode;
using UnityEngine;
using _Scripts;
using TMPro;
using Unity.Services.Matchmaker.Models;

namespace _Scripts
{

    public class TeamSelector : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI statusText;


        private void Update()
        {

        }
        public void OnTeam1ButtonClickRpc()
        {
            AssignPlayerToTeamRpc(NetworkManager.Singleton.LocalClientId, 1);
       
        }

        public void OnTeam2ButtonClickRpc()
        {
            AssignPlayerToTeamRpc(NetworkManager.Singleton.LocalClientId, 2);
            
        }

        [Rpc(SendTo.Everyone)]
        private void AssignPlayerToTeamRpc(ulong clientId, int teamId)
        {
            Player player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Player>();
            PlayerTeamManager fml = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerTeamManager>();
            player.SetTeam(teamId);

            string playerId = player._lobbyPlayerId;

            LobbyManager.Instance.UpdatePlayerTeamRpc(playerId, teamId);
        }



        public void StartGame()
        {
            foreach (var player in LobbyManager.Instance.players)
            {
                if (!player.GetComponent<Player>().IsTeamAssigned())
                {
                    LobbyUtil.Status("Not all players have selected a team.");
                    return;
                }
            }
            LobbyManager.Instance.StartGameRpc();
        }
    }
}
