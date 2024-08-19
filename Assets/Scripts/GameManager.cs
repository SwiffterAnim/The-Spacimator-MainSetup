using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private UserInputActions userInputActions;

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
        //I'm assuming the "creating curve" mode is on as default. If later we want to go to "record" mode, these might need to change.
        userInputActions = new UserInputActions();
        userInputActions.EditingCurve.Enable();
    }

    public UserInputActions GetInputAction()
    {
        return userInputActions;
    }
}
