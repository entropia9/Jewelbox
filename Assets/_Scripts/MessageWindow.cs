using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectXformMover))]
public class MessageWindow : MonoBehaviour
{
    public TMP_Text messageText;
    
    void Awake()
    {
        GetComponent<RectXformMover>().MoveOff();
    }

    public void ShowMessage(string message="")
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

        
}
