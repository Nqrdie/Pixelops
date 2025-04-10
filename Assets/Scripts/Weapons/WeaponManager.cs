using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Newtonsoft.Json.Bson;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private Weapon[] weapons;
    [SerializeField] public int currentWeaponIndex = 0;
    [SerializeField] private GameObject weaponHolder;
    private PlayerMovement playerMovement;
    private Mesh weaponMesh;

    public Dictionary<Weapon, (int currentAmmo, int reserveAmmo)> ammoValues = new();

    private WeaponHandler weaponHandler;

    private void Start()
    {
        playerMovement = transform.parent.GetComponent<PlayerMovement>();
        if (!playerMovement.IsOwner)
        {
            enabled = false;
            return;
        }
        ammoValues.Add(weapons[0], (weapons[0].maxAmmo, weapons[0].reserveAmmo));
        ammoValues.Add(weapons[1], (weapons[1].maxAmmo, weapons[1].reserveAmmo));
        ammoValues.Add(weapons[2], (weapons[2].maxAmmo, weapons[2].reserveAmmo));

        weaponHandler = GetComponent<WeaponHandler>();
    }

    [Rpc(SendTo.Everyone)]
    public void SwapMeshRpc(int meshIndex)
    {
        weaponMesh = weapons[meshIndex].weaponMesh;
        weaponHolder.GetComponent<MeshFilter>().mesh = weaponMesh;
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

    public void ResetWeapons()
    {
        weaponHandler.ResetValues();
        ammoValues.Clear();
        ammoValues.Add(weapons[0], (weapons[0].maxAmmo, weapons[0].reserveAmmo));
        ammoValues.Add(weapons[1], (weapons[1].maxAmmo, weapons[1].reserveAmmo));
        ammoValues.Add(weapons[2], (weapons[2].maxAmmo, weapons[2].reserveAmmo));
        weaponHandler.SwapWeapon(currentWeaponIndex);
    }
}
