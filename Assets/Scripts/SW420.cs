using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ShockSensorReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning;
    public int port = 12350;  // SW-420

    void Start()
    {
        udpClient = new UdpClient(port);
        isRunning = true;
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);
        while (isRunning)
        {
            try
            {
                if (udpClient.Available > 0)
                {
                    byte[] data = udpClient.Receive(ref anyIP);
                    string text = Encoding.UTF8.GetString(data);
                    ProcessData(text);
                }
                else
                {
                    Thread.Sleep(10); // Add a short sleep to prevent tight looping
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }

    private void ProcessData(string data)
    {
        if (data == "Shock detected")
        {
            Debug.Log("Shock detected!");
            Player.instance.sw420 = true;
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (receiveThread != null)
        {
            receiveThread.Join(); // Wait for the thread to finish
            receiveThread = null;
        }
        udpClient.Close();
    }
}
