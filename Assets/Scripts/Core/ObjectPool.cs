using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    // ===== 측정용 토글 (측정 끝나면 삭제) =====
    public static bool BypassPool = false;
    // ==========================================
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
        if (BypassPool)
        {
            // 풀 미사용 시뮬레이션: 매번 새로 생성
            return Object.Instantiate(_prefab, pos, rot, _root);
        }        
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
        if (BypassPool)
        {
            // 풀 미사용 시뮬레이션: 매번 파괴
            Object.Destroy(obj.gameObject);
            return;
        }        
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
