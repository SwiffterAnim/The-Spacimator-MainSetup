using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CustomGameEvent : UnityEvent<Component, object> { }

public class GameEventListener : MonoBehaviour
{
    //If you have more than one you can change the name of this game event
    [SerializeField]
    private GameEvent gameEvent;

    public CustomGameEvent response;

    private void OnEnable()
    {
        gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        gameEvent.UnregisterListener(this);
    }

    //We might not need the component and we can use struct instead of object, I'd like to know what you think.
    public void OnEventRaised(Component sender, object data)
    {
        response.Invoke(sender, data);
    }
}
