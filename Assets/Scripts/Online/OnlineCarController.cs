using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineCarController : MonoBehaviour
{
    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public Quaternion targetRotation;

    public float updatePositionSpeed = 1f;
    public float updateRotationSpeed = 1f;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, updateRotationSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, targetPosition, updatePositionSpeed * Time.deltaTime);
    }
}