using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    private GameSystem gameSystem;
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private GameObject scoreboardTeam1;
    [SerializeField] private GameObject scoreboardTeam2;
    [SerializeField] private GameObject playerScorePrefab;

    private Dictionary<GameObject, GameObject> matchPlayerToScore = new();
    private void Start()
    {
        gameSystem = FindAnyObjectByType<GameSystem>();
        StartCoroutine(LateStart());
    }

    private void Update()
    {
        if(InputHandler.Instance.scoreboardTriggered)
        {
            scoreboard.SetActive(true);
        }
        else
        {
            scoreboard.SetActive(false);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateScoreboardRpc()
    {
        foreach (var player in gameSystem.playerListTeam1)
        {
            if (matchPlayerToScore.TryGetValue(player, out var playerScore))
            {
                var playerStats = player.GetComponent<Stats>();
                playerScore.transform.Find("Kills").GetChild(0).GetComponent<TextMeshProUGUI>().text = playerStats.kills.ToString();
                playerScore.transform.Find("Deaths").GetChild(0).GetComponent<TextMeshProUGUI>().text = playerStats.deaths.ToString();
                playerScore.transform.Find("Playername").GetComponent<TextMeshProUGUI>().text = player.name;
            }
        }

        foreach (var player in gameSystem.playerListTeam2)
        {
            if (matchPlayerToScore.TryGetValue(player, out var playerScore))
            {
                var playerStats = player.GetComponent<Stats>();
                playerScore.transform.Find("Kills").GetChild(0).GetComponent<TextMeshProUGUI>().text = playerStats.kills.ToString();
                playerScore.transform.Find("Deaths").GetChild(0).GetComponent<TextMeshProUGUI>().text = playerStats.deaths.ToString();
                playerScore.transform.Find("Playername").GetComponent<TextMeshProUGUI>().text = player.name;
            }
        }
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForSeconds(2f);
        InstantiateScoreBoard();
    }
    private void InstantiateScoreBoard()
    {
        foreach (var player in gameSystem.playerListTeam1)
        {
            var playerScore = Instantiate(playerScorePrefab, scoreboardTeam1.transform);

            matchPlayerToScore[player] = playerScore;
        }

        foreach (var player in gameSystem.playerListTeam2)
        {
            var playerScore = Instantiate(playerScorePrefab, scoreboardTeam2.transform);
            matchPlayerToScore[player] = playerScore;
        }
        UpdateScoreboardRpc();
    }
}
