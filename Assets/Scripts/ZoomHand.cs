using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ZoomHand : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning;
    public int port = 12346;

    private Vector3 acc_raw;
    private Vector3 gyro_raw;
    private float[] gyro_offset = {-850, 0, -90};
    private float pitch, roll, yaw;
    private float previousTime;

    private const float alpha = 0.98f;
    private const float gyroSensitivity = 16.4f;

    private KalmanFilter kalmanFilterPitch = new KalmanFilter();
    private KalmanFilter kalmanFilterRoll = new KalmanFilter();

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
        // "Accel: ax, ay, az, Gyro: gx, gy, gz"
        string[] parts = data.Split(',');
        if (parts.Length == 6)
        {
            float ax, ay, az, gx, gy, gz;
            float.TryParse(parts[0].Split(':')[1].Trim(), out ax);
            float.TryParse(parts[1].Trim(), out ay);
            float.TryParse(parts[2].Trim(), out az);
            float.TryParse(parts[3].Split(':')[1].Trim(), out gx);
            float.TryParse(parts[4].Trim(), out gy);
            float.TryParse(parts[5].Trim(), out gz);

            acc_raw = new Vector3(ax, ay, az);
            gyro_raw = new Vector3(gx, gy, gz);

            for(int i = 0; i < 3; i++) gyro_raw[i] = (gyro_raw[i] * 0.8f) + 0.2f * (gyro_raw[i] - gyro_offset[i]);

            //Debug.Log("Y: " + gyro_raw[1]);
            //Debug.Log("x: " + gyro_raw[0] + " y: " + gyro_raw[1] + " z: " + gyro_raw[2]);
        }
    }

    void AngleCal()
    {
        float currentTime = Time.time;
        float deltaTime = currentTime - previousTime;
        previousTime = currentTime;

        if(UIManager.instance.arrowWithdraw) yaw = 0; // 누적 값 초기화

        float gyroPitch = gyro_raw[0] / gyroSensitivity * deltaTime;
        float gyroRoll = gyro_raw[1] / gyroSensitivity * deltaTime;
        float gyroYaw = gyro_raw[2] / gyroSensitivity * deltaTime;
        
        float vecPitch = Mathf.Sqrt(Mathf.Pow(acc_raw[0], 2) + Mathf.Pow(acc_raw[2], 2));
        float accelPitch = Mathf.Atan2(acc_raw[1], vecPitch) * Mathf.Rad2Deg;
        
        float vecRoll = Mathf.Sqrt(Mathf.Pow(acc_raw[1], 2) + Mathf.Pow(acc_raw[2], 2));
        float accelRoll = Mathf.Atan2(acc_raw[0], vecRoll) * Mathf.Rad2Deg;

        pitch = alpha * (pitch + gyroPitch) + (1.0f - alpha) * accelPitch;
        roll = alpha * (roll + gyroRoll) + (1.0f - alpha) * accelRoll;
        yaw += gyroYaw;

        // 칼만 필터를 사용하여 pitch와 roll 보정
        pitch = kalmanFilterPitch.GetAngle(accelPitch, gyroPitch, deltaTime);
        roll = kalmanFilterRoll.GetAngle(accelRoll, gyroRoll, deltaTime);
        yaw += gyroYaw * deltaTime;

        // Debug.Log("Pitch: " + pitch + "Roll: " + roll);

        GameManager.instance.SetZoomHandYaw(yaw);
        GameManager.instance.SetlaunchAngle(-pitch);
    }
    void FixedUpdate()
    {
        AngleCal();
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
