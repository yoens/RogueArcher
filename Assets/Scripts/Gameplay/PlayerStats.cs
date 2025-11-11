using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float moveSpeedBonus = 0f;
    public float fireRateBonus = 0f;
    public int pierceBonus = 0;
    public int damageBonus = 0;
    public int maxHPBonus = 0;
    public float projectileSpeedBonus = 0f;
    public void AddDamage(int v) => damageBonus += v;

    public void AddMaxHP(int v)
    {
        maxHPBonus += v;
        // 실제로 플레이어 Health에도 반영
        if (TryGetComponent<Health>(out var h))
        {
            h.maxHP += v;
            h.currentHP += v;   // 체력도 채워주기
        }
    }

    public void AddProjectileSpeed(float v) => projectileSpeedBonus += v;

    public void AddMoveSpeed(float amount) => moveSpeedBonus += amount;
    public void AddFireRate(float amount) => fireRateBonus += amount;
    public void AddPierce(int amount) => pierceBonus += amount;
}
