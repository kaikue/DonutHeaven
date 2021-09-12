using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndStats : MonoBehaviour
{
    public TextMeshProUGUI sprinklesText;
    public TextMeshProUGUI timeText;
    public GameObject bonus;

    private void Start()
    {
        bonus.SetActive(false);
        PersistentTracker persistent = FindObjectOfType<PersistentTracker>();
        bool allSprinkles = persistent.sprinkles == persistent.possibleSprinkles;
        if (allSprinkles)
        {
            bonus.SetActive(true);
        }
        sprinklesText.text = "Sprinkles:\n" + persistent.sprinkles + (allSprinkles ? ("/" + persistent.possibleSprinkles) : "");
        TimeSpan timeSpan = TimeSpan.FromSeconds(persistent.time);
        string timeStr = string.Format("{0:D2}:{1:D2}.{2:D}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        timeText.text = "Time:\n" + timeStr;
        Destroy(persistent.gameObject);
    }
}
