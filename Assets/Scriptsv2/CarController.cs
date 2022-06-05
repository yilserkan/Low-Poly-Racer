using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CarController : MonoBehaviour
{   
    [SerializeField] private Wheels[] wheels;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private List<Color> turboColors;
    [SerializeField] private List<ParticleSystem> particles;
    [SerializeField] private List<ParticleSystem> particles2;
    [SerializeField] private List<TrailRenderer> wheelTrails;
    [SerializeField] private GameObject vcam;

    private bool first,second, third;
    private Color c;

    // Higher CoG tilts more in Corners
    //The further the CoG is towards the rear, the more it oversteers
    //[SerializeField] private Transform centerOfMass;

    [Header("Car Specs")]
    [SerializeField] private float wheelBase;
    [SerializeField] private float rearTrack;
    [SerializeField] private float turnRadius;
    [SerializeField] private float wheelSpeed;
    [SerializeField] private float antiroll;
    [SerializeField] private float tyreFractionCoefficient;
    [SerializeField] private float inertiaFactor;

    [Header("Torque")]
    [SerializeField] private float gearRatio;
    [SerializeField] private float differentialRatio;
    [SerializeField] private float transmissionEfficiency;
    [SerializeField] private float wheelRadius;

    private float torque;
    [SerializeField] private List<Vector3> torqueCurve;
    [SerializeField] private List<Vector2> slipCurve;
    public float rpm;
    private float maxTorque;
    private float engineTorque;
    private float driveTorque;
    private float driveForce;


    [Header("Inputs")]
    public float steerInput;
    public float torqueInput;

    private float lastFramesTorqueInput;

    private float ackermanAngleLeft;
    private float ackermanAngleRight;

    private float movingDir;

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

    private float frontWheelLoad;
    private float rearWheelLoad;
    private float distFromRearToCOG;
    private float distFromFrontToCOG;

    private float frontWheelLateralAngle;
    private float rearWheelLateralAngle;
    private float normalizedRearWheelDir;
    private Vector3 centerOfMassPosition;

    [Header("Drifting")]
    [SerializeField] private bool isDrifting;
    [SerializeField] private float driftingMultiplier;
    [SerializeField] private float driftingAngle;
    [SerializeField] private float boostConstant;
    private float driftDirection;
    private float driftMode;
    private float driftPower = 1;
    private float verticalInputWhileDrifting;
    public bool startDrifting;

    private void Start() 
    {
        first = false; second = false; third = false;
        rb.inertiaTensor *= inertiaFactor;
        centerOfMassPosition = rb.centerOfMass;
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

    private void Update() 
    {   
        StabilizerBars();
        ColorDrift();
        ResetCar();

        // steerInput = Input.GetAxis("Horizontal");

        if(steerInput > 0) // Turning Right 
        {
            ackermanAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput ;
            ackermanAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput ;
        }
        else if(steerInput < 0) // Turning Left
        {
            ackermanAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
            ackermanAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
        }
        else
        {
            ackermanAngleLeft = 0;
            ackermanAngleRight = 0;
        }

        movingDir = Mathf.Sign(transform.InverseTransformDirection(rb.velocity).z);

        // torqueInput = Input.GetAxis("Vertical");

        if(movingDir != Mathf.Sign(torqueInput) && torqueInput != 0)
        {
            FL.isBraking = true;
            FR.isBraking = true;
            RL.isBraking = true;
            RR.isBraking = true;
        }
        else
        {
            FL.isBraking = false;
            FR.isBraking = false;
            RL.isBraking = false;
            RR.isBraking = false;
        }

        lastFramesTorqueInput = torqueInput;

        FL.ackermannAngle = ackermanAngleLeft;
        FR.ackermannAngle = ackermanAngleLeft;
        // RL.carSpeed = torqueInput * wheelSpeed;
        // RR.carSpeed = torqueInput * wheelSpeed;

        rpm = transform.InverseTransformDirection(rb.velocity).z / wheelRadius;

        maxTorque = LookUpTorqueCurce(rpm);
    
        engineTorque = torqueInput * maxTorque;
        driveTorque = engineTorque * gearRatio * differentialRatio * transmissionEfficiency;
        driveForce = driveTorque / wheelRadius;

        verticalInputWhileDrifting = (isDrifting && driveForce > verticalInputWhileDrifting) ? driveForce : verticalInputWhileDrifting;

        RL.carSpeed = (isDrifting) ? verticalInputWhileDrifting : driveForce;
        RR.carSpeed = (isDrifting) ? verticalInputWhileDrifting : driveForce;

        // Debug.Log(FR.transform.position.z - centerOfMassPosition.z);
        // Debug.Log(RR.transform.position.z - centerOfMassPosition.z);


        // distFromRearToCOG = Math.Abs(distFromRearToCOG - centerOfMassPosition.z);
        // distFromFrontToCOG = Math.Abs(distFromFrontToCOG - centerOfMassPosition.z);

        // frontWheelLoad = (.5f / wheelBase) * rb.mass;
        // rearWheelLoad = (.5f / wheelBase) * rb.mass;

        // Debug.Log(frontWheelLoad);

        // rearWheelLateralAngle = Mathf.Rad2Deg * Mathf.Atan((transform.InverseTransformDirection(rb.velocity).x + 
        //                     ((transform.InverseTransformDirection(rb.velocity).z / wheelRadius) * 
        //                     distFromRearToCOG)) / rb.velocity.magnitude);
        // float lateralRearWheelForce = LookUpSlipCurve(rearWheelLateralAngle) * rearWheelLoad;
        // RL.lateralForce = lateralRearWheelForce;
        // RR.lateralForce = lateralRearWheelForce;

        if (Input.GetKeyDown(KeyCode.Space) )
        {
            startDrifting = true;
        }

        if (startDrifting && !isDrifting && steerInput != 0 && rpm > 50)
        {
            verticalInputWhileDrifting = driveForce;
            foreach (ParticleSystem p in particles)
            {   
                p.Play();
            }
            isDrifting = true;
            driftDirection = steerInput;
        }

        if (Input.GetKeyUp(KeyCode.Space) )
        {
            startDrifting = false;
        }

        if(!startDrifting && isDrifting)
        {
            Boost();
        }

        if(isDrifting)
        {
            driftPower += driftingMultiplier * Time.deltaTime;

            if(driftDirection > 0)
            {
                FL.ackermannAngle += driftingAngle;
                FR.ackermannAngle += driftingAngle;
            }
            else
            {
                FL.ackermannAngle -= driftingAngle;
                FR.ackermannAngle -= driftingAngle;
            }
        }

    }

    private void Boost()
    {
        isDrifting = false;
        first = false; second = false; third = false;
        verticalInputWhileDrifting = 0;
        
        if(driftMode > 0)
        {
            foreach(TrailRenderer wheelTrail in wheelTrails)
            {
                wheelTrail.emitting = true;
            }
            vcam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTransposer>().m_ZDamping = 0.3f;
            float boostAmount = boostConstant * driftMode;
            RR.boostAmount = boostAmount;
            RL.boostAmount = boostAmount;
            Invoke(nameof(ResetBoost), driftMode);

        }
        Invoke(nameof(ResetTrail), driftMode);
        driftMode = 0;
        driftPower = 1;
        foreach (ParticleSystem p in particles)
        {
            var main = p.main;
            main.startColor = c;
            p.Stop();
        }
    }

    private void ResetBoost()
    {
        vcam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTransposer>().m_ZDamping = 0;
        RR.boostAmount = 0;
        RL.boostAmount = 0;
    }

    private void ResetTrail()
    {
        foreach (TrailRenderer wheelTrail in wheelTrails)
        {
            wheelTrail.emitting = false;
        }
    }

    private void ColorDrift()
    {   
        if(!first)
        {
            c = Color.clear;
        }

        if(driftPower > 150 && driftPower < 400-1 && !first)
        {
            first = true;
            c = turboColors[0];
            driftMode = 1;
            PlaySecondaryParticles(c);
        }
        if (driftPower > 400 && driftPower < 700 - 1 && !second ) 
        {
            second = true;
            c = turboColors[1];
            driftMode = 2;
            PlaySecondaryParticles(c);
        }
        if (driftPower > 700 && !third)
        {
            third = true;
            c = turboColors[2];
            driftMode = 3;
            PlaySecondaryParticles(c);
        }

        foreach(ParticleSystem p in particles)
        {
            ParticleSystem.MainModule main = p.main;
            main.startColor = c;
        }
    }

    private void PlaySecondaryParticles(Color c)
    {
        foreach (ParticleSystem p in particles2)
        {
            var main = p.main;
            main.startColor = c;
            p.Play();   
        }
    }

    private float LookUpTorqueCurce(float rpm)
    {   
        int closestItemIndex = 0;
        float closestDistance = Mathf.Infinity;
        for (int i = 0; i < torqueCurve.Count; i++)
        {
            float distance = Mathf.Abs(torqueCurve[i].x - Math.Abs(rpm));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItemIndex = i;
            }
        }

        gearRatio = torqueCurve[closestItemIndex].z;
        if(closestItemIndex == 0)
        {
            return torqueCurve[closestItemIndex].y;
        }

        float lerpedTorqueValue = Mathf.Lerp(torqueCurve[closestItemIndex-1].y,torqueCurve[closestItemIndex].y, (Mathf.Abs(rpm) - torqueCurve[closestItemIndex-1].x) / (torqueCurve[closestItemIndex].x - torqueCurve[closestItemIndex - 1].x));
        return lerpedTorqueValue;
    }

    private float LookUpSlipCurve(float angle)
    {
        float closestAngle = Mathf.Infinity;
        int closestAnglesIndex = 0;
        for (int i = 0; i < slipCurve.Count; i++)
        {
            float distance = Mathf.Abs(slipCurve[i].x - angle);
            if(distance < closestAngle)
            {
                closestAngle = distance;
                closestAnglesIndex = i;
            }
        }
        return slipCurve[closestAnglesIndex].y;
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

    private void ResetCar()
    {
        if(FL.resetCar || FR.resetCar || RR.resetCar || RL.resetCar)
        {
            FL.ResetCar();
            FR.ResetCar();
            RL.ResetCar();
            RR.ResetCar();

            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 0f);
            transform.position += Vector3.up * 2f; 
        }
    }
}
