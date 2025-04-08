using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Weapon[] weapons;
    [SerializeField] public int currentWeaponIndex = 0;
    [SerializeField] private GameObject weaponHolder;

    public Dictionary<Weapon, (int currentAmmo, int reserveAmmo)> ammoValues = new();

    private void Start()
    {
        ammoValues.Add(weapons[0], (weapons[0].maxAmmo, weapons[0].reserveAmmo));
        ammoValues.Add(weapons[1], (weapons[1].maxAmmo, weapons[1].reserveAmmo));
        ammoValues.Add(weapons[2], (weapons[2].maxAmmo, weapons[2].reserveAmmo));
    }

    [Rpc(SendTo.Everyone)]
    public void SwapMesh(Mesh mesh)
    {
        weaponHolder.GetComponent<MeshFilter>().mesh = mesh;
    }

    public Weapon ReturnWeapon(int i)
    {
        return weapons[i];
    }
}
