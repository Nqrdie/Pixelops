using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerTeamManager : NetworkBehaviour
{
    public int team;
    [SerializeField] private GameObject[] playerModels;

    private void Start()
    {
        AssignLayerRpc();
        StartCoroutine(SetupPlayerLocations());
    }

    [Rpc(SendTo.Everyone)]
    private void AssignLayerRpc()
    {
        // Sets the layers to disable friendly fire
        switch (team)
        {
            case 1:
                gameObject.layer = LayerMask.NameToLayer("Team1");
                playerModels[0].SetActive(true);
                playerModels[1].SetActive(false);
                break;

            case 2:
                gameObject.layer = LayerMask.NameToLayer("Team2");
                playerModels[1].SetActive(true);
                playerModels[0].SetActive(false);
                break;
        }
    }

    public IEnumerator SetupPlayerLocations()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Vector3 spawnPosition = team == 1
            ? new Vector3(Random.Range(-19.5f, -30f), 1, Random.Range(36f, 41))
            : new Vector3(Random.Range(-18.6f, -13f), 1, Random.Range(-7f, -16.75f));

        yield return new WaitForFixedUpdate();
        Debug.Log($"Team {team} Spawn {spawnPosition}");
        transform.position = spawnPosition;

            rb.isKinematic = false; 
            rb.linearVelocity = Vector3.zero; 
    }

    public int GetTeam()
    {
        return team;
    }
}
