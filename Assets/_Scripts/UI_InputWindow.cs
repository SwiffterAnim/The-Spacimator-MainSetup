using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_InputWindow : MonoBehaviour
{
    [SerializeField]
    private CurveController curveController;

    [SerializeField]
    private TMP_InputField inputText;

    private UserInputActions userInputActions;
    private GameObject selectedMarker;
    private int currentFrameNumber;

    private void OnDisable()
    {
        userInputActions.EditingCurve.AcceptChange.performed -= AcceptChange_Performed;
        userInputActions.EditingCurve.CancelChange.performed -= CancelChange_Performed;
    }

    private void Awake()
    {
        userInputActions = GameManager.Instance.GetInputAction();
        userInputActions.EditingCurve.AcceptChange.performed += AcceptChange_Performed;
        userInputActions.EditingCurve.CancelChange.performed += CancelChange_Performed;
    }

    private void AcceptChange_Performed(InputAction.CallbackContext context)
    {
        //Accept
        //int updatedFrameNumber = (int)inputText.text; //how do I make this into an int?)
        //curveController.UpdateFrameNumber(currentFrameNumber, updatedFrameNumber);
        CancelChange();
    }

    private void CancelChange_Performed(InputAction.CallbackContext context)
    {
        CancelChange();
    }

    public void CancelChange()
    {
        Destroy(gameObject);
    }
}
