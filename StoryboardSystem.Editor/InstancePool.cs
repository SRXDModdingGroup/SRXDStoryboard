using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryboardSystem.Editor; 

public class InstancePool<T> : IReadOnlyList<T> where T : MonoBehaviour {
    public int Count { get; private set; }

    private Transform root;
    private GameObject prefab;
    private List<T> instances;

    public InstancePool(Transform root, GameObject prefab) {
        this.root = root;
        this.prefab = prefab;
        instances = new List<T>();
    }

    public T this[int index] {
        get {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException();

            return instances[index];
        }
    }

    public void SetCount(int count) => SetCount(count, (_, _) => { });

    public void SetCount(int count, Action<T, int> init) {
        Count = count;
        
        for (int i = 0; i < count || i < instances.Count; i++) {
            T instance;

            if (i < instances.Count)
                instance = instances[i];
            else {
                instance = Object.Instantiate(prefab, root).GetComponent<T>();
                instances.Add(instance);
                init(instance, i);
            }

            if (i < count) {
                instance.gameObject.SetActive(true);
                instance.transform.SetParent(root);
            }
            else {
                instance.gameObject.SetActive(false);
                instance.transform.SetParent(null);
            }
        }
    }

    public IEnumerator<T> GetEnumerator() {
        for (int i = 0; i < Count; i++)
            yield return instances[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}