using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] private Transform centerOfMass;

    [SerializeField] private float motorTorque = 100f;
    [SerializeField] private float maxSteer = 20f;

    public float Steer { get; set; }
    public float Throttle { get; set; }

    private Rigidbody _rigidBody;
    private Wheel[] wheels;

    private void Start() 
    {
        wheels = GetComponentsInChildren<Wheel>();

        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.centerOfMass = centerOfMass.localPosition;    
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(_rigidBody.centerOfMass, 1f);
    }
    private void Update() 
    {
        foreach(Wheel wheel in wheels)
        {
            wheel.SteerAngle = Steer * maxSteer;
            wheel.Torque = Throttle * motorTorque;
        }
    }
}
