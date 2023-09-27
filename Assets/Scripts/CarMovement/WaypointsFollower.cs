using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsFollower : MonoBehaviour
{
    public  Circuit circuit;

    private Vector3 nextWPPosition;

    private int     currentWaypoint     = 0;

    public  float distanceToWPThreshold = 1f;

    public  float speed;
    public  float rotationSpeed;

    // Start is called before the first frame update
    void Start()
    {
        nextWPPosition = circuit.waypoints[currentWaypoint].transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToWP = Vector3.Distance (this.transform.position, nextWPPosition);
        Vector3 direction = nextWPPosition - transform.position;

        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
        transform.LookAt(nextWPPosition);
        transform.Translate(0f, 0f, speed * Time.deltaTime);

        if (distanceToWP < distanceToWPThreshold)
        {
            currentWaypoint = (currentWaypoint + 1) % circuit.waypoints.Length;
            nextWPPosition = circuit.waypoints[currentWaypoint].transform.position;
        }
    }
}
