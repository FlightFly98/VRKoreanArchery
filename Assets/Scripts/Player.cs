using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public static Player instance;

    public GameObject arrow;

    public UnityEvent arrowCreated;

    public bool valsi = false;

    public bool sw420 = false;

    public bool nakJeon = false;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        UIManager.instance.SetTLights();
    }

    public void Shoot()
    {
        GameManager.isHit = false;
        Instantiate(arrow, transform.position, Quaternion.identity);
    }
    void Update()
    {
        if(nakJeon)
        {
            Debug.Log("NakJeon detected!");
            nakJeon = true;
        }

        if(sw420 && !nakJeon && !UIManager.instance.arrowWithdraw)
        {
            Shoot();
            sw420 = false;
        }

        // 디버깅용
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
            sw420 = false;
        }
    }
}
