using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int maxHP = 5;
    public int currentHP;
    public bool destroyOnDie = true;

    public event Action OnDie;
    public event Action OnDamaged;

    //  추가: HP 변경시 (cur,max) 알림
    public event Action<int, int> OnHPChanged;

    void Awake()
    {
        currentHP = maxHP;
        OnHPChanged?.Invoke(currentHP, maxHP); // 초기값 알림
    }

    public void Take(int dmg)
    {
        currentHP -= dmg;
        if (currentHP < 0) currentHP = 0;

        OnDamaged?.Invoke();
        OnHPChanged?.Invoke(currentHP, maxHP); //  체력 변경 알림

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        OnHPChanged?.Invoke(currentHP, maxHP); 
    }

    void Die()
    {
        OnDie?.Invoke();
        if (destroyOnDie)
            Destroy(gameObject);
    }
}
