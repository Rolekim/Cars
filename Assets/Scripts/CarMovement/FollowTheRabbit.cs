using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTheRabbit : MonoBehaviour
{
    public WaypointsFollower rabbit;

    public float     distanceToFollow;

    public float     rotationSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void LateUpdate()
    {
        //float targetRotationAngles  = rabbit.transform.eulerAngles.y;
        //float currentRotationAngles = rabbit.transform.eulerAngles.y;


        //float rotationY = Mathf.LerpAngle (currentRotationAngles, targetRotationAngles, rotationSpeed * Time.deltaTime);
        //Quaternion rotation = Quaternion.Euler (0f, rotationY, 0f);

        //transform.position = rabbit.transform.position;
        //transform.position -= (rotation * Vector3.forward) * distanceToFollow;

        //transform.LookAt (rabbit.transform);


        Vector3 direction = rabbit.transform.position - transform.position;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

        transform.Translate(0f, 0f, rabbit.speed * Time.deltaTime);
    }
}
