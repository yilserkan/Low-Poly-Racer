using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheelsv2 : MonoBehaviour
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

    [Header("Wheel")]
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

    [Header("Frictions")]
    [SerializeField] private float roadFriction;
    [SerializeField] private float offRoadFriction;

    [Header("Longtitudinal Forces")]
    [SerializeField] private float airRessitanceDragConstant;
    [SerializeField] private float rollingRessitanceConstant;

    private Vector3 airRessistance;
    private Vector3 rollingResistance;

    public float frictionAmount;
    public Vector3 velocityDir;

    public float SpringLength()
    {
        return springLength;
    }

    private void Start()
    {
        maxLength = restLength + springTravel;
        minLength = restLength - springTravel;
    }

    private void Update()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, ackermannAngle, steerTime * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
    }

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, maxLength + wheelRadius))
        {

           
            //rollingResistance = transform.TransformDirection(-rollingRessitanceConstant * transform.InverseTransformDirection(rb.velocity));


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


            //Handle Friction
            if (hit.collider.gameObject.layer == 10)
            {
                frictionAmount = roadFriction;
            }
            else
            {
                frictionAmount = offRoadFriction;
            }
            airRessitanceDragConstant = frictionAmount / 30f;
            // frictionForce = frictionAmount * Mathf.Sign(wheelVelocityLS.z);
            Vector3 fr2 = transform.TransformDirection(Vector3.forward * -frictionAmount * transform.InverseTransformDirection(rb.velocity).z);
            airRessistance = transform.TransformDirection(Vector3.forward * -airRessitanceDragConstant * transform.InverseTransformDirection(rb.velocity).z * transform.InverseTransformDirection(rb.velocity).magnitude);

            //Handle SideWays Friction
            Debug.Log(transform.InverseTransformDirection(rb.velocity));

            // Debug.DrawLine(transform.position, rb.velocity*10, Color.magenta);


            rb.AddForceAtPosition(suspensionForce + fr2 +airRessistance+ (stabilizerBarForce * transform.up) + ((verticalForce) * transform.forward) + (horizonalForce * -transform.right), hit.point);
        }
        else
        {
            // Car is flying or upside down
            // Vector3 newYPosition = wheelMesh.localPosition;
            // newYPosition.y = Mathf.Lerp(wheelMesh.localPosition.y, -restLength, 6);
            // wheelMesh.localPosition = newYPosition;
        }
    }

    public void SetStabilizerBarForce(float force)
    {
        stabilizerBarForce = force;
    }
}
