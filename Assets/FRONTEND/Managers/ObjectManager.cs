﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    private static ObjectManager _instance;

    /* To add a new type of object, explicitly define a full property for the object and add to
     * the relevant list in Start() */ 


    /*Declare private serialized gameobject for each component to be managed
     * For objects present in the scene before runtime link these in the inspector
     * otherwise when instantiated this script will assign a reference to the correct variable */

    //[Header("Cameras")]
    [SerializeField]
    private GameObject mainCam, screenCam;

    //[Header("Optical Components")]
    [SerializeField]
    private GameObject board, laser, lens, grating, screen, cmos;

    //[Header("Managers")]
    [SerializeField]
    private GameObject propagationManager, emailManager, measureController, movementController, rotationController;

    //[Header("Object Lists")]
    [SerializeField]
    private List<GameObject> cameraList, componentList, managerList;
    private Dictionary<obj, GameObject> activeObjects;

    //[Header("Counters")]
    public int numCameras, numComponents, numManagers, numActive;


    //Property definitions (done so fields can remain serializable)
    public GameObject MainCam
    {
        get { return mainCam; }
        set 
        { 
            bool isNewValue = CheckUpdateCounter(mainCam, value);
            mainCam = value;
            if (isNewValue)
            {
                obj key = obj.camMain;
                UpdateActive(key, value);                
                UpdateCounters();
            }
        }
    }
    public GameObject ScreenCam
    {
        get { return screenCam; }
        set
        {
            bool isNewValue = CheckUpdateCounter(screenCam, value);            
            screenCam = value;
            if (isNewValue)
            {
                obj key = obj.camScreen;
                UpdateActive(key, value);
                UpdateCounters();
            }
        }
    }
    public GameObject Board
    {
        get { return board; }
        set
        {
            bool isNewValue = CheckUpdateCounter(board, value);
            board = value;
            if (isNewValue)
            {
                obj key = obj.board;
                UpdateActive(key, value);
                UpdateCounters();
            }
        }
    }
    public GameObject Laser
    {
        get { return laser; }
        set
        {
            bool isNewValue = CheckUpdateCounter(laser, value);
            laser = value;
            if (isNewValue)
            {
                obj key = obj.laser;
                UpdateActive(key, value);
                UpdateCounters();
            }
        }
    }

    public GameObject Lens
    {
        get { return lens; }
        set
        {
            bool isNewValue = CheckUpdateCounter(lens, value);
            lens = value;
            if (isNewValue)
            {
                obj key = obj.lens;
                UpdateActive(key, value);
                UpdateCounters();
            }
        }
    }
    public GameObject Grating
    {
        get { return grating; }
        set
        {
            bool isNewValue = CheckUpdateCounter(grating, value);
            grating = value;
            if (isNewValue)
            {
                obj key = obj.grating;
                UpdateActive(key, value);
                UpdateCounters();
            }
        }
    }
    public GameObject Screen
    {
        get { return screen; }
        set
        {
            bool isNewValue = CheckUpdateCounter(screen, value);
            screen = value;
            if (isNewValue)
            {
                obj key = obj.screen;
                UpdateActive(key, value);
                UpdateCounters();
            }
        }
    }
    public GameObject Cmos
    {
        get { return cmos; }
        set
        {
            bool isNewValue = CheckUpdateCounter(cmos, value);
            cmos = value;
            if (isNewValue)
            {
                obj key = obj.cmos;
                UpdateActive(key, value);
                UpdateCounters();
            }
        }
    }
    public GameObject PropagationManager
    {
        get { return propagationManager; }
        set
        {
            bool isNewValue = CheckUpdateCounter(propagationManager, value);
            propagationManager = value;
            if (isNewValue)
            {
                obj key = obj.propagation;
                UpdateActive(key, value);
                UpdateCounters();
            }
        }
    }
    public GameObject EmailManager
    {
        get { return emailManager; }
        set
        {
            bool isNewValue = CheckUpdateCounter(emailManager, value);
            emailManager = value;
            if (isNewValue)
            {
                obj key = obj.email;
                UpdateActive(key, value);
                UpdateCounters();
            }
        }
    }
    public GameObject MeasureController
    {
        get { return measureController; }
        set
        {
            bool isNewValue = CheckUpdateCounter(measureController, value);
            measureController = value;
            if (isNewValue)
            {
                obj key = obj.measure;
                UpdateActive(key, value);
                UpdateCounters();
            }
        }
    }

    public GameObject MovementController
    {
        get { return movementController; }
        set
        {
            bool isNewValue = CheckUpdateCounter(movementController, value);
            movementController = value;
            if (isNewValue)
            {
                managerList = new List<GameObject> { propagationManager, emailManager, measureController, rotationController };
                obj key = obj.rotation;
                UpdateActive(key, value);
                UpdateCounters();
            }
        }
    }
    public GameObject RotationController
    {
        get { return rotationController; }
        set
        {
            bool isNewValue = CheckUpdateCounter(rotationController, value);
            rotationController = value;
            if (isNewValue)
            {
                managerList = new List<GameObject> { propagationManager, emailManager, measureController, movementController, rotationController };
                obj key = obj.rotation;
                UpdateActive(key, value);
                UpdateCounters();                
            }
        }
    }

    //Object manager as singleton
    public static ObjectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("ObjectManager is NULL.");                
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;   
    }


    // Start is called before the first frame update
    void Start()
    {
        cameraList = new List<GameObject> { mainCam, screenCam };
        componentList = new List<GameObject> { board, laser, lens, grating, screen, cmos };
        managerList = new List<GameObject> { propagationManager, emailManager, measureController, rotationController };
        activeObjects = new Dictionary<obj, GameObject>();

        UpdateCounters();        
    }

    public void AddRef(obj objectID, GameObject value)
    {
        //Method to add reference to a gameobject by passing in the obj code, rather than using the property directly

        GetRefFromCode(objectID) = value; 
    }


    public ref GameObject GetRefFromCode(obj objectID)
    {
        //Returns a reference to the private variable given an object shortcode
        switch (objectID)
        {
            case obj.camMain:
                return ref mainCam;
            case obj.camScreen:
                return ref screenCam;
            case obj.board:
                return ref board;
            case obj.laser:
                return ref laser;
            case obj.lens:
                return ref lens;
            case obj.grating:
                return ref grating;
            case obj.screen:
                return ref screen;
            case obj.cmos:
                return ref cmos;
            case obj.propagation:
                return ref propagationManager;
            case obj.email:
                return ref emailManager;
            case obj.measure:
                return ref measureController;
            case obj.movement:
                return ref movementController;
            case obj.rotation:
                return ref rotationController;
            default:
                return ref mainCam;
        }

    }

    void UpdateCameraRefs()
    {
        //Grabs the main camera through the Camera class - all others by name
        mainCam = Camera.main.gameObject;
        screenCam = GameObject.Find("ScreenCam");

        numCameras = 2;
    }

    //METHODS BELOW RELATE TO CAMERAS, CURRENTLY BROKEN

    void UpdateActive(obj key, GameObject value)
    {
        if (activeObjects.ContainsKey(key) ^ value != null)
        {
            if (value == null)
            {
                activeObjects.Remove(key);
            }
            else
            {
                activeObjects.Add(key, value);
            }
        }
    }

    Boolean CheckUpdateCounter(GameObject property, GameObject value)
    {
        if (value == null ^ property == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void UpdateCounters()
    {
        //Updates counters by determining the status of the appropriate variables

        List<GameObject>[] tempObj = { cameraList, componentList, managerList };
        int[] tempCountArray = { 0, 0, 0 };
        //int[] tempCountArray = { numCameras, numComponents, numManagers };
        numActive = 0;

        //Loop through the elements of each temporary array above
        for (int i = 0; i < tempObj.Length; i++)
        {
            int tempCount = 0;

            //loop through list of gameobjects tempObj[i], where item is a gameobject
            foreach (var item in tempObj[i])
            {                
                if (item != null)
                {
                    tempCount += 1;
                    if (item.activeInHierarchy)
                    {
                        numActive += 1;
                    }
                }                
            }

            tempCountArray[i] = tempCount;
        }

        numCameras = tempCountArray[0];
        numComponents = tempCountArray[1];
        numManagers = tempCountArray[2];
    }

    public void CheckRequiredComponents()
    {
        bool flag = true;
        Debug.Log("live");
        foreach (var item in new GameObject[4] {cmos, lens, grating, laser })
        {
            if (item == null)
            {
                flag = false;
                break;
            }
        }
        
        if (flag)
        {
            UIController.Instance.data.gameObject.SetActive(true);
        }
    }
}
