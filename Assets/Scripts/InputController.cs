using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private string inputSteerAxis = "Horizontal";
    [SerializeField] private string inputThrottleAxis = "Vertical";

    public float ThrottleInput{ get; private set; }
    public float SteerInput { get; private set; }

    void Update()
    {
        SteerInput = Input.GetAxis(inputSteerAxis);
        ThrottleInput = Input.GetAxis(inputThrottleAxis);
    }
}
