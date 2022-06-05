using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum ControlType { HumanInput, AI }
    [SerializeField] private ControlType controlType = ControlType.HumanInput;

    public float BestLapTime { get; private set;} = Mathf.Infinity;
    public float LastLapTime { get; private set; } = 0;
    public float CurrentLapTime { get; private set; } = 0;
    public int CurrentLap { get; private set; } = 0;

    private float lapTimerTimeStamp;
    private int lastCheckpointPassed = 0;

    private Transform checkpointsParent;
    private int checkpointCount;
    private int checkpointLayer;
    private Car carController;

    private void Awake() 
    {
        checkpointsParent = GameObject.Find("Checkpoints").transform;
        checkpointCount = checkpointsParent.childCount;
        checkpointLayer = LayerMask.NameToLayer("Checkpoint");
        carController = GetComponent<Car>();
    }

    private void StartLap() 
    {
        Debug.Log("startLap");
        CurrentLap++;    
        lastCheckpointPassed = 1;
        lapTimerTimeStamp = Time.time;
    }

    private void EndLap()
    {
        LastLapTime = Time.time - lapTimerTimeStamp;
        BestLapTime = Mathf.Min(LastLapTime, BestLapTime);
        Debug.Log("EndLap" + LastLapTime);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.layer != checkpointLayer) { return; }

        if(other.name == "1")
        {
            if(lastCheckpointPassed == checkpointCount)
            {
                EndLap();
            }

            if(CurrentLap == 0 || lastCheckpointPassed == checkpointCount)
            {
                StartLap();
            }
            return;
        }

        if(other.gameObject.name == (lastCheckpointPassed +1).ToString())
        {
            lastCheckpointPassed++;
        }

    }

    void Update()
    {
        CurrentLapTime = lapTimerTimeStamp > 0 ? Time.time - lapTimerTimeStamp : 0;

        if(controlType == ControlType.HumanInput)
        {
            carController.Steer = GameManager.Instance.InputController.SteerInput;
            carController.Throttle = GameManager.Instance.InputController.ThrottleInput;
        }
    }
}
