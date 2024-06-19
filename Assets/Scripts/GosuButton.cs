using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GosuButton : MonoBehaviour, BButton
{
    public void SetLevel()
    {
        GameManager.instance.gameLevel = GameManager.UserLevel.Gosu;
    }
}
