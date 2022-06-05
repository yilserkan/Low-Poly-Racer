using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheels : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform wheelMesh;

    public bool frontLeftWheel;
    public bool frontRightWheel;
    public bool rearLeftWheel;
    public bool rearRightWheel;

    [Header("Suspension")]
    [SerializeField] private float restLength;
    [SerializeField] private float springTravel;
    [SerializeField] private float springStiffness;
    [SerializeField] private float damperStiffness;

    private float maxLength;
    private float minLength;
    private float springLength;
    private float springVelocity;
    private float springForce;
    private float damperForce;
    private float frictionForce;
    private Vector3 suspensionForce;
    private float lastFramesSpringLength;

    [Header ("Wheel")]
    [SerializeField] private float wheelRadius;
    [SerializeField] private float steerTime;
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float wheelRotationSpeed;
    public float carSpeed;
    public float ackermannAngle;
    private float wheelAngle;
    private Vector3 wheelVelocityLS;
    private float verticalForce;
    private float horizonalForce;
    private float stabilizerBarForce;
    public float lateralForce;

    [Header("Frictions")]
    [SerializeField] private float roadFriction;
    [SerializeField] private float offRoadFriction;


    [Header("Longtitudinal Forces")]
    [SerializeField] private float airRessitanceDragConstant;
    [SerializeField] private float brakingForceConstant;

    private Vector3 airRessistance;
    private Vector3 rollingResistance;
    private Vector3 tractionForce;
    private Vector3 longtidunalForce;
    private Vector3 inputForce;
    private Vector3 weightTransferForce;
    private float lastFramesVelocity;

    public float frictionAmount;
    public float velocityDir;
    public bool isBraking;

    public float boostAmount;
    public bool resetCar;
    public float lastWheelTouchedGroundTimeStamp; 

    public float SpringLength() {
        return springLength;
     }

    private void Start() 
    {
        maxLength = restLength + springTravel;
        minLength = restLength - springTravel;    
    }

    private void Update() 
    {
        wheelAngle = Mathf.Lerp(wheelAngle, ackermannAngle,steerTime * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
    }
   
    private void FixedUpdate()
    {
        if(Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, maxLength + wheelRadius))
        {
            lastWheelTouchedGroundTimeStamp = Time.time;
            //Calculate Spring Length
            lastFramesSpringLength = springLength;
            springLength = hit.distance - wheelRadius;
            springLength = Mathf.Clamp(springLength, minLength, maxLength);

            //Calc the Position of the wheelMesh
            Vector3 newwheelMeshPosition = wheelMesh.localPosition;
            newwheelMeshPosition.y = 0 - springLength;
            wheelMesh.localPosition = newwheelMeshPosition;

            //Calc Forces
            springForce = springStiffness * (restLength - springLength);
            springVelocity = (lastFramesSpringLength - springLength) / Time.fixedDeltaTime;
            damperForce = damperStiffness * springVelocity;
            suspensionForce = (springForce + damperForce) * Vector3.up;
            wheelVelocityLS = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
            verticalForce = carSpeed;
            horizonalForce = wheelVelocityLS.x * horizontalSpeed;
            

            //Handle Frictions
            if(hit.collider.gameObject.layer == 10)
            {
                frictionAmount = roadFriction;
            }
            else
            {
                frictionAmount = offRoadFriction;
            }
            airRessitanceDragConstant = frictionAmount / 30f;
            airRessistance = transform.TransformDirection(Vector3.forward * -airRessitanceDragConstant * transform.InverseTransformDirection(rb.velocity).z * transform.InverseTransformDirection(rb.velocity).magnitude);

            rollingResistance = transform.TransformDirection(Vector3.forward * -frictionAmount * transform.InverseTransformDirection(rb.velocity).z);

            //Handle Braking
            if (isBraking )
            {
                if(transform.InverseTransformDirection(rb.velocity).z >= -1 &&
                transform.InverseTransformDirection(rb.velocity).z <= 1)
                {
                    inputForce = Vector3.zero;
                }
                else
                {
                    if(Mathf.Sign(transform.InverseTransformDirection(rb.velocity).z) > 0)
                        inputForce = brakingForceConstant * -transform.forward;
                    else
                        inputForce = brakingForceConstant * transform.forward;
                }
            }
            else
            {
                // inputForce = tractionForce;
            }

            longtidunalForce = rollingResistance + airRessistance + inputForce;

            //Check if acceleration is zero, if yes long Force = 0
            if(-Mathf.Epsilon <= transform.InverseTransformDirection(rb.velocity).z - lastFramesVelocity &&
                transform.InverseTransformDirection(rb.velocity).z - lastFramesVelocity <= Mathf.Epsilon)
            {   
                longtidunalForce = Vector3.zero;
            }
            lastFramesVelocity = transform.InverseTransformDirection(rb.velocity).z;


            rb.AddForceAtPosition(suspensionForce + longtidunalForce + (stabilizerBarForce * transform.up) + ((verticalForce + boostAmount) * transform.forward) + (horizonalForce * -transform.right), hit.point);
        }
        else
        {
            // Car is flying or upside down
            if(lastWheelTouchedGroundTimeStamp + 5f < Time.time)
            {
                resetCar = true;
            }

            Vector3 newwheelMeshPosition = wheelMesh.localPosition;
            newwheelMeshPosition.y = Mathf.Lerp(newwheelMeshPosition.y, 0 - restLength, 8 );
            wheelMesh.localPosition = newwheelMeshPosition;

        }
    }

    public void SetStabilizerBarForce(float force)
    {
        stabilizerBarForce = force;
    }

    public void ResetCar()
    {
        resetCar = false;
        lastWheelTouchedGroundTimeStamp = Time.time;
    }
}
