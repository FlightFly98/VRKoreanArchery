using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    public enum UserLevel
    {
        Beginner,
        Gosu
    }

    public UserLevel gameLevel;

    public static bool isHit = false; // 화살이 맞았는가?

    public float poundForce; // 파운드

    public float fullLength; // 당긴 길이

    public float stringHeight = 0.14f; // 현고

    public float pulledDistance; // 당긴 길이 - 현고

    public float bowK; // 활 탄성 계수

    public float arrowWeight; // 화살 무게

    public float initialVelocity;// 화살 초기 속도

    public float launchAngle; // 화살 초기 각도

    public float gravity = 9.81f; // 중력 가속도

    public float kkagjHandRoll;

    public float zoomHandYaw;
    public float zoomHandRoll;

    
    public void SetGame()
    {
        switch (gameLevel)
        {
            case (UserLevel.Beginner):
                SetPoundForce("15");
                SetFullLength("??");
                //SetStringHeight
                SetPulledDistance();
                SetBowK();

                SetArrowWeight("7.5");
                break;

            case (UserLevel.Gosu):
                SetPoundForce(UIManager.instance.poundInfo);
                SetFullLength(UIManager.instance.bowInfo);
                //SetStringHeight
                SetPulledDistance();
                SetBowK();

                SetArrowWeight(UIManager.instance.arrowInfo);
                break;
        }
    }
    public float GetPoundForce()
    {
        return poundForce;
    }
    public void SetPoundForce(string inputPound)
    {
        float pound = 0f;
        pound = float.Parse(inputPound);
        poundForce = (pound * 0.453592f) * gravity;
        //Debug.Log("PoundForce: " + poundForce);
    }
    public float GetFullLength()
    {
        return fullLength;
    }
    public void SetFullLength(string inputBowType)
    {
        switch(inputBowType)
        {
            case "특장":
                fullLength = 2.85f * 0.303030f; // 자 -> m
                Debug.Log("fullLength: " + fullLength);
                break;
            case "장장":
                fullLength = 2.65f * 0.303030f;
                Debug.Log("fullLength: " + fullLength);
                break;
            default:
                fullLength = 0.81f;
                break;
        }
    }

    public void SetFullLength(float inputLength)
    {
        fullLength = inputLength;
    }

    public float GetStringHeight()
    {
        return stringHeight;
    }
    public void SetStringHeight(string inputStringHeight)
    {
        stringHeight = float.Parse(inputStringHeight);
    }
    public float GetPulledDistance()
    {
        return pulledDistance;
    }
    public void SetPulledDistance()
    {
        pulledDistance = fullLength - stringHeight;
        //Debug.Log("pulledDistance: " + pulledDistance);
    }
    public float GetArrowWeight()
    {
        return arrowWeight;
    }
    public void SetArrowWeight(string inputArrow)
    {
        arrowWeight = float.Parse(inputArrow) * 0.00375f; // 돈 -> kg
        //Debug.Log("arrowWeight: " + arrowWeight);
    }
    public float GetBowK()
    {
        return bowK;
    }
    public void SetBowK()
    {
        bowK = poundForce / fullLength; // K = F / x
        //Debug.Log("bowK: " + bowK);
    }

    public float GetInitialVelocity()
    {
        if (!isHit)
        {
            Debug.Log("initialVelocity: " + initialVelocity);
            return initialVelocity;
        }
        else
            return 0f;
    }
    public void SetInitialVelocity(float distance)
    {
        SetFullLength(distance);
        // SetPulledDistance();
        SetBowK();

        initialVelocity =
            Mathf.Sqrt((GetBowK() * Mathf.Pow(GetFullLength(), 2)) / GetArrowWeight());
    }

    public float GetlaunchAngle()
    {
        if (!isHit)
        {
            Debug.Log("Angle: " + launchAngle);
            return launchAngle;
        }  
        else
            return 0f;
    }
    public void SetlaunchAngle(float angle)
    {
        launchAngle = angle; // 각도 설정
    }

    public float GetkkagjHandRoll()
    {
        return kkagjHandRoll;
    }

    public void SetkkagjHandRoll(float roll)
    {
        kkagjHandRoll = roll;
    }

    public float GetZoomHandYaw()
    {
        return zoomHandYaw;
    }

    public void SetZoomHandYaw(float yaw)
    {
        zoomHandYaw = yaw;
    }

    public float GetZoomHandRoll()
    {
        return zoomHandRoll;
    }

    public void SetZoomHandRoll(float roll)
    {
        zoomHandRoll = roll;
    }
}

