﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.EventSystems;


// LOCKED VERSION OF LUKE'S SCRIPT WHICH CONSTRAINS THE LINE TO BE HORIZONTAL OR VERTICAL

/* Script controls the processs of constructing lines, as well as storing and displaying these.
 * Left click on mesh colliders to place the start and end points. 
 * Right clicking anywhere resets this process at any point.
 * One two points are placed, left clicking on any collider for a third time acts to save the line, and writes the distance to the console
 * 
 * IMPLEMENTATION: Attach to some GameObject to use as a controller, needs hooking up to a camera and a marker prefab
 * in the inspector. No need to attach anything to the line renderer component.
 * 
 * The marker prefab is just a primative 3d shape WITHOUT a mesh collider - it marks the start and end of a line being constructed
 * 
 * Then create a UI toggle, and for the On Value Changed event of the toggle, add the OnChange function of this script as a listener
 * 
 * This provides the basic functionality, excluding control of layers from the UI - do this from the inspector
*/


public class DetectorMeasurementControl_LOCKED : MonoBehaviour

{
    public GameObject markerPrefab;
    public DetectorBehaviour detector;
    public Transform screenOrigin;
    Camera cam;

    GameObject markerOne; //Simple objects to mark the start and end of lines under construction
    GameObject markerTwo;
    bool modeActive; //Whether UI checkbox is ticked
    int clicks; //Used for line creation control flow

    [SerializeField] [Range(1,8)] int digits; //number of digits stored for position data - affects snapping intensity
    [SerializeField] [Range(1, 10)] int currentLayer;

    Vector3 startPoint; //Local variables of line under construction, before they are written to lineData 
    Vector3 endPoint;    
    string startTag;
    string endTag;

    private LineRenderer line;
    List<MeasurementLine> lineData = new List<MeasurementLine>();                   //Data structure holding line data
    public List<GameObject> lineObjects = new List<GameObject>();                  //Current list of lines being shown in the current layer
    Dictionary<string, Vector3> positionCache = new Dictionary<string, Vector3>(); //GameObjects are found by name for their global position and rotations
    Dictionary<string, Quaternion> rotationCache = new Dictionary<string, Quaternion>(); //These caches store them temporarily if they have been previously requested.

    //screen parameters + data
    private float screenWidth, screenHeight, screenResolution, matrixResolution, offset;
    private float[,] matrix;
    [SerializeField] string file;

    bool isCursorOverButton;

    private void Start()
    {
        file = Application.dataPath + "/img_reduced.txt";
        cam = UIController.Instance.screenCam;
        clicks = 0;
        modeActive = false;
        line = GetComponent<LineRenderer>();
    }

    public void OnChange(bool ticked)
        // Activated upon any change to checkbox
    {
        modeActive = ticked;

        if (ticked)
        {
            CreateTool();
            SetLineProperties(line);
        }
        else
        {            
            EndTool();
        }        
    }

