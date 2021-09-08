using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public Transform otherPoint;
    public float moveTime;
    private Vector3 startPos;
    private Vector3 endPos;

    private void Start()
    {
        startPos = transform.position;
        endPos = otherPoint.position;
    }

    private void FixedUpdate()
    {
        float time = Time.time;
        bool returning = Mathf.Floor(time / moveTime) % 2 == 0;
        float cycleTime = time % moveTime;
        Vector3 from = returning ? endPos : startPos;
        Vector3 to = returning ? startPos : endPos;
        transform.position = Vector3.Lerp(from, to, cycleTime / moveTime);
    }
}
