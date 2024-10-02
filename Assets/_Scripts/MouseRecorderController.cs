using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseRecorderController : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private CurveController curveController;

    [SerializeField]
    private FPSOptions FPS = FPSOptions.FPS_24;

    private UserInputActions userInputActions;

    private bool recording;
    private float recordInterval = 1f / 24f;
    private float timeSinceLastRecording = 0f;
    private int fps;
    private List<Marker> recordedPositions = new List<Marker>();

    private void Start()
    {
        userInputActions = GameManager.Instance.GetInputAction();
        userInputActions.EditingCurve.RecordMouse.performed += RecordMouse_Performed;
        userInputActions.EditingCurve.RecordMouse.canceled += RecordMouse_Canceled;
        fps = (int)(FPS);
    }

    private void OnDisable()
    {
        userInputActions.EditingCurve.RecordMouse.performed -= RecordMouse_Performed;
        userInputActions.EditingCurve.RecordMouse.canceled -= RecordMouse_Canceled;
    }

    private void Update()
    {
        if (recording)
        {
            timeSinceLastRecording += Time.unscaledDeltaTime;
            {
                if (timeSinceLastRecording >= recordInterval)
                {
                    Marker marker = new Marker(
                        isKey: true,
                        position: inputManager.GetWorldMouseLocation2D()
                    );
                    recordedPositions.Add(marker);
                    timeSinceLastRecording -= recordInterval;
                }
            }
        }
    }

    private void RecordMouse_Performed(InputAction.CallbackContext context)
    {
        recording = true;
        timeSinceLastRecording = 0f;
    }

    private void RecordMouse_Canceled(InputAction.CallbackContext context)
    {
        recording = false;

        //Calculate which Marker is a ghost (AFTER recording positions) depending on fps and change their

        foreach (Marker marker in recordedPositions)
        {
            curveController.AddMarker_TAIL(marker.position);
        }

        recordedPositions.Clear();
    }
}

//Creating a Struct Marker to keep info about the marker. Like if it's a ghost and it's position. What's the different between using this and the Marker Entity?
//And perhaps I should use this Marker struct in other parts of my script too, since it's always useful info.
public struct Marker
{
    public bool isKey;
    public Vector2 position;

    public Marker(bool isKey, Vector2 position)
    {
        this.isKey = isKey;
        this.position = position;
    }
}

//Enum to give dropdown option in Inspector.
public enum FPSOptions
{
    FPS_4 = 4,
    FPS_8 = 8,
    FPS_12 = 12,
    FPS_24 = 24,
}
