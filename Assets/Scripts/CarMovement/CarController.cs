using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TractionType
{
    RWD,
    FWD,
    AWD
}

public class CarController : MonoBehaviour
{
    private Rigidbody           rigidbody;

    [SerializeField]
    private AudioSource         skidSound;
    [SerializeField]
    private AudioSource         engineSound;


    public  WheelController[]   wheels;

    public  WheelController[]   frontWheels;

    public  WheelController[]   backWheels;

    [HideInInspector]
    public  float []            wheelsFordwardSlip;

    [HideInInspector]
    public  float []            wheelsSidewaysSlip;  

    private Quaternion          initialRotation;

    public  float               maxSpeed;
    public  float               torque;
    //[HideInInspector]
    public  float               lastTorque = 0;
    public  float               maxSteerAngle = 30f;
    [HideInInspector]
    public  float               lastStearing;
    public  float               maxBrakeTorque = 500f;
    [HideInInspector]
    public  float               lastBreak;

    public  ParticleSystem      smokePartycleSystem;
    private ParticleSystem[]    skidSmokes = new ParticleSystem[4];
    public  Transform           skidTrailPrefab;
    private Transform[]         skidTrails;

    public  GameObject[]        breakLights;

    public  float               gearLength = 3f;
    public  float               currentSpeed
    {
        get { return rigidbody.velocity.magnitude * gearLength; }
    }
    public  float               maxGearSpeed = 300f;
    public  int                 numGears = 5;
    private float               gearProp = 1 / 5f;

    private float               rpm;
    private int                 currentGear = 1;
    private float               currentGearProp ;
    public  float               lowMotorAudioPitch = 1f;
    public  float               highMotorAudioPitch = 6f;

    public  float               antiRoll = 5000f;

    public TractionType traction;

    public float SkidThreshold = 0.8f;

    private void Start()
    {
        wheelsFordwardSlip = new float[4];
        wheelsSidewaysSlip = new float[4];
        initialRotation = transform.rotation;
        rigidbody = GetComponent<Rigidbody>();
        skidTrails = new Transform[wheels.Length];
        Transform centerOfMass = transform.Find("CenterOfMass");

        if(centerOfMass)
        {
            rigidbody.centerOfMass = centerOfMass.localPosition;
        }

        InstatiateSmokeParticleSystem();

    }

    private void Update()
    {
        CalculateEngineSound();
    }

    private void InstatiateSmokeParticleSystem()
    {
        for (int i = 0; i < skidSmokes.Length; i++)
        {
            skidSmokes[i] = Instantiate(smokePartycleSystem);
            skidSmokes[i].Stop();
        }
    }

    private void FixedUpdate()
    {
        VisualRepresentation();
        //StabilizerBar();
        CheckSkid();
    }

    public Rigidbody ReturnRigidbody()
    {
        return rigidbody;
    }

    public void VerticalInput (float input)
    {
       
        float thrustTorque = (currentSpeed < maxSpeed) ? input * torque : 0f;

        if (rigidbody.velocity.magnitude < maxSpeed)
        {
            thrustTorque = input * torque;
        }

        switch (traction)
        {
            case TractionType.RWD:
                {
                    foreach (WheelController wheel in backWheels)
                    {
                        wheel.wheelCollider.motorTorque = thrustTorque;
                    }
                    break;
                }

            case TractionType.FWD:
                {
                    foreach (WheelController wheel in frontWheels)
                    {
                        wheel.wheelCollider.motorTorque = thrustTorque;
                    }
                    break;
                }

            case TractionType.AWD:
                {
                    foreach (WheelController wheel in wheels)
                    {
                        wheel.wheelCollider.motorTorque = thrustTorque;
                    }
                    break;
                }
        }

        lastTorque = thrustTorque;
    }

    public void HorizontalInput (float input)
    {
        foreach (WheelController wheel in frontWheels)
        {
            wheel.wheelCollider.steerAngle = input * maxSteerAngle;
        }

        lastStearing = input * maxSteerAngle;
    }

    public void Brake (float input)
    {
        float brakeImpulse = input * maxBrakeTorque;

        foreach (WheelController wheel in wheels)
        {
            wheel.wheelCollider.brakeTorque = brakeImpulse;
        }

        if (input > float.Epsilon)
        {
            foreach (GameObject breakLightGO in breakLights)
            {
                breakLightGO.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject breakLightGO in breakLights)
            {
                breakLightGO.SetActive(false);
            }
        }

        lastBreak = brakeImpulse;
    }

    public void ResetPosition()
    {
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        transform.position = new Vector3(transform.position.x, 2f, transform.position.z);
        transform.rotation = initialRotation;

        rigidbody.ResetInertiaTensor();

        foreach (WheelController wheel in wheels)
        {
            wheel.wheelCollider.brakeTorque = float.MaxValue;
        }
    }

