using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Newtonsoft.Json.Bson;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private Weapon[] weapons;
    [SerializeField] public int currentWeaponIndex = 0;
    [SerializeField] public GameObject weaponHolder;
    [SerializeField] private GameObject muzzleFlash;
    private PlayerMovement playerMovement;
    private Mesh weaponMesh;

    public Dictionary<Weapon, (int currentAmmo, int reserveAmmo)> ammoValues = new();

    private WeaponHandler weaponHandler;

    private void Start()
    {
        playerMovement = transform.parent.GetComponent<PlayerMovement>();
        weaponHandler = gameObject.GetComponent<WeaponHandler>();
        if (!playerMovement.IsOwner)
        {
            enabled = false;
            return;
        }
        ammoValues.Add(weapons[0], (weapons[0].maxAmmo, weapons[0].reserveAmmo));
        ammoValues.Add(weapons[1], (weapons[1].maxAmmo, weapons[1].reserveAmmo));
        ammoValues.Add(weapons[2], (weapons[2].maxAmmo, weapons[2].reserveAmmo));
    }

    [Rpc(SendTo.Everyone)]
    public void SwapMeshRpc(int meshIndex)
    {
        weaponMesh = weapons[meshIndex].weaponMesh;
        weaponHolder.GetComponent<MeshFilter>().mesh = weaponMesh;
        switch (meshIndex)
        {
            case 0:
                weaponHolder.transform.localPosition = new Vector3(0.17f, 0.02f, 0.7f);
                muzzleFlash.transform.localPosition = new Vector3(-0.4f, -0.01f, -0.04f);
                break;

            case 1:
                weaponHolder.transform.localPosition = new Vector3(0.17f, 0.02f, 0.6f);
                muzzleFlash.transform.localPosition = new Vector3(-0.13f, 0.001f, -0.055f);
                break;

            case 2:
                weaponHolder.transform.localPosition = new Vector3(0.17f, 0.15f, 1.17f);
                muzzleFlash.transform.localPosition = new Vector3(-0.61f, -0.01f, 0f);
                break;
        }

    }

    public void SwapMesh(int meshIndex)
    {
        weaponMesh = weapons[meshIndex].weaponMesh;
        SwapMeshRpc(meshIndex);
    }


    public Weapon ReturnWeapon(int i)
    {
        return weapons[i];
    }

    [Rpc(SendTo.Everyone)]
    public void ResetWeaponsRpc()
    {
        weaponHandler.ResetValues();
        ammoValues.Clear();
        for (int i = 0; i < weapons.Length; i++)
        {
            ammoValues.Add(weapons[i], (weapons[i].maxAmmo, weapons[i].reserveAmmo));
        }
    }
}
