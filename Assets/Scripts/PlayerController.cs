using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rigidbody;
    private CarController carController;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        carController = GetComponent<CarController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        carController.VerticalInput   (Input.GetAxis ("Vertical")   );
        carController.HorizontalInput (Input.GetAxis ("Horizontal") );
        carController.Brake           (Input.GetAxis ("Jump")       );
    }
}


    
