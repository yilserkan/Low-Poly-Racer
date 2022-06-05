using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 eularRotation;

    [SerializeField] private float damper;
    
    void Start()
    {
        transform.eulerAngles = eularRotation;
    }

    void Update()
    {
        if(target == null) { return; }

        transform.position = Vector3.Lerp(transform.position, target.position + offset, damper);
    }
}
