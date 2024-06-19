using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginnerButton : MonoBehaviour, BButton
{
    public void SetLevel()
    {
        GameManager.instance.gameLevel = GameManager.UserLevel.Beginner;
        UIManager.instance.checkInput = true;
    }
}
