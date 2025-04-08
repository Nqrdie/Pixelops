using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : ScriptableObject
{
    public int maxAmmo;
    public int reserveAmmo;
    public int damage;
    public int recoil;
    public int damageFalloff;
    public float fireRate;
    public float speedModifier;
    public float reloadTime;
    public Mesh weaponMesh;
}
