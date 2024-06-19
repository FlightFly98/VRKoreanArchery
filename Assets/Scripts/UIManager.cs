using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEditor;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject[] TLights;
    private Color originColor;
    Color tempColor;

    int targetNum;

    public Canvas startCanvas;
    public Canvas inputCanvas;
    public Canvas inGameCanvas;

    public bool checkInput = false;

    public bool arrowWithdraw = false;

    public TMP_InputField poundInputField;
    public TMP_InputField bowInputField;
    public TMP_InputField arrowInputField;

    public string poundInfo;
    public string bowInfo;
    public string arrowInfo;

    public Text hitTargetCountText;
    int hitCount = 0;

    void Awake()
    {
        instance = this;
    }

    public void SettingInputField()
    {
        poundInfo = poundInputField.GetComponent<TMP_InputField>().text;

        bowInfo = bowInputField.GetComponent<TMP_InputField>().text;

        arrowInfo = arrowInputField.GetComponent<TMP_InputField>().text;

        checkInput = true;
    }

    public void CanvasManager(Canvas previousCanvas, Canvas nextCanvas)
    {
        previousCanvas.gameObject.SetActive(false);
        nextCanvas.gameObject.SetActive(true);
    }

    public void SetHitCount()
    {
        hitCount++;
        hitTargetCountText.text = hitCount.ToString();
    }

    public void SetArrowWithdraw()
    {
        if(arrowWithdraw) { arrowWithdraw = false;}
        else { arrowWithdraw = true; }
    }

    public void StartMainScene()
    { 
       SceneManager.LoadScene(1);
    }

    public void OnclickRestart()
    {
       SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public void SetTLights()
    {
        TLights = GameObject.FindGameObjectsWithTag("TLight");
        // 신호등 배정
        for(int i = 0; i < 3; i++)
        {
            String nameTL = "TL";
            if(TLights[i].gameObject.name != nameTL + (i + 1).ToString())
            {
                for(int j = 0; j < 3; j++)
                {
                    if(TLights[j].gameObject.name == nameTL + (i + 1).ToString())
                    {
                        GameObject tmp = TLights[i];
                        TLights[i] = TLights[j];
                        TLights[j] = tmp;
                    }
                }
            }
        }
        // 색깔 부여
        for(int i = 0; i < 3; i ++){ originColor = TLights[i].GetComponent<Renderer>().material.color; }
    }
        
    public void SetBlinkTLight(int num)
    {
        targetNum = num;
        StartCoroutine(BlinkTLight());
    }
    IEnumerator BlinkTLight()
    {
        TLights[targetNum].GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
        tempColor = TLights[targetNum].GetComponent<Renderer>().material.color;

        int count = 0;
        while (count < 2)
        {
            while (TLights[targetNum].GetComponent<Renderer>().material.color.r > 0f)
            {
                tempColor.r -= 0.1f;
                TLights[targetNum].GetComponent<Renderer>().material.color = tempColor;
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.5f);

            while (TLights[targetNum].GetComponent<Renderer>().material.color.r < 1f)
            {
                tempColor.r += 0.1f;
                TLights[targetNum].GetComponent<Renderer>().material.color = tempColor;
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.5f);
            count++;
        }

        TLights[targetNum].GetComponent<Renderer>().material.color = originColor;
    }
}
