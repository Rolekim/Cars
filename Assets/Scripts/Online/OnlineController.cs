using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Globalization;
using UnityEngine;

[RequireComponent(typeof(SocketController))]
public class OnlineController : MonoBehaviour
{
    public string playersName = "Miguel";

    public GameObject playerGO;

    public List<OnlineCarController> onlineCars = new List<OnlineCarController>();

    private Dictionary<string, OnlineCarController> onlineCarsDict = new Dictionary<string, OnlineCarController>();

    public GameObject onlineCarPrefab;

    private SocketController socket;

    public float timeToSendData = 0.33f;
    private float timeToSendDataAux = 0f;

    void Awake()
    {
        socket = GetComponent<SocketController>();
    }

    void LateUpdate()
    {
        timeToSendDataAux -= Time.deltaTime;

        if (timeToSendDataAux <= 0f)
        {
            SendPosition();
            SendRotation();

            timeToSendDataAux = timeToSendData;
        }
    }

    void SendPosition()
    {
        string currentPosition = "updatePosition|" + playersName + "|" + playerGO.transform.position.ToString();
        socket.Send(currentPosition);
    }

    void SendRotation()
    {
        string currentRotation = "updateRotation|" + playersName + "|" + playerGO.transform.rotation.ToString();
        socket.Send(currentRotation);
    }

    public void ParseOnlineMessages(string str)
    {
        string[] strSplit = str.Split('$');

        for (int i = 0; i < strSplit.Length - 1; i++)
        {
            ParseOnlineMessage(strSplit[i]);
        }
    }

    public void ParseOnlineMessage(string str)
    {
        string[] strSplit = str.Split('|');

        if (strSplit.Length > 0)
        {
            switch (strSplit[0])
            {
                case "join":
                    NewPlayerJoin(strSplit[1]);
                    break;

                case "updatePosition":
                    ReceivePosition(strSplit[1], strSplit[2]);
                    break;

                case "updateRotation":
                    ReceiveRotation(strSplit[1], strSplit[2]);
                    break;
            }
        }
    }

    void NewPlayerJoin(string name)
    {
        Debug.Log("Se une");

        if (!onlineCarsDict.ContainsKey(name))
        {
            Debug.Log("Llega");
            GameObject newCar = Instantiate(onlineCarPrefab, new Vector3(2, 0, 0), Quaternion.identity);
            newCar.name = name;
            onlineCars.Add(newCar.GetComponent<OnlineCarController>());

            onlineCarsDict.Add(name, newCar.GetComponent<OnlineCarController>());
        }

        // inform the new player about this clients car
        string startConnectionStr = "join|" + playersName;
        socket.Send(startConnectionStr);
    }

    void ReceivePosition(string name, string positionStr)
    {
        Vector3 targetPosition = StringToVector3(positionStr);
        onlineCarsDict[name].targetPosition = targetPosition;
    }

    void ReceiveRotation(string name, string quaternionStr)
    {
        Quaternion targetRotation = StringToQuaternion(quaternionStr);
        onlineCarsDict[name].targetRotation = targetRotation;
    }

    private static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
            sVector = sVector.Substring(1, sVector.Length - 2);

        // split the items
        string[] sArray = sVector.Split(',');

        float wat = float.Parse("1.0", CultureInfo.InvariantCulture.NumberFormat);

        return new Vector3(
            float.Parse(sArray[0], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(sArray[1], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(sArray[2], CultureInfo.InvariantCulture.NumberFormat)
        );
    }

    static Quaternion StringToQuaternion(string sQuaternion)
    {
        // Remove the parentheses
        if (sQuaternion.StartsWith("(") && sQuaternion.EndsWith(")"))
        {
            sQuaternion = sQuaternion.Substring(1, sQuaternion.Length - 2);
        }

        // split the items
        string[] sArray = sQuaternion.Split(',');

        return new Quaternion(
            float.Parse(sArray[0], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(sArray[1], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(sArray[2], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(sArray[3], CultureInfo.InvariantCulture.NumberFormat)
        );
    }
}