using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    [HideInInspector]
    public  WheelCollider   wheelCollider;
    public  float           torque;
    public  float           maxSteerAngle = 30f;
    public  float           maxBrakeTorque = 500f;
    public  Transform       mesh;


    void Start()
    {
        wheelCollider   = GetComponent<WheelCollider> ();
        wheelCollider.ConfigureVehicleSubsteps (5, 12, 15);
    }
}
