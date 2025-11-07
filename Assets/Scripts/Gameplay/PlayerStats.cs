using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float moveSpeedBonus = 0f;
    public float fireRateBonus = 0f;
    public int pierceBonus = 0;

    public void AddMoveSpeed(float amount) => moveSpeedBonus += amount;
    public void AddFireRate(float amount) => fireRateBonus += amount;
    public void AddPierce(int amount) => pierceBonus += amount;
}
