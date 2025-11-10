using UnityEngine;

[CreateAssetMenu(menuName = "RogueArcher/Enemy", fileName = "EnemySO_")]
public class EnemySO : ScriptableObject
{
    public string enemyName = "Chaser";
    public GameObject prefab;
    public int maxHP = 3;
    public float moveSpeed = 2f;
    public int contactDamage = 1;
}
