﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DistanceMeasTool : Tool
{
    void OnEnable()
    {
        gameObject.GetComponent<Image>().sprite = TooltrayController.Instance._measureSprite;
    }

    GameObject measurementController;
    MeasurementControl measurementControl;
    bool isBeingUsed = false;
    GameObject _activeBckg;

    public override void ButtonInteract()
    {
        //Call initialisation method for the relevant tool
        isBeingUsed = !isBeingUsed;
        if (measurementControl == null)
        {
            measurementController = new GameObject("measurementController");
            measurementControl = measurementController.AddComponent<MeasurementControl>();
        }
        measurementController.SetActive(isBeingUsed); //if button is active then enable MoveFunction to listen for input
        Debug.Log(isBeingUsed);

        if (isBeingUsed)
        {
            TooltrayController.Instance.newTool = this;
            TooltrayController.Instance.SwitchTool();
            TooltrayController.Instance.activeTools.Add(this);
            TooltrayController.Instance.ActiveToolBckg(_activeBckg, 0, true, UIController.Instance.currentMode);
        }
        else
        {
            TooltrayController.Instance.ActiveToolBckg(_activeBckg, 0, false, UIController.Instance.currentMode);
        }
    }

    public override void DeactivateButton()
    {

    }
}
