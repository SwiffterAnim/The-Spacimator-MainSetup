using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CurveManager : MonoBehaviour
{
    [SerializeField]
    GameObject curveGroupPrefab;

    private void Awake()
    {
        //Instantiating the first curveGroup. This can later be locked and others can be created. Only 1 active curve at a time.
        Instantiate(curveGroupPrefab, Vector3.zero, quaternion.identity);
    }
}
