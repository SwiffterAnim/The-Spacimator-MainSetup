using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour
{
    private Vector2 mousePosition;
    private RaycastHit2D[] mouseHitAllArray;
    private UserInputActions userInputActions;

    private bool firstOfTwoClicked = false;
    private bool secondOfTwoClicked = false;
    private float timer;
    private float timeLimitToActivateDoubleClick = 0.45f;

    private void Start()
    {
        userInputActions = GameManager.Instance.GetInputAction();
        userInputActions.EditingCurve.MoveMarker.performed += MoveMarker_performed;
        userInputActions.EditingCurve.MoveMarker.canceled += MoveMarker_canceled;
        userInputActions.EditingCurve.AddMarker.performed += AddMarker_performed;
        userInputActions.EditingCurve.DeleteMarker.performed += DeleteMarker_performed;
    }

    private void OnDestroy()
    {
        userInputActions.EditingCurve.MoveMarker.performed -= MoveMarker_performed;
        userInputActions.EditingCurve.MoveMarker.canceled -= MoveMarker_canceled;
        userInputActions.EditingCurve.AddMarker.performed -= AddMarker_performed;
        userInputActions.EditingCurve.DeleteMarker.performed -= DeleteMarker_performed;
    }

    private void Update()
    {
        //Checking for curve hovered.
        RaycastHit[] mouseRayCastHit = DetectALL_MouseRaycastHit3D();
        if (mouseRayCastHit != null)
        {
            //Checking if for some reason there's more than one 3D object detected.
            if (mouseRayCastHit.Length > 1)
            {
                Debug.LogError("Two different 3D objects detected.");
            }
            //Checking if it's a curve mesh.
            else if (
                mouseRayCastHit[0].collider.gameObject.TryGetComponent(out SplineController SC)
            )
            {
                GameEventSystem.Instance.Raise<OnHoveringCurveEvent>(new OnHoveringCurveEvent());
            }
        }
        else
        {
            GameEventSystem.Instance.Raise<OnHoveringCurveCanceledEvent>(
                new OnHoveringCurveCanceledEvent()
            );
        }

        //Checking for Double Click.
        if (firstOfTwoClicked)
        {
            timer += Time.unscaledDeltaTime;
            if (timer < timeLimitToActivateDoubleClick && secondOfTwoClicked)
            {
                GameEventSystem.Instance.Raise<onDoubleClickPerformed>(
                    new onDoubleClickPerformed()
                );
                firstOfTwoClicked = false;
                secondOfTwoClicked = false;
                timer = 0f;
            }
            else if (timer >= timeLimitToActivateDoubleClick && !secondOfTwoClicked)
            {
                firstOfTwoClicked = false;
                timer = 0f;
            }
        }
    }

    private void MoveMarker_performed(InputAction.CallbackContext context)
    {
        GameEventSystem.Instance.Raise<onLeftClickPerformed>(
            new onLeftClickPerformed(
                Input.mousePosition,
                DetectALL_MouseRayCastHit2D(),
                GetHoveredObject()
            )
        );

        if (!firstOfTwoClicked)
        {
            firstOfTwoClicked = true;
        }
        else
        {
            secondOfTwoClicked = true;
        }
    }

    private void MoveMarker_canceled(InputAction.CallbackContext context)
    {
        GameEventSystem.Instance.Raise<onLeftClickCanceled>(new onLeftClickCanceled());
    }

    private void AddMarker_performed(InputAction.CallbackContext context)
    {
        GameEventSystem.Instance.Raise<onRightClickPerformed>(
            new onRightClickPerformed(GetWorldMouseLocation2D(), DetectALL_MouseRaycastHit3D())
        );
    }

    private void DeleteMarker_performed(InputAction.CallbackContext context)
    {
        GameEventSystem.Instance.Raise<onDeletePerformed>(new onDeletePerformed());
    }

    //Not sure if this isn't returning the same as Input.mousePosition lol
    private Vector2 GetWorldMouseLocation2D()
    {
        mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mousePosition = mouseWorldPosition;
        return mousePosition;
    }

    public RaycastHit2D[] DetectALL_MouseRayCastHit2D()
    {
        mouseHitAllArray = Physics2D.RaycastAll(GetWorldMouseLocation2D(), Vector3.forward);
        return mouseHitAllArray;
    }

    private RaycastHit[] DetectALL_MouseRaycastHit3D()
    {
        Vector3 mouseHit = GetWorldMouseLocation2D();

        RaycastHit[] mouseHitAllArray3D = Physics.RaycastAll(mouseHit, Vector3.forward);
        RaycastHit[] negativeMouseHitAllArray3D = Physics.RaycastAll(mouseHit, -Vector3.forward);
        if (mouseHitAllArray3D.Length != 0)
        {
            return mouseHitAllArray3D;
        }
        else if (negativeMouseHitAllArray3D.Length != 0)
        {
            return negativeMouseHitAllArray3D;
        }
        else
        {
            return null;
        }
    }

    private GameObject GetHoveredObject()
    {
        if (mouseHitAllArray.Length > 0)
        {
            return mouseHitAllArray[^1].transform.gameObject;
        }
        else
        {
            return null;
        }
    }
}

//====================    EVENTS    ====================
public struct onDoubleClickPerformed { }

public struct onRightClickPerformed
{
    public Vector2 mousePosition { get; private set; }
    public RaycastHit[] mouseHitAllArray3D { get; private set; }

    public onRightClickPerformed(Vector2 mousePosition, RaycastHit[] mouseHitAllArray3D = null)
    {
        this.mousePosition = mousePosition;
        this.mouseHitAllArray3D = mouseHitAllArray3D;
    }
}

public struct onLeftClickPerformed
{
    public Vector2 mousePosition { get; private set; }
    public RaycastHit2D[] mouseRayCastHit { get; private set; }
    public GameObject hoveredObject { get; private set; }

    //Constructor.
    public onLeftClickPerformed(
        Vector2 mousePosition,
        RaycastHit2D[] mouseRayCastHit,
        GameObject hoveredObject
    )
    {
        this.mousePosition = mousePosition;
        this.mouseRayCastHit = mouseRayCastHit;
        this.hoveredObject = hoveredObject;
    }
}

public struct onLeftClickCanceled { }

public struct onDeletePerformed { }

public struct OnHoveringCurveEvent { }

public struct OnHoveringCurveCanceledEvent { }
