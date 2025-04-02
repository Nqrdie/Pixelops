using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Weapons : NetworkBehaviour
{

    [Header("Weapon stats")]
    [SerializeField] protected int maxAmmo;
    [SerializeField] protected int currentAmmo;
    [SerializeField] protected int reserveAmmo;
    [SerializeField] protected int damage;
    [SerializeField] protected int recoil;
    [SerializeField] protected int damageFalloff;
    [SerializeField] protected float fireRate;
    [SerializeField] protected float speedModifier;
    protected float nextFire;
    [SerializeField] protected RawImage hitmarkerImage;

    protected Camera playerCam;
    protected InputHandler input;

    private void Start()
    {
        input = FindFirstObjectByType<InputHandler>();
        playerCam = Camera.main;

        if(!IsOwner)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if(input.reloadTriggered)
            Reload();
        if(input.shootTriggered && Time.time > nextFire && currentAmmo > 0)
        {
            Shoot();
        }
        
        if(SceneManager.GetActiveScene().name == "Main" && hitmarkerImage == null)
        {
            hitmarkerImage = GameObject.FindWithTag("Hitmarker").GetComponent<RawImage>();
        }
    }

    protected void Reload()
    {
        int ammoNeeded = maxAmmo - currentAmmo;
        if (reserveAmmo >= reserveAmmo - ammoNeeded)
        {
            reserveAmmo -= ammoNeeded;
            currentAmmo += ammoNeeded;
        }
        else
        {
            currentAmmo += reserveAmmo;
            reserveAmmo = 0;
        }

        if (reserveAmmo == 0)
        {
            Debug.Log("Out of ammo");
        }
    }

    private void SwapWeapon()
    {

    }

    protected void Shoot()
    {
        nextFire = Time.time + fireRate;

        currentAmmo--;
        Vector3 rayOrigin = playerCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, playerCam.transform.forward, out hit))
        {
            GameObject target = hit.collider.transform.gameObject.CompareTag("Player") ? hit.collider.transform.parent.gameObject : null;
            if (target != null)
            {
                target.GetComponent<PlayerHealth>().TakeDamage(damage);
                Debug.Log("Shot " + target.name + " for " + damage + " Damage");
                StartCoroutine(hitmarker());
            }
        }
    }


    protected IEnumerator hitmarker()
    {
        hitmarkerImage.enabled = true;
        yield return new WaitForSeconds(0.1f);
        hitmarkerImage.enabled = false;
    }
}
