using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject UIRacePanel;

    [SerializeField] private TMP_Text UITextCurrentLap;
    [SerializeField] private TMP_Text UITextCurrentTime;
    [SerializeField] private TMP_Text UITextLastLapTime;
    [SerializeField] private TMP_Text UITextBestLapTime;

    [SerializeField] private Player UpdateUIForPlayer;

    private int currentLap;
    private float currentTime;
    private float lastLapTime;
    private float bestLapTime;

    void Update()
    {
        if(UpdateUIForPlayer == null) { return; }      

        if(UpdateUIForPlayer.CurrentLap != currentLap)
        {
            currentLap = UpdateUIForPlayer.CurrentLap;
            UITextCurrentLap.text = $"LAP: {currentLap}";
        }
        if (UpdateUIForPlayer.CurrentLapTime != currentTime)
        {
            currentTime = UpdateUIForPlayer.CurrentLapTime;
            UITextCurrentTime.text = $"TIME: {(int)currentTime / 60}:{(currentTime) % 60:00.000}";
        }
        if (UpdateUIForPlayer.LastLapTime != lastLapTime)
        {
            lastLapTime = UpdateUIForPlayer.LastLapTime;
            UITextLastLapTime.text = $"TIME: {(int)lastLapTime / 60}:{(lastLapTime) % 60:00.000}";
        }
        if (UpdateUIForPlayer.BestLapTime != bestLapTime)
        {
            bestLapTime = UpdateUIForPlayer.BestLapTime;
            UITextBestLapTime.text = bestLapTime < 1000000 ? $"TIME: {(int)bestLapTime / 60}:{(bestLapTime) % 60:00.000}" : "Best : NONE";
        }
    }
}
