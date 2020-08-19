﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TakeDataTool : Tool
{
    public DetectorMeasurementControl_LOCKED detectorMeasure;

    GameObject _activeBckg;
    string activeName = "Take Data Active";

    GameObject _newTooltip, _newTooltipBckg;
    bool hoverBool;
    Button takeDataButton;
    string takeDataTooltip = "Take Data Tool";
   
    void OnEnable()
    {
        gameObject.GetComponent<Image>().sprite = TooltrayController.Instance._takeDataSprite;
        detectorMeasure = GameObject.Find("v2 DETECTOR").GetComponentInChildren<DetectorMeasurementControl_LOCKED>();
        takeDataButton = this.GetComponent<Button>();
    }

    bool isBeingUsed = false;

    public override void ButtonInteract()
    {
        isBeingUsed = !isBeingUsed; 

        if (isBeingUsed)
        {
            TooltrayController.Instance.newTool = this;
            TooltrayController.Instance.SwitchTool();
            TooltrayController.Instance.activeTools.Add(this);   
            UIController.Instance.mainCam.GetComponentInParent<CameraManager>().enabled = false;
            TooltrayController.Instance.ActiveToolBckg(_activeBckg, 0, true, activeName);      
            //cam.transform.position = new Vector3(-499.9f, 488.04f, 49.6f);                
        }

        UIController.Instance.SwitchCams(isBeingUsed);

        UIController.Instance.emailManager.SetActive(isBeingUsed);
        detectorMeasure.OnChange(isBeingUsed);
    }

    public override void DeactivateButton()
    {
        GameObject.Find("CameraFocus").GetComponent<CameraManager>().enabled = true;
        TooltrayController.Instance.ActiveToolBckg(_activeBckg, 0, false, activeName);
        UIController.Instance.emailManager.SetActive(false);
        detectorMeasure.OnChange(false);
        UIController.Instance.SwitchCams(false);      
    }

    public override void OnPointerEnter(PointerEventData data)
    {
        TooltipManager.Instance.OnHoverButtonActivateTooltip(takeDataButton, _newTooltip, _newTooltipBckg, takeDataTooltip);
    }

    public override void OnPointerExit(PointerEventData data)
    {
        TooltipManager.Instance.DeactivateTooltip(takeDataButton, _newTooltip, _newTooltipBckg, takeDataTooltip);
    }

}
