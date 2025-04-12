using Unity.Netcode;
using UnityEngine;

public class Stats : NetworkBehaviour
{
    public int kills;
    public int deaths;
    private Scoreboard scoreboard;
    private void Start()
    {
        scoreboard = FindFirstObjectByType<Scoreboard>();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    public void AddDeathRpc()
    {
        deaths++;
        scoreboard.UpdateScoreboardRpc();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    public void AddKillRpc()
    {
        kills++;
        scoreboard.UpdateScoreboardRpc();
    }
}