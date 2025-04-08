using NUnit.Framework.Constraints;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class PlayerTeamManager : NetworkBehaviour
{
    public int team;

    private void Start()
    {
        AssignLayerRpc();
        SetupPlayerLocations();
    }

    [Rpc(SendTo.Everyone)]
    private void AssignLayerRpc()
    {
        switch (team)
        {
            case 1:
                transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
                gameObject.layer = LayerMask.NameToLayer("Team1");
                break;

            case 2:
                transform.GetChild(0).GetComponent<Renderer>().material.color = Color.blue;
                gameObject.layer = LayerMask.NameToLayer("Team2");
                break;
        }
    }
    private void SetupPlayerLocations()
    {
        if (team == 1)
        {
            transform.position = new Vector3(Random.Range(14f, -14.1f), 1, -25);
        }
        else if (team == 2)
        {
            transform.position = new Vector3(Random.Range(14f, -14.1f), 1, 25);
        }
    }

    public int GetTeam()
    {
        return team;
    }
}
