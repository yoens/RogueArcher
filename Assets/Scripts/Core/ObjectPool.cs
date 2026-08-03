using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    public static bool BypassPool = false;   // ← [추가 ①] 스위치

    readonly Queue<T> _pool = new Queue<T>();
    readonly T _prefab;
    readonly Transform _root;

    public ObjectPool(T prefab, int prewarm = 0, Transform root = null)
    {
        _prefab = prefab;
        _root = root;
        if (BypassPool) return;              // ← [추가 ②] 풀 끄면 prewarm 스킵
        for (int i = 0; i < prewarm; i++)
        {
            var obj = Object.Instantiate(_prefab, _root);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public T Get(Vector3 pos, Quaternion rot)
    {
        T obj;
        if (BypassPool || _pool.Count == 0)  // ← [수정 ③] 조건에 BypassPool 추가
            obj = Object.Instantiate(_prefab, _root);
        else
            obj = _pool.Dequeue();

        obj.transform.SetPositionAndRotation(pos, rot);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        if (BypassPool)                      // ← [추가 ④] 풀 끄면 파괴
        {
            Object.Destroy(obj.gameObject);
            return;
        }
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}