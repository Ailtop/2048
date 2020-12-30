using System.Collections.Generic;
using UnityEngine;

public abstract class CollectionComponent<T> : MonoBehaviour where T : Component
{
    public static List<T> Instances { get; private set; } = new List<T>();

    protected T instance;

    protected virtual void Awake()
    {
        instance = this as T;
        Instances.Add(instance);
    }

    protected virtual void OnDestroy()
    {
        Instances.Remove(instance);
    }
}