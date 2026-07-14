using UnityEngine;

public enum UpgradeType
{
    MoveSpeed,
    FireRate,
    Pierce,
    Damage,
    MaxHP,
    ProjectileSpeed
}

[CreateAssetMenu(menuName = "RogueArcher/Upgrade", fileName = "UpgradeSO_")]
public class UpgradeSO : ScriptableObject
{
    public string displayName = "Move Speed +1";
    [TextArea] public string description;
    public UpgradeType type;
    public float floatValue;    
    public int intValue;       
}
