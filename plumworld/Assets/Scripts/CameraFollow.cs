using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{  
    public Transform target;
    Vector3 velocity = Vector3.zero;
    public Vector3 offset = Vector3.zero;
    public float smoothTime = .15f;
    void Update()
    {    
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.position = smoothedPosition;
    }

}
