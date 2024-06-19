using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    
    // public Rigidbody body;
    bool isThisHit = false;
    private float ID;
    float Timedir = 0.0f;
    Vector3 calVector;
    float tmpGravity;
    float v0;
    float a0;
    float kkagjHandRoll = 0.0f;
    float zoomHandYaw;
    float elapsedTime = 0.0f;

    private CameraSwitch cameraSwitch;

    void Start()
    {
        ID = this.GetInstanceID();
        Timedir = Time.fixedDeltaTime;
        tmpGravity = GameManager.instance.gravity;

        v0 = GameManager.instance.GetInitialVelocity();
        a0 = GameManager.instance.GetlaunchAngle() * Mathf.Deg2Rad;

        zoomHandYaw = GameManager.instance.GetZoomHandYaw();
        kkagjHandRoll = GameManager.instance.GetkkagjHandRoll();

        // 화살 초기 각도 설정
        transform.rotation = Quaternion.Euler(-GameManager.instance.GetlaunchAngle(), kkagjHandRoll, zoomHandYaw);

        // Debug.Log("V0: " + v0 + " A0: " + a0);
        calVector = new Vector3(
            0,
            v0 * Mathf.Sin(a0),
            v0 * Mathf.Cos(a0)
        );

        cameraSwitch = FindObjectOfType<CameraSwitch>();
        cameraSwitch.SwitchToFollowCamera(this.transform);
        // Debug.Log(v0 * Mathf.Sin(a0 * Mathf.Deg2Rad) + " " + v0 * Mathf.Cos(a0 * Mathf.Deg2Rad));
    }
    void ArrowCalPosition()
    {
        elapsedTime += Time.fixedDeltaTime;
        Vector3 position = transform.position +
                           calVector * elapsedTime -
                           new Vector3(0, 0.5f * tmpGravity * elapsedTime * elapsedTime, 0);

        transform.position = position;

        if (calVector != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(calVector + new Vector3(0, tmpGravity, 0) * elapsedTime);
        }

        Debug.Log(position.z + " " + position.y);
        if (transform.position.y < 0)
        {
            Debug.Log("Time to hit the ground: " + elapsedTime + " seconds");
            Debug.Log("z: " + transform.position.z);
            isThisHit = true;
        }
    }
    void ArrowCalVelocity()
    {
        calVector.z = v0 * Mathf.Cos(a0) * Timedir;
        calVector.y = v0 * Mathf.Sin(a0) * Timedir * GameManager.instance.gravity;

        transform.Translate(calVector);
        transform.Rotate(new Vector3(Mathf.Cos(a0 * Mathf.PI / 180.0f), 0, 0));
    }

    void FixedUpdate()
    {
        if (!GameManager.isHit && !isThisHit)
            //ArrowCalPosition();
            ArrowCalVelocity();
    }

    private void OnTriggerEnter(Collider other)
    {
        isThisHit = true;
        Debug.Log(other.tag);
    }
}
