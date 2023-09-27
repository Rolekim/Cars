using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipCarCheck : MonoBehaviour
{
    private CarController carController;
    private Rigidbody     rb;

    private float lastTimeCheckedOk;
    public  float timeToResetCar = 3f;

    private void Awake()
    {
        carController = GetComponent<CarController>();
        rb            = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (transform.up.y > 0.5 || rb.velocity.magnitude > 1f)
        {
            lastTimeCheckedOk = Time.time;
        }

        if (Time.time > lastTimeCheckedOk + timeToResetCar)
        {
            carController.ResetPosition();
        }
    }
}
