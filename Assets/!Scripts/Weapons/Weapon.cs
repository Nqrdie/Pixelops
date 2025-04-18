using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : ScriptableObject
{
    public int maxAmmo;
    public int reserveAmmo;
    public int damage;
    public float fireRate;
    public float reloadTime;
    public Mesh weaponMesh;
    public int maxReserveAmmo;
}
