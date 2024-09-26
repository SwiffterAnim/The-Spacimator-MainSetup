using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimationController : MonoBehaviour
{
    [SerializeField]
    private GameObject animatedObjectPrefab;

    [SerializeField]
    private CurveController curveController;

    [SerializeField]
    private Canvas canvas;

    private bool isPlaying = false;
    public bool hideMarkers = false;
    private GameObject animatedObject;
    private float recordInterval = 1f / 24f;
    private float timeSinceLastRecording = 0f;
    private int i = 0;

    private void Awake()
    {
        canvas.worldCamera = Camera.main;
    }

    private void Update()
    {
        if (isPlaying && animatedObject != null)
        {
            timeSinceLastRecording += Time.deltaTime;

            if (timeSinceLastRecording >= recordInterval) //fps
            {
                //Move the animatedObject into the marker position at index i.
                animatedObject.transform.position = curveController
                    .markerList[i]
                    .transform
                    .position;

                timeSinceLastRecording = 0f;

                //This is to check if we're on the last frame, so we can keep looping the animation.
                if (i >= curveController.markerList.Count - 1)
                {
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
        }
    }

    public void PlayAnimation()
    {
        if (curveController.markerList.Count > 1 && animatedObject == null)
        {
            Vector3 initialPosition = curveController.markerList[0].transform.position;
            animatedObject = Instantiate(
                animatedObjectPrefab,
                initialPosition,
                Quaternion.identity
            );
        }

        //Hide the markers and the curve?
        if (hideMarkers)
        {
            for (int i = 0; i < curveController.markerList.Count; i++)
            {
                if (curveController.markerList[i].TryGetComponent(out MarkerEntity markerEntity))
                {
                    markerEntity.isPlaying = true;
                }
            }
        }

        isPlaying = true;
    }

    public void StopAnimation()
    {
        isPlaying = false;
        if (animatedObject != null)
        {
            Destroy(animatedObject);
        }
        i = 0;

        //Show the markers and the curve?
        if (hideMarkers)
        {
            for (int i = 0; i < curveController.markerList.Count; i++)
            {
                if (curveController.markerList[i].TryGetComponent(out MarkerEntity markerEntity))
                {
                    markerEntity.isPlaying = false;
                }
            }
        }
    }
}
