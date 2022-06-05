using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInputManager : MonoBehaviour
{   
    [SerializeField] private CarController carController;

    [SerializeField] private bool isPlayer;
    [SerializeField] private bool isAI;

    // Update is called once per frame
    void Update()
    {
        if(isPlayer)
        {
            GetPlayerInputs();
        }
    }

    private void GetPlayerInputs()
    {
        carController.steerInput = Input.GetAxis("Horizontal");
        carController.torqueInput = Input.GetAxis("Vertical");
    }
}
