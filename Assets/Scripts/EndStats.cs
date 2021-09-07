using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndStats : MonoBehaviour
{
    public TextMeshProUGUI sprinklesText;
    public TextMeshProUGUI timeText;

    private void Start()
    {
        PersistentTracker persistent = FindObjectOfType<PersistentTracker>();
        sprinklesText.text = "Sprinkles Collected:\n" + persistent.sprinkles;
        TimeSpan timeSpan = TimeSpan.FromSeconds(persistent.time);
        string timeStr = string.Format("{0:D2}:{1:D2}.{2:D}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        timeText.text = "Time:\n" + timeStr;
        Destroy(persistent.gameObject);
    }
}
