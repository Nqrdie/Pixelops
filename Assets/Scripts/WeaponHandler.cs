using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class WeaponHandler : MonoBehaviour
{
    private WeaponManager weaponManager;
    [Header("Weapon stats")]
    protected int maxAmmo;
    protected int currentAmmo = 0;
    protected int reserveAmmo = 0;
    protected int damage;
    protected int recoil;
    protected int damageFalloff;
    protected float fireRate;
    protected float speedModifier;
    protected float reloadTime;
    protected Mesh weaponMesh;
    protected TextMeshProUGUI ammoText;
    protected float nextFire;
    protected RawImage hitmarkerImage;
    private Weapon currentWeapon;
    private LayerMask layerMask;
    private PlayerTeamManager playerTeamManager;

    protected Camera playerCam;
    protected InputHandler input;

    protected PlayerMovement playerMovement;

    private Coroutine reload;

    private void Start()
    {
        playerMovement = transform.parent.GetComponent<PlayerMovement>();
        input = FindFirstObjectByType<InputHandler>();
        playerCam = Camera.main;

        if(!playerMovement.IsOwner)
        {
            enabled = false;
            return;
        }

        weaponManager = FindFirstObjectByType<WeaponManager>();
        currentWeapon = weaponManager.ReturnWeapon(0);
        currentAmmo = weaponManager.ammoValues[weaponManager.ReturnWeapon(0)].currentAmmo;
        reserveAmmo = weaponManager.ammoValues[weaponManager.ReturnWeapon(0)].reserveAmmo;
        SwapWeapon(0);
        playerTeamManager = transform.parent.GetComponent<PlayerTeamManager>();

        layerMask = playerTeamManager.GetTeam() == 1 ? LayerMask.GetMask("Team2") : LayerMask.GetMask("Team1");
    }
    private void Update()
    {
        if (input.reloadTriggered)
        {
            if (reload == null)
            {
                Debug.Log("Reloading");
                reload = StartCoroutine(Reload());
            }
            else
            {
                Debug.Log("Already reloading");
            }
        }

        if(input.shootTriggered && Time.time > nextFire && currentAmmo > 0)
        {
            Shoot();
        }
        
        if (SceneManager.GetActiveScene().name == "Main" && hitmarkerImage == null && ammoText == null)
        {
            hitmarkerImage = GameObject.FindWithTag("Hitmarker").GetComponent<RawImage>();
            ammoText = GameObject.FindWithTag("AmmoText").GetComponent<TextMeshProUGUI>();
        }
        
        if(input.switchWeaponAction.triggered)
        {

            switch (input.switchWeaponIndex)
            {
                case 0:
                    SwapWeapon(0);
                    break;
                case 1:
                    SwapWeapon(1);
                    break;
                case 2:
                    SwapWeapon(2);
                    break;
            }
        }

        ammoText.text = currentAmmo + " / " + reserveAmmo;
    }

    protected IEnumerator Reload()
    {
        Debug.Log("Reloading2");
        int ammoNeeded = maxAmmo - currentAmmo;
        if (reserveAmmo >= reserveAmmo - ammoNeeded)
        {
            yield return new WaitForSeconds(reloadTime);
            reserveAmmo -= ammoNeeded;
            currentAmmo += ammoNeeded;
        }
        else
        {
            yield return new WaitForSeconds(reloadTime);
            currentAmmo += reserveAmmo;
            reserveAmmo = 0;
        }

        if (reserveAmmo == 0)
        {
            Debug.Log("Out of ammo");
        }

        Debug.Log("Reload set null");
        reload = null;
    }

    private void SwapWeapon(int i)
    {
        if (reload != null)
        {
            StopCoroutine(reload);
            reload = null;
        }

        weaponManager.ammoValues[currentWeapon] = (currentAmmo, reserveAmmo);
        currentWeapon = weaponManager.ReturnWeapon(i);

        currentAmmo = weaponManager.ammoValues[currentWeapon].currentAmmo;
        reserveAmmo = weaponManager.ammoValues[currentWeapon].reserveAmmo;

        maxAmmo = currentWeapon.maxAmmo;
        damage = currentWeapon.damage;
        recoil = currentWeapon.recoil;
        damageFalloff = currentWeapon.damageFalloff;
        fireRate = currentWeapon.fireRate;
        speedModifier = currentWeapon.speedModifier;
        reloadTime = currentWeapon.reloadTime;
        weaponMesh = currentWeapon.weaponMesh;

        weaponManager.currentWeaponIndex = i; 
        
        weaponManager.SwapMesh(weaponMesh);
    }

    protected void Shoot()
    {
        nextFire = Time.time + fireRate;

        currentAmmo--;
        Vector3 rayOrigin = playerCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, playerCam.transform.forward, out hit, layerMask))
        {
            GameObject target = hit.collider.transform.gameObject.CompareTag("Player") ? hit.collider.transform.parent.gameObject : null;
            if (target != null)
            {
                target.GetComponent<PlayerHealth>().TakeDamage(damage);
                Debug.Log("Shot " + target.name + " for " + damage + " Damage");
                StartCoroutine(Hitmarker());
            }
        }
    }


    protected IEnumerator Hitmarker()
    {
        hitmarkerImage.enabled = true;
        yield return new WaitForSeconds(0.1f);
        hitmarkerImage.enabled = false;
    }
}
