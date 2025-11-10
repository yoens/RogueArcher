using UnityEngine;

[CreateAssetMenu(menuName = "RogueArcher/Weapon", fileName = "WeaponSO_")]
public class WeaponSO : ScriptableObject
{
    public string weaponName = "Basic Bow";
    public GameObject projectilePrefab;
    public float fireCooldown = 0.25f;
    public float projectileSpeed = 12f;
    public int baseDamage = 1;
}
