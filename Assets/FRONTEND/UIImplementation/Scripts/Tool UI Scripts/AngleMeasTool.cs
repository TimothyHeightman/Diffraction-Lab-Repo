﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AngleMeasTool : Tool
{
    // Start is called before the first frame update
    void OnEnable()
    {
        gameObject.GetComponent<Image>().sprite = TooltrayController.Instance._angleSprite;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void ButtonInteract()
    {
        //Call initialisation method for the relevant tool
    }
}
