using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Text thrustText, brakeText, steeringText;

    [SerializeField]
    private Slider thrustSlider, brakeSlider;

    [SerializeField]
    private Image stearingImage;

    [SerializeField]
    private Image [] wheelsImages;

    [SerializeField]
    private CarController car;

    private void Awake()
    {
        car = GetComponent<CarController>();        
    }

    // Update is called once per frame
    void Update()
    {
        float wheelSlipAux = 0f;

        for (int i = 0; i < wheelsImages.Length; ++i)
        {
            wheelSlipAux = 1f - car.wheelsFordwardSlip[i] + car.wheelsSidewaysSlip[i];
            wheelsImages[i].color = new Color(wheelSlipAux, wheelSlipAux, wheelSlipAux);
        }

        thrustSlider.value = car.lastTorque / car.torque;
        brakeSlider .value = car.lastBreak  / car.maxBrakeTorque;
        stearingImage.rectTransform.localRotation = Quaternion.Euler (0f, 0f, -car.lastStearing);

        thrustText  .text = "Thrust: "   + car.lastTorque  .ToString ("F2");
        brakeText   .text = "Break: "    + car.lastBreak   .ToString ("F2");
        steeringText.text = "Stearing: " + car.lastStearing.ToString ("F2");
    }
}
