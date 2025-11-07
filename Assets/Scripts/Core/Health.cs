using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int maxHP = 5;
    public int currentHP;
    public bool destroyOnDie = true;

    public event Action OnDie;

    void Awake() => currentHP = maxHP;

    public void Take(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    void Die()
    {
        OnDie?.Invoke();
        if (destroyOnDie)
            Destroy(gameObject);
    }
}