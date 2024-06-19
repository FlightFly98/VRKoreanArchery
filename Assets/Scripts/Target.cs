using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    int arrowHashCode = 0;
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Arrow" && arrowHashCode != other.GetHashCode())
        {
            GameManager.isHit = true;

            arrowHashCode = other.GetHashCode();

            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.isKinematic = true;

            UIManager.instance.SetHitCount();

            String targetName = this.gameObject.name;
            
            UIManager.instance.SetBlinkTLight(int.Parse(targetName.Substring(targetName.IndexOf("_") + 1).Trim()) - 1);         

            Debug.Log("관중");
        }

        Debug.Log(other.tag);
    }
}

