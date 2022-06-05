using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAIManager : MonoBehaviour
{   
    [SerializeField] private Transform pathParent;
    [SerializeField] private float nodeChangeInterval;
    [SerializeField] private float driftingInterval;

    [Header("Sensor")]
    [SerializeField] private Transform sensorStartTransform;
    [SerializeField] private float sensorLength;
    [SerializeField] private float edgeSensorsOffset;
    [SerializeField] private float edgeSensorAngles;
    [SerializeField] private float angleToStartDrifting;
    [SerializeField] private float angleToStopDrifting;

    private List<Transform> pathNodes = new List<Transform>();
    private CarController carController;
    private int currentNode;
    private float sensorSteerInput;
    private bool isAvoiding;
    private bool isBackingUp;
    private bool isStuck;
    private float lastCurrentNodeChangedTime;
    private float lastDritedTime;
    private float stuckTimeInterval = 1.5f;
    

    private void Start() 
    {
        carController = GetComponent<CarController>();
        currentNode = 0;
        for (int i = 0; i < pathParent.childCount; i++)
        {   
            if(pathParent.GetChild(i) != pathParent.transform)
            {
                pathNodes.Add(pathParent.GetChild(i));
            }
        } 
    }

    private void Update() 
    {
        HandleNodeOutOfReach();
        HandleStuck();
        Sensors();
        SteerWheel();
        DriveCar();
        // Debug.Log($"Update -> {carController.steerInput}");
    }

    private void HandleStuck()
    {
        if(carController.rpm < 0.025)
        {
            stuckTimeInterval -= Time.deltaTime;
            if(stuckTimeInterval <= 0 && !isStuck)
            {
                isStuck = true;
                Debug.Log("Started");
                StartCoroutine(Backup());
            }
            return;
        }
        stuckTimeInterval = 1.5f;
        isStuck = false;
    }

    private void HandleNodeOutOfReach()
    {
        float yDistance = Mathf.Abs(transform.position.y - pathNodes[currentNode].position.y);
        float nodeDistance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(pathNodes[currentNode].position.x,pathNodes[currentNode].position.z));
        if(yDistance > 2 && nodeDistance < 2 && Time.time > lastCurrentNodeChangedTime)
        {
            currentNode--;
            lastCurrentNodeChangedTime = Time.time + nodeChangeInterval;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.tag == "Road" && Time.time > lastCurrentNodeChangedTime && other.transform == pathNodes[currentNode])
        {
            ChangeCurrentNode();
            lastCurrentNodeChangedTime = Time.time + nodeChangeInterval;
        }
    }

    private void SteerWheel()
    {
        if(isAvoiding || isBackingUp) { return; }
        Vector3 pathLocalVector = transform.InverseTransformPoint(pathNodes[currentNode].position);
        Vector3 pathLocalVectorNormalized = pathLocalVector / pathLocalVector.magnitude;

        Vector3 dirToPathNode = (pathNodes[currentNode].position - transform.position).normalized;
        float targetAngle = Vector3.SignedAngle(transform.forward, dirToPathNode, Vector3.up);

        if (Mathf.Abs(targetAngle) > 100)
        {
            StartCoroutine(Backup());
            return;
            // carController.startDrifting = true;
        }

        //Start Drifting
        if(Time.time > lastDritedTime)
        {
            if (Mathf.Abs(targetAngle) > angleToStartDrifting)
            {
                // carController.startDrifting = true;
            }
            lastDritedTime = Time.time + driftingInterval;
        }
        //Stop Drifting
        if(carController.startDrifting == true && Mathf.Abs(targetAngle) < angleToStopDrifting)
        {
            // carController.startDrifting = false;
        }

        carController.steerInput = pathLocalVectorNormalized.x;
    }

    private void DriveCar()
    {
        if(isBackingUp) { return; }
        Vector3 dirToPathNode = (pathNodes[currentNode].position - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, dirToPathNode);
        if(Vector3.Distance(transform.position, pathNodes[currentNode].position) > 10)
        {
            dotProduct = Mathf.Abs(dotProduct);
        }
        carController.torqueInput = dotProduct/2;
        // carController.torqueInput = 0;
    }

    private void ChangeCurrentNode()
    {
        currentNode = (currentNode == pathNodes.Count - 1) ? 0 : currentNode + 1; 
    }

    private void Sensors()
    {    
        RaycastHit hit;
        Vector3 sensorPos = sensorStartTransform.localPosition;
        isAvoiding = false;
        sensorSteerInput = 0;

        // Rigth straigth sensor
        sensorPos.x = sensorStartTransform.localPosition.x + edgeSensorsOffset;
        if (Physics.Raycast(transform.TransformPoint(sensorPos), transform.forward, out hit, sensorLength))
        {
            if(hit.transform.tag != "Road")
            {
                isAvoiding = true;
                sensorSteerInput += -1;
                Debug.DrawLine(transform.TransformPoint(sensorPos), hit.point);
            }
        }
        // Rigth Angled sensor
        else if (Physics.Raycast(transform.TransformPoint(sensorPos), Quaternion.AngleAxis(edgeSensorAngles, transform.up) * transform.forward, out hit, sensorLength))
        {
            if (hit.transform.tag != "Road")
            {
                isAvoiding = true;
                sensorSteerInput += -0.5f;
                Debug.DrawLine(transform.TransformPoint(sensorPos), hit.point);
            }
        }
        // Left straigth sensor
        sensorPos.x = sensorStartTransform.localPosition.x - edgeSensorsOffset;
        if (Physics.Raycast(transform.TransformPoint(sensorPos), transform.forward, out hit, sensorLength))
        {
            if (hit.transform.tag != "Road")
            {
                isAvoiding = true;
                sensorSteerInput += 1;
                Debug.DrawLine(transform.TransformPoint(sensorPos), hit.point);
            }
        }
        else if (Physics.Raycast(transform.TransformPoint(sensorPos), Quaternion.AngleAxis(-edgeSensorAngles, transform.up) * transform.forward, out hit, sensorLength))
        {
            if (hit.transform.tag != "Road")
            {
                isAvoiding = true;
                sensorSteerInput += .5f;
                Debug.DrawLine(transform.TransformPoint(sensorPos), hit.point);
            }
        }
        //Sensor in the center
        sensorPos = sensorStartTransform.localPosition;
        if(sensorSteerInput == 0)
        {
            if (Physics.Raycast(transform.TransformPoint(sensorPos), transform.forward, out hit, sensorLength))
            {
                Debug.DrawLine(transform.TransformPoint(sensorPos), hit.point);
                float hitNormalAngle = Vector3.SignedAngle((transform.TransformPoint(sensorPos) - hit.point).normalized,hit.normal, Vector3.up);
                if (hit.transform.tag != "Road")
                {
                    if(hitNormalAngle < -15)
                    {
                        sensorSteerInput = 1;
                    }
                    else if(hitNormalAngle > 15)
                    {
                        sensorSteerInput = -1;
                    }
                    else
                    {
                        //carController.torqueInput = -1;
                        carController.torqueInput = 1f;
                       
                        StartCoroutine(Backup());
                    }
                   
                }
            }
        }

        if(isAvoiding && !isBackingUp)
        {
            carController.steerInput = sensorSteerInput;
        }
    }

    IEnumerator Backup()
    {
        isBackingUp = true;
        carController.torqueInput *= -1f;
        carController.steerInput = Mathf.Sign(carController.steerInput) * -1f;
        yield return new WaitForSeconds(1f);
        isBackingUp = false;
        StopCoroutine(Backup());
    }
}