    private void Update()
    {
        if (modeActive)
        {
            isCursorOverButton = EventSystem.current.IsPointerOverGameObject();
            Ray ray = cam.ScreenPointToRay(Input.mousePosition); //Get user input based on click from camera in game view
            RaycastHit hit;                                    //Currently calling this every frame for debugging purposes
            //Debug.DrawRay(ray.origin, ray.direction * 20, Color.yellow);
            if (!isCursorOverButton)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    switch (clicks)
                    {
                        case 0:
                            // first marker
                            TooltrayController.Instance.dynamicButtons[1].SetActive(false);
                            ObjectManager.Instance.EmailManager.transform.Find("Canvas").gameObject.SetActive(false);
                            startPoint = RoundVector(hit.point, digits); //round position of position marker to arbritrary precision
                            markerOne.transform.position = startPoint;
                            line.SetPosition(0, startPoint);
                            startTag = hit.collider.name;

                            markerOne.SetActive(true);

                            if (Input.GetMouseButtonDown(0)) //if left click whilst over mesh collided
                            {
                                clicks += 1;
                                Debug.Log("LINE START: " + startPoint);
                            }
                            break;

                        case 1:
                            // second marker
                            endPoint = RoundVector(hit.point, digits);


                            // for the detector - the second point must be horizontal/vertical relative to the first point

                            // direction from first point to current mouse position
                            Vector3 dir = endPoint - startPoint;
                            // normalised projections along the x/y axes
                            float x_proj = Vector3.Dot(dir, new Vector3(1, 0));
                            float y_proj = Vector3.Dot(dir, new Vector3(0, 1));

                            //HORIZONTAL OR VERTICAL LOCKING
                            if (Mathf.Abs(x_proj) > Mathf.Abs(y_proj))
                            {
                                //greater projection along the horizontal axis - so lock horizontally
                                endPoint.x = x_proj + startPoint.x;
                                endPoint.y = startPoint.y;
                            }
                            else
                            {
                                //greater projection along the vertical axis - so lock vertically
                                endPoint.x = startPoint.x;
                                endPoint.y = y_proj + startPoint.y;
                            }


                            markerTwo.transform.position = endPoint;
                            line.SetPosition(1, endPoint);
                            endTag = hit.collider.name;

                            markerOne.SetActive(true);
                            markerTwo.SetActive(true);
                            line.enabled = true;

                            if (Input.GetMouseButtonDown(0))
                            {
                                clicks += 1;
                                StoreLine();
                                float distance = GetDistance(lineData[lineData.Count - 1]);
                                Debug.Log(distance);
                                Debug.Log("LINE END: " + endPoint);
                                GenerateData();
                                TooltrayController.Instance.dynamicButtons[1].SetActive(true);
                            }
                            break;

                        case 2:
                            // set line and output length
                            if (Input.GetMouseButtonDown(0))
                            {
                                // first fetch the appropriate row/column of data and write to text file                    
                                // then deal with the line
                                //StoreLine();
                                DisableMarkers();
                                clicks = 0;
                                //DrawLine(lineData[lineData.Count - 1]); //Local variables are cleared so reload line from storage without markers

                                //Debug.Log("DISTANCE: " + GetDistance(lineData[lineData.Count - 1])); //Distance output - feel free to hook up to UI
                            }
                            break;

                        default:
                            //catchall for any edge cases
                            clicks = 0;
                            break;
                    }
                }
                else if (clicks < 2)
                {
                    DisableMarkers(); //Make lines and markers not visable if we hover away from objects, unless we have a complete line
                }

                if (Input.GetMouseButtonDown(1)) //right click resets measurement
                {
                    DisableMarkers();
                    clicks = 0;
                }
            }
        }
            
                        
    }


    // this function selects a subset of points from the Intensity matrix depending on the line drawn by the user - and writes this to the text file.
    private void GenerateData()
    {
        // 1. Transform start/end to LOCAL coordinates
        Vector3 localStartPoint = startPoint - screenOrigin.position;
        Vector3 localEndPoint = endPoint - screenOrigin.position;

        // 2. From the start/end points - calculate the local row/col bounds
        float x1 = localStartPoint.x; // horizontal bound 1
        float x2 = localEndPoint.x; // horizontal bound 1
        float y1 = localStartPoint.y; // vertical bound 1
        float y2 = localEndPoint.y; //vertical bound 2

        float x_left = Mathf.Min(x1, x2); // leftmost horizontal bound
        float x_right = Mathf.Max(x1, x2); // rightmost horizontal bound
        float y_bottom = Mathf.Min(y1, y2); // lowest vertical bound
        float y_top = Mathf.Max(y1, y2); // highest vertical bound

        // 3. fetch the parameters
        screenHeight = detector.ScreenHeight;
        screenWidth = detector.ScreenWidth;
        screenResolution = detector.Resolution;
        matrix = detector.Matrix;
        matrixResolution = matrix.GetLength(0);
        
        Debug.Log("----------------");
        Debug.Log("screenHeight: " + screenHeight);
        Debug.Log("screenWidth: " + screenWidth);
        Debug.Log("screenResolution: " + screenResolution);
        Debug.Log("matrixResolution: " + matrixResolution);

        // 4. transform the above bounds from local coordinates to matrix space
        bool oversampling = matrixResolution < screenResolution;

        
        x_left = x_left * screenResolution / screenWidth;
        x_right = x_right * screenResolution / screenWidth;
        y_bottom = y_bottom * screenResolution / screenHeight;
        y_top = y_top * screenResolution / screenHeight;

        Debug.Log("-------------------");
        Debug.Log("x left rounded: " + (int)x_left);
        Debug.Log("x_right rounded: " + (int)x_right);
        Debug.Log("y_bottom rounded: " + (int)y_bottom);
        Debug.Log("y_top rounded: " + (int)y_top);
        Debug.Log("-----------------");

        if (oversampling) //less pixels available than needed
        {
            // screen size is larger than the matrix size
            // need to generate black values beyond the bounds
            offset = (screenResolution - matrixResolution) / 2;
        }
        else // screen size the same as matrix size
        {
            offset = 0;
        }

        Debug.Log("Offset: " + offset);


        // 5. loop through all the data within the bounds and export to a text file
        FetchUserData((int)x_left, (int)x_right, (int)y_bottom, (int)y_top);
    }

    private void FetchUserData(int x_left, int x_right, int y_bottom, int y_top)
    {
        using (TextWriter tw = new StreamWriter(file))
        {
            tw.Write("x (m)" + "\t" + "y (m)" + "\t" + "Intensity");
            tw.WriteLine();

            for (int i = x_left; i <= x_right; i++)
            {
                for (int j = y_bottom; j <= y_top; j++)
                {
                    float position_x = (i - x_left) * screenWidth / screenResolution;
                    float position_y = (j - y_bottom) * screenHeight / screenResolution;

                    int i_transformed = (int) (i - offset);
                    int j_transformed = (int) (j - offset);

                    float intensity;

                    if (i_transformed < 0 || j_transformed <0 || i_transformed >= matrixResolution || j_transformed >= matrixResolution)
                    {
                        intensity = 0f;
                    }
                    else
                    {
                        intensity = matrix[j_transformed, i_transformed];//flipped i,j
                    }

                    tw.Write(position_x.ToString("#.00000") + "\t" + position_y.ToString("#.00000") + "\t" +  intensity.ToString("#.00000"));
                    tw.WriteLine();
                }
            }
        }
        Debug.Log("File Saved: " + file);
    }

    Vector3 RoundVector(Vector3 objectPosition, int digits) //Method to handle rounding of transform components
    {
        //INPUT vector3 origin which gives a local origin to centre positions around

        float x = Round(objectPosition.x, digits);
        float y = Round(objectPosition.y, digits);
        float z = Round(objectPosition.z, digits);

        return new Vector3(x, y, z);
    }

    private float Round(float component, int digits) //Performs rounding to arbritrary accuracy
    {
        float multiplier = Mathf.Pow(10, digits);
        return Mathf.Round(component * multiplier) / multiplier; 
    }
    private void DisableMarkers()
    {
        if (clicks != 1)
        {
            markerOne.SetActive(false);
        }
        markerTwo.SetActive(false);
        line.enabled = false;
    }



    private void CreateTool()   //Handles creation of tool upon ticking of box in UI to enable tool
    {        
        markerOne = Instantiate(markerPrefab);
        markerTwo = Instantiate(markerPrefab);
        line.enabled = false;
        //LoadLines();
    }

    private void EndTool()     //disable and destroy all objects, reset clicks to zero
    { 
        clicks = 0;
        TooltrayController.Instance.dynamicButtons[1].SetActive(false);
        ObjectManager.Instance.EmailManager.transform.Find("Canvas").gameObject.SetActive(false);
        GameObject.Destroy(markerOne);
        GameObject.Destroy(markerTwo);
        line.enabled = false;
        ClearAllLines();        
    }

    void SetLineProperties(LineRenderer line)
    {
        line.positionCount = 2;
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        line.generateLightingData = true;
        line.numCornerVertices = 10;
        //line.material.color = Color.red;
        line.receiveShadows = false;
        line.shadowBias = 100f;
        //Debug.Log(line.endWidth);
    }

    public void OnLayerChange(int newLayer)
    {
        if (modeActive)
        {
            currentLayer = newLayer;
            ClearAllLines();
            LoadLines();
        }
    }

    private void StoreLine()
    {
        Vector3 globalStart = FindGlobalPos(startTag);          //Get required global positions and rotations, as we convert the global points 
        Vector3 globalEnd = FindGlobalPos(endTag);              //of the line into local positions (at zero rotation)
        Quaternion startRotation = FindGlobalRotation(startTag);
        Quaternion endRotation = FindGlobalRotation(endTag);

        Vector3 localStart = Quaternion.Inverse(startRotation) * (startPoint - globalStart) ;
        Vector3 localEnd = Quaternion.Inverse(endRotation) * (endPoint - globalEnd);


        lineData.Add(new MeasurementLine(localStart, localEnd, startTag, endTag, currentLayer));
    }

    void DrawLine(MeasurementLine lineData)
    {
        GameObject lineObject = new GameObject();
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        SetLineProperties(line);

        line.SetPosition(0, (FindGlobalRotation(lineData.StartTag) * lineData.StartLocation) + FindGlobalPos(lineData.StartTag)); //adds the local position of the line to the global position of the relevant object
        line.SetPosition(1, (FindGlobalRotation(lineData.EndTag) * lineData.EndLocation) + FindGlobalPos(lineData.EndTag));

        line.name = lineData.Layer.ToString();
        lineObjects.Add(lineObject);
    }

    void ClearAllLines()
    {
        foreach (var item in lineObjects)
        {
            Destroy(item);
        }
        lineObjects.Clear();
        positionCache.Clear();
        rotationCache.Clear();
    }

    void LoadLines()
    {
        foreach (var item in lineData)
        {
            if (item.Layer == currentLayer)
            {
                DrawLine(item);
                //Debug.Log(GetDistance(item));
            }
        }
    }


    Vector3 FindGlobalPos(string Name)
    //search positioncache dictionary for name
    //if not present then find gameobject in scene by name
    //return this vector
    {
        if (positionCache.TryGetValue(Name, out Vector3 globalPos))
        {
            return globalPos;
        }
        else
        { 
            GameObject currentObject = GameObject.Find(Name);
            Vector3 position = currentObject.transform.position;
            positionCache.Add(Name, position); //add this position to cache for future reference
            return position;
        }
    }

    Quaternion FindGlobalRotation(string Name)
    //search rotationcache dictionary for name
    //if not present then find gameobject in scene by name
    //return this quaternion
    {
        if (rotationCache.TryGetValue(Name, out Quaternion globalRot))
        {
            return globalRot;
        }
        else
        {
            GameObject currentObject = GameObject.Find(Name);
            Quaternion rotation = currentObject.transform.rotation;
            rotationCache.Add(Name, rotation); //add this rotation to cache for future reference
            return rotation;
        }
    }   

    float GetDistance(MeasurementLine lineData)             //Returns distance from an entry of lineData
    {
        Vector3 globalStart = lineData.StartLocation + FindGlobalPos(lineData.StartTag);
        Vector3 globalEnd = lineData.EndLocation + FindGlobalPos(lineData.EndTag);
        return (Vector3.Distance(globalStart, globalEnd));
    }       

}
