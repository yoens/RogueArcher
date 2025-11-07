using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    readonly Queue<T> _pool = new Queue<T>();
    readonly T _prefab;
    readonly Transform _root;

    public ObjectPool(T prefab, int prewarm = 0, Transform root = null)
    {
        _prefab = prefab;
        _root = root;
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
        if (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(_prefab, _root);
        }

        obj.transform.SetPositionAndRotation(pos, rot);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