    private void CheckSkid()
    {
        int wheelsSkidding = 0;
        for(int index = 0; index < wheels.Length; ++index)
        {
            WheelHit wheelHit;
            wheels[index].wheelCollider.GetGroundHit(out wheelHit);

            wheelsFordwardSlip[index] = Mathf.Abs (wheelHit.forwardSlip );
            wheelsSidewaysSlip[index] = Mathf.Abs (wheelHit.sidewaysSlip);

            if(Mathf.Abs(wheelHit.forwardSlip) >= SkidThreshold || Mathf.Abs(wheelHit.sidewaysSlip) >= SkidThreshold)
            {
                wheelsSkidding++;
                StartSkidTrail(index);
                skidSmokes[index].transform.position =
                    wheels[index].wheelCollider.transform.position -
                    wheels[index].wheelCollider.transform.up *
                    wheels[index].wheelCollider.radius;

                skidSmokes[index].Emit(1);
            }
            else
            {
                EndSkidTrail(index);
            }
        }

        if(wheelsSkidding > 0)
        {
            skidSound.volume = wheelsSkidding / (float)wheels.Length;

            if (!skidSound.isPlaying)
            {
                skidSound.Play();
            }
        }
        else if (skidSound.isPlaying)
        {
            skidSound.Stop();
        }
    }

    private void StartSkidTrail(int i)
    {
        if(skidTrails[i] == null)
        {
            skidTrails[i] = Instantiate(skidTrailPrefab);
        }

        skidTrails[i].parent = wheels[i].transform;
        skidTrails[i].localRotation = Quaternion.Euler(90f, 0f, 0f);
        skidTrails[i].localPosition = -Vector3.up * wheels[i].wheelCollider.radius;

    }

    private void EndSkidTrail(int i)
    {
        if (skidTrails[i] == null)
            return;

        Transform skidTrail = skidTrails[i];
        skidTrails[i] = null;
        skidTrail.parent = null;
        skidTrail.rotation = Quaternion.Euler(90f, 0f, 0f);
        Destroy(skidTrail.gameObject, 30);
    }

    // Visual Representation
    private void VisualRepresentation()
    {
        Vector3 position;
        Quaternion rotation;

        foreach (WheelController wheel in wheels)
        {
            wheel.wheelCollider.GetWorldPose(out position, out rotation);

            wheel.mesh.position = position;
            wheel.mesh.rotation = rotation;
        }
    }

    private void CalculateEngineSound()
    {
        float speedProp         = currentSpeed / maxGearSpeed;
        float targetGearFactor  = Mathf.InverseLerp (gearProp * currentGear, gearProp * (currentGear + 1), speedProp);

        currentGearProp         = Mathf.Lerp (currentGearProp, targetGearFactor, Time.deltaTime * 5f);

        float gearNumFactor     = currentGear / (float) numGears;
        rpm                     = Mathf.Lerp (gearNumFactor, 1, currentGearProp);

        float upperGearMax      = gearProp * (currentGear + 1);
        float downGearMax       = gearProp * currentGear;

        if (currentGear > 0 && speedProp < downGearMax)
        {
            currentGear--;
        }

        if (currentGear < (numGears - 1) && speedProp > upperGearMax)
        {
            currentGear++;
        }

        engineSound.pitch = Mathf.Lerp (lowMotorAudioPitch, highMotorAudioPitch, rpm) * 0.25f;
    }

    void StabilizerBar ()
    {
        GroundWheels (frontWheels[1].wheelCollider, frontWheels[0].wheelCollider);
        GroundWheels (backWheels[1] .wheelCollider, backWheels[0] .wheelCollider);
    }

    private void GroundWheels (WheelCollider leftWheel, WheelCollider rightWheel)
    {
        WheelHit  hit;
        float     leftTravel = 1f, rightTravel = 1f;

        // Calculate proportions of how grounded each wheel is.
        bool leftGrounded  = leftWheel.GetGroundHit (out hit);

        if (leftGrounded)
        {
            leftTravel     = (-leftWheel.transform.InverseTransformPoint(hit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;
        }

        bool rightGrounded = rightWheel.GetGroundHit(out hit);

        if (rightGrounded)
        {
            rightTravel    = (-rightWheel.transform.InverseTransformPoint(hit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;
        }

        float antiRollForce = (leftTravel - rightTravel) * antiRoll;

        if (leftGrounded)
        {
            rigidbody.AddForceAtPosition (leftWheel.transform.up * -antiRollForce, leftWheel.transform.position);
        }

        if (rightGrounded)
        {
            rigidbody.AddForceAtPosition (rightWheel.transform.up * -antiRollForce, rightWheel.transform.position);
        }
    }
}
