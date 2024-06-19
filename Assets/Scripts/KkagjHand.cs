using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

public class KkagjHand : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning;
    public int port = 12347;

    private float rssi;
    private int movingAverageWindowSize = 5; // 이동 평균 필터 크기
    private Queue<float> rssiQueue = new Queue<float>();

    public float distance;

    // 칼만 필터
    private float kalmanRssi = -56f;
    private float kalmanGain = 0f;
    private float estimateError = 1f;
    private float measurementError = 1f;
    private float processNoise = 0.01f;

    // 이동할 수 있는 범위 설정 (예: 0.5m ~ 5m)
    private float minRange = 0.14f;
    private float maxRange = 0.9f;

    private Vector3 acc_raw;
    private Vector3 gyro_raw;
    private float[] gyro_offset = {-850, 0, 200};

    private float pitch, roll, yaw; // 각도 저장 변수
    private float previousTime; // 이전 시간 저장 변수

    // 보정 계수 (조정 필요)
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
        // 데이터 포맷: "Rssi: rssi, Accel: ax, ay, az, Gyro: gx, gy, gz"
        string[] parts = data.Split(',');
        float ax, ay, az, gx, gy, gz;
        if (parts.Length == 7)
        {
            float.TryParse(parts[0].Split(':')[1].Trim(), out rssi);
            //Debug.Log(rssi);
            float.TryParse(parts[1].Split(':')[1].Trim(), out ax);
            float.TryParse(parts[2].Trim(), out ay);
            float.TryParse(parts[3].Trim(), out az);
            float.TryParse(parts[4].Split(':')[1].Trim(), out gx);
            float.TryParse(parts[5].Trim(), out gy);
            float.TryParse(parts[6].Trim(), out gz);

            acc_raw = new Vector3(ax, ay, az);
            gyro_raw = new Vector3(gx, gy, gz);
            
            for(int i = 0; i < 3; i++) gyro_raw[i] = (gyro_raw[i] * 0.8f) + 0.2f * (gyro_raw[i] - gyro_offset[i]);
            
            //Debug.Log("x: " + gyro_raw[0] + " y: " + gyro_raw[1] + " z: " + gyro_raw[2]);
        }
    }
    
    public void ProcessRSSI()
    {
        //Debug.Log("Rssi: " + rssi);

        // 이동 평균 필터
        float filteredRSSI = ApplyMovingAverageFilter(rssi); 

        // RSSI -> 거리
        float convertDistance = ConvertRSSIToDistance(filteredRSSI);

        // 최소(현고 길이) ~ 최대(만작 길이)
        convertDistance = Mathf.Clamp(convertDistance, minRange, maxRange);

        // 칼만 필터
        float kalDistance = KalmanFilter(convertDistance);

        distance = kalDistance;

        GameManager.instance.SetInitialVelocity(distance);

        AngleCal();
    }

    void AngleCal()
    {
        // 시간 계산
        float currentTime = Time.time;
        float deltaTime = currentTime - previousTime;
        previousTime = currentTime;

        // 각도 변화 계산
        float gyroPitch = gyro_raw[0] / gyroSensitivity * Time.deltaTime;
        float gyroRoll = gyro_raw[1] / gyroSensitivity * Time.deltaTime;
        float gyroYaw = gyro_raw[2] / gyroSensitivity * Time.deltaTime;

        // 가속도계를 이용한 각도 계산
        float vecPitch = Mathf.Sqrt(Mathf.Pow(acc_raw[0], 2) + Mathf.Pow(acc_raw[2], 2));
        float accelPitch = Mathf.Atan2(acc_raw[1], vecPitch) * Mathf.Rad2Deg;
        
        float vecRoll = Mathf.Sqrt(Mathf.Pow(acc_raw[1], 2) + Mathf.Pow(acc_raw[2], 2));
        float accelRoll = Mathf.Atan2(acc_raw[0], vecRoll) * Mathf.Rad2Deg;

        // 저역 필터 + 자이로
        pitch = alpha * (pitch + gyroPitch) + (1.0f - alpha) * accelPitch;
        roll = alpha * (roll + gyroRoll) + (1.0f - alpha) * accelRoll;
        
        // 칼만 필터를 사용하여 pitch와 roll 보정
        pitch = kalmanFilterPitch.GetAngle(accelPitch, gyroPitch, deltaTime);
        roll = kalmanFilterRoll.GetAngle(accelRoll, gyroRoll, deltaTime);
        yaw += gyroYaw * deltaTime;

        //Debug.Log("pitch: " + pitch + " roll: " + roll + " yaw: " + yaw);

        // 깍지손 손등이 하늘을 보는지만 판별, 과도하지 않게 설정
        if(roll <= 0) roll = 0;
        else if (roll > 5) roll = 3;

        GameManager.instance.SetkkagjHandRoll(roll);
    }

    float ConvertRSSIToDistance(float rssi)
    {
        float txPower = -56f; // 1m에서의 RSSI 값
        if (rssi == 0)
        {
            return -1.0f; // 측정 불가
        }
        float ratio = rssi * 1.0f / txPower;
        if (ratio < 1.0)
        {
            return Mathf.Pow(ratio, 10);
        }
        else
        {
            return (0.89976f) * Mathf.Pow(ratio, 7.7095f) + 0.111f;
        }
    }

    float ApplyMovingAverageFilter(float newRSSI)
    {
        if (rssiQueue.Count >= movingAverageWindowSize)
        {
            rssiQueue.Dequeue();
        }

        rssiQueue.Enqueue(newRSSI);

        float sum = 0f;
        foreach (float rssi in rssiQueue)
        {
            sum += rssi;
        }

        return sum / rssiQueue.Count;
    }

    float KalmanFilter(float rssi)
    {
        // 칼만 이득 계산
        kalmanGain = estimateError / (estimateError + measurementError);

        // 업데이트된 RSSI 값 계산
        kalmanRssi = kalmanRssi + kalmanGain * (rssi - kalmanRssi);

        // 오차 갱신
        estimateError = (1 - kalmanGain) * estimateError + processNoise;

        return kalmanRssi;
    }
 
    void FixedUpdate()
    {
        ProcessRSSI();
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
