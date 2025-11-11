using UnityEngine;

public enum UpgradeType
{
    MoveSpeed,
    FireRate,
    Pierce,
    Damage,
    MaxHP,
    ProjectileSpeed
    // 나중에 Damage, MaxHP 이런 거 계속 추가 가능
}

[CreateAssetMenu(menuName = "RogueArcher/Upgrade", fileName = "UpgradeSO_")]
public class UpgradeSO : ScriptableObject
{
    public string displayName = "Move Speed +1";
    [TextArea] public string description;
    public UpgradeType type;
    public float floatValue;    // 이동속도 +1.0 이런 거
    public int intValue;        // 관통 +1 이런 거
}
