using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using TMPro;
public class GameSystem : NetworkBehaviour
{
    public List<GameObject> playerList = new();
    public List<GameObject> playerListTeam1 = new();
    public List<GameObject> playerListTeam2 = new();
    public List<GameObject> DeadPlayerListTeam1 = new();
    public List<GameObject> DeadPlayerListTeam2 = new();
    private TextMeshProUGUI roundWonText;
    public static GameSystem Instance { get; private set; } = null;
    [SerializeField] private GameObject settingsMenu;
    public bool settingsTriggered = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        roundWonText = GameObject.FindGameObjectWithTag("RoundEndText").GetComponent<TextMeshProUGUI>();
        roundWonText.gameObject.SetActive(false);
        StartCoroutine(LateStart());
    }

    private void Update()
    {
        if(InputHandler.Instance.settingsTriggered)
        {
            settingsMenu.SetActive(true);
            settingsTriggered = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            settingsMenu.SetActive(false);
            settingsTriggered = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    private IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1f);
        GetActivePlayers();
    }

    private void GetActivePlayers()
    {
        foreach (KeyValuePair<ulong, NetworkClient> client in NetworkManager.Singleton.ConnectedClients)
        {
            GameObject player = client.Value.PlayerObject.gameObject;
            PlayerTeamManager playerTeamManager = player.GetComponent<PlayerTeamManager>();

            playerList.Add(player);

            if (playerTeamManager.GetTeam() == 1)
                playerListTeam1.Add(player);
            else if (playerTeamManager.GetTeam() == 2)
                playerListTeam2.Add(player);
        }
    }

    [Rpc(SendTo.Everyone)]  
    private void KillPlayerRpc()
    {
        foreach (GameObject player in playerList)
        {
            int playerHealth = player.GetComponent<PlayerHealth>().health;

            if (playerHealth <= 0)
            {
                player.SetActive(false);
            }
        }
    }
    public void CheckPlayerDeaths()
    {

        foreach (GameObject player in playerList)
        {
            PlayerTeamManager playerTeamManager = player.GetComponent<PlayerTeamManager>();

            KillPlayerRpc();
            if (playerTeamManager.GetTeam() == 1 && !player.activeInHierarchy)
            {
                DeadPlayerListTeam1.Add(player);
            }
            else if (playerTeamManager.GetTeam() == 2 && !player.activeInHierarchy)
            {
                DeadPlayerListTeam2.Add(player);
            }

        }

        if (DeadPlayerListTeam1.Count == playerListTeam1.Count || DeadPlayerListTeam2.Count == playerListTeam2.Count)
        {
            StartCoroutine(ResetTimer());
            //ResetRoundRpc();
        }
    }

    private IEnumerator ResetTimer()
    {
        roundWonText.text = DeadPlayerListTeam1.Count == playerListTeam1.Count ? "Team 2 Won the round!" : "Team 1 Won the round!";
        roundWonText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        roundWonText.gameObject.SetActive(false);
        ResetRoundRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ResetRoundRpc()
    {
        foreach (GameObject player in playerList)
        {
            player.SetActive(true);

            PlayerTeamManager playerTeamManager = player.GetComponent<PlayerTeamManager>();
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            Transform gunTransform = player.transform.Find("Gun");

            if (gunTransform == null)
            {
                Debug.LogError($"Gun object not found for player: {player.name}");
                continue;
            }

            WeaponManager weaponManager = gunTransform.GetComponent<WeaponManager>();
            if (weaponManager == null)
            {
                Debug.LogError($"WeaponManager component not found on Gun object for player: {player.name}");
                continue;
            }

            weaponManager.ResetWeaponsRpc();
            playerHealth.ResetHurtFlash();
            playerTeamManager.SetupPlayerLocations();
            playerHealth.health = playerHealth.maxHealth;
            DeadPlayerListTeam1.Remove(player);
            DeadPlayerListTeam2.Remove(player);
        }
    }

}
