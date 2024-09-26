using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepRotationUpright : MonoBehaviour
{
    [SerializeField]
    Transform parent;

    void Update()
    {
        Vector3 yOffset = parent.position;
        yOffset.y -= 0.5f;
        transform.position = yOffset;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}
