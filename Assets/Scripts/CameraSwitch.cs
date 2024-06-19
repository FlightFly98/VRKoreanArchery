using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Camera mainCamera;
    public Camera followCamera;
    private Transform arrow;
    public Vector3 offset = new Vector3(0, 0.1f, -0.001f);
    public float followSpeed = 2f;
    //public float switchBackDistance = 5f;

    public bool following = false;

    void Start()
    {
        mainCamera.enabled = true;
        followCamera.enabled = false;
        mainCamera.GetComponent<AudioListener>().enabled = true;
        followCamera.GetComponent<AudioListener>().enabled = false;
    }

    void Update()
    {
        //if (Player.instance.sw420)
        if(following && arrow != null)
        {
            Vector3 targetPosition = arrow.position - arrow.forward * offset.z + arrow.up * offset.y;
            followCamera.transform.position = Vector3.Lerp(followCamera.transform.position, targetPosition, Time.deltaTime * followSpeed);

            // 카메라가 화살을 바라보도록 설정
            followCamera.transform.LookAt(arrow);

            if(GameManager.isHit)
            {
                following = false;
                StartCoroutine(WaitAndSwitchToMainCamera(3f));
            }
        }
    }

    public void SwitchToFollowCamera(Transform arrowTransform)
    {
        mainCamera.enabled = false;
        followCamera.enabled = true;
        mainCamera.GetComponent<AudioListener>().enabled = false;
        followCamera.GetComponent<AudioListener>().enabled = true;
        followCamera.transform.position = Player.instance.transform.position + offset;
        arrow = arrowTransform;
        following = true;
    }

    public void SwitchToMainCamera()
    {
        mainCamera.enabled = true;
        followCamera.enabled = false;
        mainCamera.GetComponent<AudioListener>().enabled = true;
        followCamera.GetComponent<AudioListener>().enabled = false;
        arrow = null;
        following = false;
    }

    IEnumerator WaitAndSwitchToMainCamera(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SwitchToMainCamera();
    }
}
