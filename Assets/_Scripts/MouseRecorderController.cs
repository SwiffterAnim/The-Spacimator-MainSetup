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
    private List<MarkerDataStruct> recordedPositions = new List<MarkerDataStruct>();

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
                    MarkerDataStruct marker = new MarkerDataStruct(
                        isKey: true,
                        markerPosition: Input.mousePosition
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

        if (recordedPositions.Count > 0)
        {
            GameEventSystem.Instance.Raise<onRecordingFinishedEvent>(
                new onRecordingFinishedEvent(recordedPositions)
            );
        }

        recordedPositions.Clear();
    }
}

//====================    EVENTS    ====================
public struct onRecordingFinishedEvent
{
    public List<MarkerDataStruct> recordedMarkers { get; private set; }

    public onRecordingFinishedEvent(List<MarkerDataStruct> recordedMarkers)
    {
        this.recordedMarkers = recordedMarkers;
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
