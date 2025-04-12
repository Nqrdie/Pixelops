using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHealth : NetworkBehaviour
{
    public int health = 100;
    public int maxHealth = 100;
    private GameSystem gameSystem;
    private TextMeshProUGUI healthText;
    private GameObject hurtFlash;
    private Stats playerStats;

    private void Start()
    {
        if(!IsOwner)
        {
            enabled = false;
            return;
        }
        gameSystem = GameObject.FindFirstObjectByType<GameSystem>();
        healthText = GameObject.FindGameObjectWithTag("HealthText").GetComponent<TextMeshProUGUI>();
        hurtFlash = GameObject.FindGameObjectWithTag("HurtFlash").gameObject;
        hurtFlash.SetActive(false);
        playerStats = GetComponent<Stats>();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void TakeDamageRpc(int amount)
    {

        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);


        UpdateHealthClientRpc(health);
    }

    [ClientRpc]
    private void UpdateHealthClientRpc(int updatedHealth)
    {
        
        health = updatedHealth;

        if (hurtFlash != null)
        {
            StartCoroutine(HurtFlash());
        }
    }

    private IEnumerator HurtFlash()
    {
        if(hurtFlash == null)
            yield break;
        hurtFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        hurtFlash.SetActive(false);
    }
    private void Update()
    {
        healthText.text = "Health: " + health.ToString() + " / " + maxHealth;
        Die();
    }

    public void ResetHurtFlash()
    {
        if(hurtFlash == null)
            return;
        hurtFlash.SetActive(false);
    }

    public void Die()
    {
        if(health <= 0)
        {
            gameSystem.CheckPlayerDeaths();
            playerStats.AddDeathRpc();
        }
    }
}