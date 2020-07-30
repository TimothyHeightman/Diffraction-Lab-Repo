﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MoveFunction : MonoBehaviour
{
    IMovable movableObject;
    IMovable tempObject;        
    Vector3 mOffset,startPos, endPos, expectedPos;
    string ID;
    bool isSelected, mouseDown = false;
    public float force = 1000f;
    Rigidbody rb;


    Camera mainCam;
    float cameraDist;
    float maxSpeed = 1000f;

    GameObject confirmHolder, denyHolder;
    Button confirmButton, denyButton;    

    List<Tuple<string, Vector3>> positionHistory;   //stores id of the object moved and the net translation in world space



    private void Start()
    {
        positionHistory = new List<Tuple<string, Vector3>>();
        mainCam = Camera.main;

        confirmHolder = new GameObject();
        denyHolder = new GameObject();

        SetUpButton(confirmHolder);
        SetUpButton(denyHolder);

        confirmButton = confirmHolder.AddComponent<Button>();
        denyButton = denyHolder.AddComponent<Button>();

        confirmButton.onClick.AddListener(ConfirmPlacement);
        denyButton.onClick.AddListener(DenyPlacement);

        confirmHolder.GetComponent<Image>().color = Color.green;
        denyHolder.GetComponent<Image>().color = Color.red;       
    }



    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //Get user input based on click from camera in game view
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                tempObject = hit.collider.gameObject.GetComponent<IMovable>();
                if (!isSelected & tempObject != null)
                {
                    movableObject = tempObject;
                    startPos = movableObject.pos;
                    ID = movableObject.ID;
                    rb = movableObject.rigidBody;
                    rb.isKinematic = false;
                    rb.freezeRotation = true;
                    isSelected = true;
                }
                cameraDist = Camera.main.WorldToScreenPoint(movableObject.pos).z;
                mOffset = movableObject.pos - GetMouseAsWorldPoint();                
            }
            else
            {
                tempObject = null;                
            }
        }


        if (tempObject != null && isSelected)         //If the selected object can be moved
        {
            if (Input.GetMouseButton(0))      //If we are holding the mouse down and have clicked on an object
            {                
                if (movableObject.ID == tempObject.ID)      //If we have selected the object currently being moved
                {
                    confirmHolder.SetActive(false);
                    denyHolder.SetActive(false);
                    expectedPos = GetMouseAsWorldPoint() + mOffset;                    
                }

            }

            if (Input.GetMouseButtonUp(0))
            {                
                endPos = movableObject.pos;
                
                confirmHolder.SetActive(true);
                denyHolder.SetActive(true);
                rb.velocity = Vector3.zero;
            }
        }

        if (movableObject != null && !mouseDown)
        {
            confirmButton.transform.position = mainCam.WorldToScreenPoint(movableObject.pos) + new Vector3(50, 0, 0);
            denyButton.transform.position = mainCam.WorldToScreenPoint(movableObject.pos) - new Vector3(50, 0, 0);
        }
    }



    private void FixedUpdate()
    {
        if (isSelected)
        {
            Vector3 expectedVelocity = (expectedPos - movableObject.pos) * force * Time.deltaTime;
            rb.velocity = new Vector3(Mathf.Clamp(expectedVelocity.x, -maxSpeed, maxSpeed), Mathf.Clamp(expectedVelocity.y, -maxSpeed, maxSpeed), Mathf.Clamp(expectedVelocity.z, -maxSpeed, maxSpeed));
        }
    }



    private Vector3 GetMouseAsWorldPoint()
    { 
        Vector3 mousePoint = Input.mousePosition;   
        mousePoint.z = cameraDist;                     
        return mainCam.ScreenToWorldPoint(mousePoint);      
    }

    void RecordMove()
    {
        Vector3 netMove = endPos - startPos;
        positionHistory.Add(new Tuple<string, Vector3>(ID, netMove));
        Debug.Log(new Tuple<string, Vector3>(ID, netMove));
    }

    void LoadLastMove()
    {
        Tuple<string, Vector3> entry = positionHistory[-1];
        IMovable[] movables = (IMovable[])FindObjectsOfType<GameObject>().OfType<IMovable>(); //need to check this works
        foreach (var item in movables)
        {
            if (item.ID == entry.Item1)
            {
                item.pos = entry.Item2;
                break;
            }
        }
    }

    void SetUpButton(GameObject rootObject)
    {
        rootObject.transform.parent = FindObjectOfType<Canvas>().transform;
        rootObject.AddComponent<RectTransform>().sizeDelta = new Vector2(20,20);
        rootObject.AddComponent<Image>();        
        rootObject.SetActive(false);

        //set listeners for methods
        //then on mouse up assign their positions to the selected object if we have one
    }

    void ConfirmPlacement()
    {
        RecordMove();
        isSelected = false;
        confirmHolder.SetActive(false);
        denyHolder.SetActive(false);
        movableObject.rigidBody.isKinematic = true;
    }

    void DenyPlacement()
    {
        movableObject.pos = startPos;
        isSelected = false;
        confirmHolder.SetActive(false);
        denyHolder.SetActive(false);
        movableObject.rigidBody.isKinematic = true;
    }
    

}

