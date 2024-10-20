using System;
using System.Collections.Generic;
using UnityEngine;

public class GameEventSystem : MonoBehaviour
{
    private readonly Dictionary<Type, Delegate> events = new();

    public static GameEventSystem Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Raise method for events that do NOT return a result (Action<TEvent>)
    public void Raise<TEvent>(TEvent eventObject)
    {
        if (events.TryGetValue(typeof(TEvent), out Delegate existing))
        {
            if (existing is Action<TEvent> action)
            {
                action.Invoke(eventObject);
            }
            else
            {
                throw new InvalidOperationException(
                    $"TEvent {typeof(TEvent)} is expected to use a Func, but a different delegate type is registered."
                );
            }
        }
        else
        {
            Debug.LogWarning("Event not found!");
        }
    }

    // Raise method for events that do not return a result (Func<TEvent, TResult>)
    public TResult Raise<TEvent, TResult>(TEvent eventObject)
    {
        if (events.TryGetValue(typeof(TEvent), out Delegate action))
        {
            if (action is Func<TEvent, TResult> func)
            {
                return func.Invoke(eventObject);
            }
            else
            {
                throw new InvalidOperationException(
                    $"TEvent {typeof(TEvent)} is expected to use a Func, but a different delegate type is registered."
                );
            }
        }
        else
        {
            Debug.LogWarning("Event not found!");
            return default;
        }
    }

    //Registering listeners to event that do NOT require a result to be returned. (Action<TEvent>)
    public void RegisterListener<TEvent>(Action<TEvent> action)
    {
        if (events.TryGetValue(typeof(TEvent), out Delegate existing))
        {
            if (existing is Action<TEvent> existingAction)
            {
                // Cast the existing delegate to Func<TEvent, TResult> and combine
                events[typeof(TEvent)] = existingAction + action;
            }
            else
            {
                throw new InvalidOperationException(
                    $"TEvent {typeof(TEvent)} is expected to use an Action, but a different delegate type is registered."
                );
            }
        }
        else
        {
            events[typeof(TEvent)] = action; // No existing event, so just add it
        }
    }

    //Registering listeners to event that require a result to be returned. (Func<TEvent, TResult>)
    public void RegisterListener<TEvent, TResult>(Func<TEvent, TResult> func)
    {
        if (events.TryGetValue(typeof(TEvent), out Delegate existing))
        {
            if (existing is Func<TEvent, TResult> existingFunc)
            {
                // Cast the existing delegate to Func<TEvent, TResult> and combine
                events[typeof(TEvent)] = existingFunc + func;
            }
            else
            {
                throw new InvalidOperationException(
                    $"TEvent {typeof(TEvent)} is expected to use a Func, but a different delegate type is registered."
                );
            }
        }
        else
        {
            events[typeof(TEvent)] = func; // No existing event, so just add it
        }
    }

    //Unregistering listeners to event that do NOT require a result to be returned. (Action<TEvent>)
    public void UnregisterListener<TEvent>(Action<TEvent> action)
    {
        if (events.TryGetValue(typeof(TEvent), out Delegate existing))
        {
            if (existing is Action<TEvent> existingAction)
            {
                events[typeof(TEvent)] = Delegate.Remove(existingAction, action);
            }
        }
    }

    //Unregistering listeners to event that require a result to be returned. (Func<TEvent, TResult>)
    public void UnregisterListener<TEvent, TResult>(Func<TEvent, TResult> func)
    {
        if (events.TryGetValue(typeof(TEvent), out Delegate existing))
        {
            if (existing is Func<TEvent, TResult> existingFunc)
            {
                events[typeof(TEvent)] = Delegate.Remove(existingFunc, func);
            }
        }
    }
}
