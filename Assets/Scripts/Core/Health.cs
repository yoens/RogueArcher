using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int maxHP = 5;
    public int currentHP;
    public bool destroyOnDie = true;

    public event Action OnDie;
    public event Action OnDamaged;

    
    public event Action<int, int> OnHPChanged;

    void Awake()
    {
        currentHP = maxHP;
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    public void Take(int dmg)
    {
        currentHP -= dmg;
        OnDamaged?.Invoke();
        OnHPChanged?.Invoke(currentHP, maxHP); 

        var fx = GetComponentInChildren<FlashEffect>(true);
        if (fx != null) fx.Flash();

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }
    public void RefreshHPEvent()
    {
        OnHPChanged?.Invoke(currentHP, maxHP);
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
