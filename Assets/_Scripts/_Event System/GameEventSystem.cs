using System;
using System.Collections.Generic;
using UnityEngine;

public class GameEventSystem : MonoBehaviour
{
    private readonly Dictionary<Type, Delegate> events = new();

    public static GameEventSystem Intance;

    private void Awake()
    {
        //todo singleton pattern
    }

    public TResult Raise<TEvent, TResult>(TEvent eventObject)
    {
        if (events.TryGetValue(typeof(TEvent), out Delegate action))
        {
            ((Func<TEvent, TResult>)action).Invoke(eventObject);
        }
        else
        {
            Debug.Log("Event not found!");
        }
    }

    public void RegisterListener<TEvent, TResult>(Func<TEvent, TResult> action)
    {
        //todo
    }


    public void UnregisterListener<TEvent, TResult>(Func<TEvent, TResult> action)
    {
        //todo
    }
}