using Unity.Netcode;
using UnityEngine;
using _Scripts.Player;
using _Scripts;


public class TeamSelector : NetworkBehaviour
{
    Player playerManager;

    public void OnTeam1ButtonClickRpc()
    {
        Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} clicked Team 1 button");
        playerManager = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        AssignPlayerToTeamRpc(NetworkManager.Singleton.LocalClientId, 1);
    }

    public void OnTeam2ButtonClickRpc()
    {
        playerManager = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        AssignPlayerToTeamRpc(NetworkManager.Singleton.LocalClientId, 2);
    }

    private void AssignPlayerToTeamRpc(ulong clientId, int teamId)
    {
        Debug.Log(playerManager);
        playerManager.AssignTeamRpc(clientId, teamId);
        Debug.Log($"Player {clientId} assigned to team {teamId}");
    }

    public void StartGame()
    {
        LobbyManager.Instance.teamSelected = true;
        LobbyManager.Instance.StartGameRpc();
    }
}
