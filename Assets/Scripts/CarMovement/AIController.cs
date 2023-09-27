using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField]
    private Circuit         circuit;
    private CarController   carController;

    [SerializeField]
    private float brakingSensibiliy = 0.5f; 

    [SerializeField]
    public  float           steeringSensitivity = 0.01f;

    [SerializeField]
    private Vector3         actualWP, nextWP;

    private int             currentWPIndex;

    [SerializeField]
    private float           minSpeedToBrake = 8f;
    [SerializeField]
    private float           minAcceleration = 8f;
    public  float           distanceToWPThreshold = 5f;
    public  float           distaceToAdvanceBreak = 10f;

    private float           currentSectionDistance;

    private void Start()
    {
        carController = GetComponent<CarController>();
        actualWP      = circuit.waypoints[currentWPIndex].transform.position;
        nextWP        = circuit.waypoints[(currentWPIndex + 1) % circuit.waypoints.Length].transform.position;

        currentSectionDistance = Vector3.Distance(actualWP, transform.position);
    }

    private void Update()
    {
        Debug.DrawLine(transform.position, actualWP, Color.blue);
    }

    void FixedUpdate()
    {
        AIMove();
    }

    private void AIMove()
    {
        actualWP = circuit.waypoints[currentWPIndex].transform.position;
        nextWP = circuit.waypoints[(currentWPIndex + 1) % circuit.waypoints.Length].transform.position;

        Vector3 localTarget          = transform.InverseTransformPoint(actualWP);
        Vector3 nextLocalTarget      = transform.InverseTransformPoint(nextWP);

        float   targetAngle          = Mathf.Atan2 (localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float   nextTargetAngle      = Mathf.Atan2 (nextLocalTarget.x, nextLocalTarget.z) * Mathf.Rad2Deg;

        float   distanceToNextWP     = Vector3.Distance (actualWP, transform.position);

        float   currentSectionFactor = distanceToNextWP / currentSectionDistance;
        float   speedFactor          = carController.currentSpeed / carController.maxSpeed;

        float   steer                = Mathf.Clamp (targetAngle * steeringSensitivity, -1f, 1f) *
                                       Mathf.Sign  (carController.currentSpeed);

        float   acceleration         = 1f;
        float   brake                = 0f;

        acceleration = Mathf.Lerp (minAcceleration, 1f, currentSectionFactor);

        if (currentSectionFactor < 0.5f && carController.currentSpeed > minSpeedToBrake)
        {
            brake = Mathf.Lerp ((-1f - Mathf.Abs(nextTargetAngle)) * brakingSensibiliy , 1f + speedFactor, 1f - currentSectionFactor);
        }

        brake = Mathf.Clamp01(brake);
        

        carController.VerticalInput   (acceleration);
        carController.HorizontalInput (steer);
        carController.Brake           (brake);

        if (distanceToNextWP < distanceToWPThreshold)
        {
            currentWPIndex = (currentWPIndex + 1) % circuit.waypoints.Length;
            actualWP       = circuit.waypoints[currentWPIndex].transform.position;
            nextWP         = circuit.waypoints[(currentWPIndex + 1) % circuit.waypoints.Length].transform.position;

            currentSectionDistance = Vector3.Distance(actualWP, transform.position);
        }
    }
}
