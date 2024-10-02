using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    int arrowHashCode = 0;

    string thisTargetName;

    void Start()
    {
        thisTargetName = this.gameObject.name;
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Arrow") // && arrowHashCode != other.GetHashCode())
        {
            GameManager.isHit = true;

            arrowHashCode = other.GetHashCode();

            //Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            //rb.constraints = RigidbodyConstraints.FreezeAll;
            //rb.isKinematic = true;

            UIManager.instance.SetHitCount();
            UIManager.instance.SetBlinkTLight(int.Parse(thisTargetName.Substring(thisTargetName.IndexOf('_') + 1)));            

            //Debug.Log("관중");
        }

        //Debug.Log(other.tag);
    }
}

