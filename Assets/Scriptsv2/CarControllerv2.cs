using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerv2 : MonoBehaviour
{
    [SerializeField] private Wheels[] wheels;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform centerOfMass;

    [Header("Car Specs")]
    [SerializeField] private float wheelBase;
    [SerializeField] private float rearTrack;
    [SerializeField] private float turnRadius;
    [SerializeField] private float wheelSpeed;
    [SerializeField] private float antiroll;



    [Header("Inputs")]
    private float steerInput;
    private float torqueInput;

    private float ackermanAngleLeft;
    private float ackermanAngleRight;

    private Vector3 lastFramesPosition;

    //Wheels
    private Wheels FL;
    private Wheels FR;
    private Wheels RL;
    private Wheels RR;

    private float FLSpringTravel;
    private float FRSpringTravel;
    private float RLSpringTravel;
    private float RRSpringTravel;
    private float frontAntiRollForce;
    private float rearAntiRollForce;

    private void Start()
    {
        // rb.centerOfMass = centerOfMass.localPosition;
        foreach (Wheels wheel in wheels)
        {
            if (wheel.frontLeftWheel)
            {
                FL = wheel;
            }
            if (wheel.frontRightWheel)
            {
                FR = wheel;
            }
            if (wheel.rearLeftWheel)
            {
                RL = wheel;
            }
            if (wheel.rearRightWheel)
            {
                RR = wheel;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(rb.centerOfMass, 1f);
    }

    private void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        if (steerInput > 0) // Turning Right 
        {
            ackermanAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
            ackermanAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
        }
        else if (steerInput < 0) // Turning Left
        {
            ackermanAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
            ackermanAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
        }
        else
        {
            ackermanAngleLeft = 0;
            ackermanAngleRight = 0;
        }

        torqueInput = Input.GetAxis("Vertical");

        FL.ackermannAngle = ackermanAngleLeft;
        FR.ackermannAngle = ackermanAngleLeft;
        RL.carSpeed = torqueInput * wheelSpeed;
        RR.carSpeed = torqueInput * wheelSpeed;
    }

    private void FixedUpdate()
    {
        StabilizerBars();
    }

    private void StabilizerBars()
    {
        FLSpringTravel = FL.SpringLength();
        FRSpringTravel = FR.SpringLength();
        RLSpringTravel = RL.SpringLength();
        RRSpringTravel = RR.SpringLength();

        frontAntiRollForce = (FLSpringTravel - FRSpringTravel) * antiroll;
        rearAntiRollForce = (RLSpringTravel - RRSpringTravel) * antiroll;

        FL.SetStabilizerBarForce(-frontAntiRollForce);
        FR.SetStabilizerBarForce(frontAntiRollForce);
        RL.SetStabilizerBarForce(-rearAntiRollForce);
        RR.SetStabilizerBarForce(rearAntiRollForce);
    }

}
