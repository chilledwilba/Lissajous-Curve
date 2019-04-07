using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEnableDisable : MonoBehaviour
{
    public Transform UIPanel;

    public void SwitchUIPanelState()
    {
        bool state = UIPanel.gameObject.activeSelf;
        UIPanel.gameObject.SetActive(!state);
    }
}