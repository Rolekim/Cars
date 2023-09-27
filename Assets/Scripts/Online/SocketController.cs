using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(OnlineController))]
public class SocketController : MonoBehaviour
{
    private Socket socket;

    public string serverIP = "192.168.1.85";
    public int serverSocket = 16384;

    private byte[] obuffer = new byte[2048];
    private byte[] ibuffer = new byte[2048];

    private OnlineController onlineMng;

    private void Awake()
    {
        onlineMng = GetComponent<OnlineController>();
    }

    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(serverIP), 16384);
        socket.Connect(remoteEP);

        StartCoroutine(OnReceived());

        Send("join|" + onlineMng.playersName);
    }

    public void Send(string str)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(str + "$");
        bytes.CopyTo(obuffer, 0);

        socket.Send(obuffer, bytes.Length, SocketFlags.None);
    }

    private IEnumerator OnReceived()
    {
        while (true)
        {
            if (socket.Available > 0)
            {
                int bytesReceived = socket.Available;

                byte[] strBuffer = new byte[bytesReceived];

                socket.Receive(ibuffer, socket.Available, SocketFlags.None);

                Buffer.BlockCopy(ibuffer, 0, strBuffer, 0, bytesReceived);
                string str = Encoding.ASCII.GetString(strBuffer);
                Debug.Log(str);

                onlineMng.ParseOnlineMessages(str);
            }

            yield return null;
        }
    }

    //private void Send(byte[] buffer, int length)
    //{
    //    socket.Send(buffer, length, SocketFlags.None);
    //}
}