using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    public static bool BypassPool = false;

    readonly Queue<T> _pool = new Queue<T>();
    readonly T _prefab;
    readonly Transform _root;
    readonly bool _bypassPool;

    public int CreatedCount { get; private set; }
    public int ReusedCount { get; private set; }

    public int InactiveCount => _pool.Count;
    public bool IsBypassing => _bypassPool;

    public ObjectPool(
        T prefab,
        int prewarm = 0,
        Transform root = null)
    {
        _prefab = prefab;
        _root = root;

        // 이 풀의 사용 모드를 생성 시 확정
        _bypassPool = BypassPool;

        if (_bypassPool) return;

        for (int i = 0; i < prewarm; i++)
        {
            var obj = Object.Instantiate(_prefab, _root);
            CreatedCount++;

            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public T Get(Vector3 pos, Quaternion rot)
    {
        T obj;

        if (_bypassPool || _pool.Count == 0)
        {
            obj = Object.Instantiate(_prefab, _root);
            CreatedCount++;
        }
        else
        {
            obj = _pool.Dequeue();
            ReusedCount++;
        }

        obj.transform.SetPositionAndRotation(pos, rot);
        obj.gameObject.SetActive(true);

        return obj;
    }

    public void Return(T obj)
    {
        if (_bypassPool)
        {
            Object.Destroy(obj.gameObject);
            return;
        }

        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}