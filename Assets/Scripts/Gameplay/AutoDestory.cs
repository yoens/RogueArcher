using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float life = 0.5f;

    void Start()
    {
        Destroy(gameObject, life);
    }
}
