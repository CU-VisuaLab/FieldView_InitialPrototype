/**********************************************************************
 * Alex Thompson
 * 8/11/2016
 * Class file for labeling the X and Y axes appropriately based on the
 * given data set. Class renders data points as spheres at their appropriate
 * coordinates. This class can plot a maximum of 3 different data sets in
 * one plot, however more data point prefabs can be added to increase
 * the maximum possible number of data sets.
 *********************************************************************/

using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class GraphRender3D : MonoBehaviour {

    public CSVReader CSV;
    public GameObject DataPoint0, DataPoint1, DataPoint2;
    public string[] columnHeaders;
    //public Vector3 position;
    public float size_scale;
    private GameObject XLabel, YLabel, ZLabel, xAxis, yAxis, zAxis;
    private ArrayList Points;
    //private GameObject visCanvas;
    public float[] xDomain;
    public float[] yDomain;
    public float[] zDomain;
    private float[] xRange;
    private float[] yRange;
    private float[] zRange;
    public float yx_ratio;
    private string[] mountainInfo;
    private GameObject[] dataPoints;
    private List<Mountain> remoteMountains = new List<Mountain>();
    private List<Mountain> localMountains = new List<Mountain>();

    public string databasePassword;

    // Use this for initialization
    void Start () {
        CSV.ParseData();
        CSV.SetDataSets();

        string[] headers = CSV.GetColumnHeads();

        XLabel = new GameObject("XLabel");// visCanvas.transform.Find("XLabel").gameObject;
        YLabel = new GameObject("YLabel"); // visCanvas.transform.Find("YLabel").gameObject;
        ZLabel = new GameObject("ZLabel"); // visCanvas.transform.Find("ZLabel").gameObject;
        xAxis = transform.Find("XAxis").gameObject;// visCanvas.transform.Find("XAxis").gameObject;
        yAxis = transform.Find("YAxis").gameObject;// visCanvas.transform.Find("YAxis").gameObject;
        zAxis = transform.Find("ZAxis").gameObject;// visCanvas.transform.Find("ZAxis").gameObject;

        XLabel.AddComponent<TextMesh>();
        YLabel.AddComponent<TextMesh>();
        ZLabel.AddComponent<TextMesh>();
        TextMesh xText = XLabel.GetComponent<TextMesh>();
        TextMesh yText = YLabel.GetComponent<TextMesh>();
        TextMesh zText = ZLabel.GetComponent<TextMesh>();

        xText.text = headers[1];
        xText.fontSize = 100;
        xText.color = Color.black;
        yText.text = headers[3];
        yText.fontSize = 100;
        yText.color = Color.black;
        zText.text = headers[2];
        zText.fontSize = 100;
        zText.color = Color.black;
        Points = new ArrayList();

        LoadData();
        ScaleGraph(size_scale);
        SetTickMarks(size_scale);

        /*
         * Assuming this application was loaded from FieldView, there will be local data passed in
         */
        try
        {
            AndroidJavaClass UPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = UPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");
            bool hasLocal = intent.Call<bool>("hasExtra", "localData");
            String localData = "No local data synced! Add datapoints in the 'FieldView' app";
            if (hasLocal)
            {
                AndroidJavaObject extras = intent.Call<AndroidJavaObject>("getExtras");
                localData = extras.Call<string>("getString", "localData");
                loadLocalData(localData);
            }

        } catch (Exception e)
        {
            Debug.Log("No Local Data from Android App");
        }

        /*
         * Get the remote data
         */
        ServerData sd = GameObject.Find("MapScatterRender").GetComponent<ServerData>();
        if (sd != null) remoteMountains = sd.getMySQLData("studies.cu-visualab.org", "fieldview", "Fourteeners", "visualab", databasePassword);


        DisplayData();
    }

    // Update is called once per frame
    void Update () {

        var rayTransform = OVRGazePointer.instance.rayTransform;
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0)) // Tap on the Gear VR touchpad
        {
            Ray ray = new Ray(rayTransform.position, rayTransform.forward); // Get the ray from the OVRGazePointer and detect the object hovered
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.GetComponent<CapsuleCollider>() != null) // If it is a data point
                {
                    GameObject.Find("MountainInfo").GetComponent<Text>().text = mountainInfo[getDataPointIndex(hit.transform.gameObject)];// hit.transform.Find("label").GetComponent<TextMesh>().text;
                }
                else if (hit.transform.gameObject.name == "ZoomInPanel")
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f);
                }
                else if (hit.transform.gameObject.name == "ZoomOutPanel")
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.1f);
                }
            }
        }
        else
        {
            if (Math.Abs(Input.GetAxis("Mouse X")) < 1f && Math.Abs(Input.GetAxis("Mouse X")) > 0.3f)
                transform.position = new Vector3(transform.position.x - (0.2f * Input.GetAxis("Mouse X")), transform.position.y, transform.position.z);
            if (Math.Abs(Input.GetAxis("Mouse Y")) < 1f && Math.Abs(Input.GetAxis("Mouse Y")) > 0.3f)
                transform.position = new Vector3(transform.position.x, transform.position.y + (0.2f * Input.GetAxis("Mouse Y")), transform.position.z);
        }
    }

    public void loadLocalData(String objString)
    {
        localMountains = JsonConvert.DeserializeObject<List<Mountain>>(objString);
    }

    public void loadRemoteData(List<Mountain> mountains)
    {
        remoteMountains = mountains;
    }

    void LoadData()
    {
        if (xDomain.Length == 0) {
            xDomain = new float[2];
            xDomain[0] = 0f;// (float) CSV.MinXValue();
            xDomain[1] = (float)CSV.MaxXValue();
        }
        if (yDomain.Length == 0)
        {
            yDomain = new float[2];
            yDomain[0] = 0f;// (float) CSV.MinYValue();
            yDomain[1] = (float)CSV.MaxYValue();
        }
        if (zDomain.Length == 0)
        {
            zDomain = new float[2];
            zDomain[0] = 0f;// (float) CSV.MinYValue();
            zDomain[1] = (float)CSV.MaxZValue();
        }
        xRange = new float[2];
        yRange = new float[2];
        zRange = new float[2];
    }

    void ScaleGraph (float scale)
    {
        /*
         * Set the axes and labels
         */

        xRange[0] = -scale / 25f;  xRange[1] = scale / 25f;

        yRange[0] = -scale * yx_ratio / 25f;  yRange[1] = scale * yx_ratio / 25f;

        zRange[0] = 0; zRange[1] = 2 * scale * yx_ratio / 25f;

        GameObject.Find("MapImage").transform.localPosition = new Vector3(0, -yx_ratio * scale / 25f, yx_ratio * scale / 25f);
        // For now the Z range = Y range

        XLabel.transform.parent = transform;
        YLabel.transform.parent = transform;
        ZLabel.transform.parent = transform;
        xAxis.transform.parent = transform;
        yAxis.transform.parent = transform;
        zAxis.transform.parent = transform;

        xAxis.transform.localPosition = new Vector3(0, -yx_ratio * scale / 25f, 0);
        xAxis.transform.localEulerAngles = new Vector3(0, 0, 90);
        xAxis.transform.localScale = new Vector3(0.05f, scale / 25f, 0.05f);

        yAxis.transform.localPosition = new Vector3(-scale / 25f , 0, 0);
        yAxis.transform.localScale = new Vector3(0.05f, yx_ratio * scale / 25f, 0.05f);

        zAxis.transform.localPosition = new Vector3(-scale / 25f, -yx_ratio * scale / 25f, yx_ratio * scale / 25f);
        zAxis.transform.localEulerAngles = new Vector3(90, 0, 0);
        zAxis.transform.localScale = new Vector3(0.05f, yx_ratio * scale / 25f, 0.05f);

        XLabel.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
        XLabel.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;
        YLabel.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
        YLabel.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;
        ZLabel.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
        ZLabel.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;

        XLabel.transform.localPosition = new Vector3(0, -scale * yx_ratio / 25f - 0.6f, 0);
        YLabel.transform.localPosition = new Vector3(-scale / 25f - 0.9f, 0, 0);
        ZLabel.transform.localPosition = new Vector3(-scale / 25f - 0.9f, -scale * yx_ratio / 25f - 0.4f, scale * yx_ratio / 25f);

        XLabel.transform.localScale = new Vector3(.02f, .02f, .02f);
        YLabel.transform.localScale = new Vector3(.02f, .02f, .02f);
        ZLabel.transform.localScale = new Vector3(.02f, .02f, .02f);
        YLabel.transform.Rotate(new Vector3(0, 0, 90));

        float pointSize = scale / 750f;
        DataPoint0.transform.localScale = new Vector3(pointSize, scale * yx_ratio / 25f, pointSize);
        DataPoint1.transform.localScale = new Vector3(pointSize, pointSize, pointSize);
        DataPoint2.transform.localScale = new Vector3(pointSize, pointSize, pointSize);
    }

    void SetTickMarks(float scale)
    {
        int numberOfXTicks = (int) Math.Floor(size_scale / 10) + 1;
        GameObject tickmarkObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tickmarkObject.transform.localScale = new Vector3(.05f, 0.1f, .05f);

        GameObject tickLabel = new GameObject("Tick Label");
        tickLabel.transform.localScale = new Vector3(.012f, .012f, .012f);
        tickLabel.AddComponent<TextMesh>();
        tickLabel.GetComponent<TextMesh>().color = Color.black;

        // X Tickmarks
        for (int i = 0; i < numberOfXTicks; i++)
        {
            GameObject newTick = GameObject.Instantiate(tickmarkObject);
            newTick.transform.parent = transform;
            newTick.transform.Rotate(90, 0, 0);
            newTick.transform.localPosition = new Vector3(xRange[0] + (xRange[1] - xRange[0]) * ((float)i / (numberOfXTicks - 1)),
                -scale * yx_ratio / 25f, 0);
            GameObject newTickLabel = GameObject.Instantiate(tickLabel);
            newTickLabel.GetComponent<TextMesh>().text = string.Format("{0:N2}", xDomain[0] + (xDomain[1] - xDomain[0]) * ((float)i / (numberOfXTicks - 1)));
            newTickLabel.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            newTickLabel.GetComponent<TextMesh>().fontSize = 100;
            newTickLabel.transform.parent = transform;
            newTickLabel.transform.localPosition = new Vector3(xRange[0] + (xRange[1] - xRange[0]) * ((float)i / (numberOfXTicks - 1)), -scale * yx_ratio / 25f - 0.2f, 0);
        }

        // Y Tickmarks
        int numberOfYTicks = (int)Math.Floor(yx_ratio * size_scale / 10) + 1;
        for (int i = 0; i < numberOfYTicks; i++)
        {
            GameObject newTick = GameObject.Instantiate(tickmarkObject);
            newTick.transform.Rotate(0, 0, 90);
            newTick.transform.parent = transform;
            newTick.transform.localPosition = new Vector3(-scale / 25f,
                yRange[0] + (yRange[1] - yRange[0]) * ((float)i / (numberOfYTicks - 1)), 0);
            GameObject newTickLabel = GameObject.Instantiate(tickLabel); newTickLabel.GetComponent<TextMesh>().anchor = TextAnchor.MiddleRight;
            newTickLabel.GetComponent<TextMesh>().fontSize = 100;
            newTickLabel.GetComponent<TextMesh>().text = string.Format("{0:N2}", yDomain[0] + (yDomain[1] - yDomain[0]) * ((float)i / (numberOfYTicks - 1)));
            newTickLabel.transform.parent = transform;
            newTickLabel.transform.localPosition = new Vector3(-scale / 25f - 0.2f, yRange[0] + (yRange[1] - yRange[0]) * ((float)i / (numberOfYTicks - 1)), 0);
        }
        
        // Z Tickmarks
        int numberOfZTicks = (int)Math.Floor(yx_ratio * size_scale / 10) + 1;
        for (int i = 0; i < numberOfZTicks; i++)
        {
            GameObject newTick = GameObject.Instantiate(tickmarkObject);
            newTick.transform.parent = transform;
            newTick.transform.Rotate(0, 0, 90);
            newTick.transform.localPosition = new Vector3(-scale / 25f, -scale * yx_ratio / 25f,
                zRange[0] + (zRange[1] - zRange[0]) * ((float)i / (numberOfYTicks - 1)));
            GameObject newTickLabel = GameObject.Instantiate(tickLabel); newTickLabel.GetComponent<TextMesh>().anchor = TextAnchor.MiddleRight;
            newTickLabel.GetComponent<TextMesh>().fontSize = 100;
            newTickLabel.GetComponent<TextMesh>().text = string.Format("{0:N2}", zDomain[0] + (zDomain[1] - zDomain[0]) * ((float)i / (numberOfZTicks - 1)));
            newTickLabel.transform.parent = transform;
            newTickLabel.transform.Rotate(0, 0, 45);
            newTickLabel.transform.localPosition = new Vector3(-scale / 25f - 0.2f, -scale * yx_ratio / 25f - 0.1f, 
                zRange[0] + (zRange[1] - zRange[0]) * ((float)i / (numberOfYTicks - 1)));
        }
        GameObject.Destroy(tickmarkObject);
        GameObject.Destroy(tickLabel);
    }

    void DisplayData()
    {
        int localOffset = 0; // To make sure local points are properly assigned
        // Offline Mode
        if (remoteMountains.Count == 0)
        {
            mountainInfo = new string[CSV.NumberOfData() + localMountains.Count];
            dataPoints = new GameObject[CSV.NumberOfData() + localMountains.Count];
            localOffset = CSV.NumberOfData() - 1;
            for (int i = 1; i <= CSV.NumberOfData(); i++)
            {
                var barSize = yAxis.transform.localScale.y * ((Convert.ToSingle(CSV.rowData[i][3]) - yDomain[0]) / (yDomain[1] - yDomain[0]));
                Vector3 position = new Vector3(xRange[0] + (xRange[1] - xRange[0]) * ((Convert.ToSingle(CSV.rowData[i][1]) - xDomain[0]) / (xDomain[1] - xDomain[0])),
                                               yRange[0] + 0.5f * (yRange[1] - yRange[0]) * (barSize / yAxis.transform.localScale.y),
                                               zRange[0] + (zRange[1] - zRange[0]) * ((Convert.ToSingle(CSV.rowData[i][2]) - zDomain[0]) / (zDomain[1] - zDomain[0])));
                DataPoint2.transform.localScale = new Vector3(0.05f, barSize, 0.05f);
                var newPoint = Instantiate(DataPoint2, position, Quaternion.identity);
                newPoint.transform.parent = transform;
                newPoint.transform.localPosition = position;
                dataPoints[i - 1] = newPoint;
                mountainInfo[i - 1] = CSV.rowData[i][0] + " (Elevation " + CSV.rowData[i][3] + "):\n(" + CSV.rowData[i][2] + "," + CSV.rowData[i][1] + ")";
            }
        }
        // Online Mode
        else
        {
            mountainInfo = new string[remoteMountains.Count + localMountains.Count];
            dataPoints = new GameObject[remoteMountains.Count + localMountains.Count];
            localOffset = remoteMountains.Count;
            for (int i = 0; i < remoteMountains.Count; i++)
            {
                var barSize = yAxis.transform.localScale.y * ((Convert.ToSingle(remoteMountains[i].elevation) - yDomain[0]) / (yDomain[1] - yDomain[0]));
                localOffset = remoteMountains.Count;
                Vector3 position = new Vector3(xRange[0] + (xRange[1] - xRange[0]) * (Convert.ToSingle(remoteMountains[i].longitude) - xDomain[0]) / (xDomain[1] - xDomain[0]),
                                               yRange[0] + 0.5f * (yRange[1] - yRange[0]) * (barSize / yAxis.transform.localScale.y),
                                               zRange[0] + (zRange[1] - zRange[0]) * ((Convert.ToSingle(remoteMountains[i].latitude) - zDomain[0]) / (zDomain[1] - zDomain[0])));
                DataPoint1.transform.localScale = new Vector3(0.05f, barSize, 0.05f);
                var newPoint = Instantiate(DataPoint1, position, Quaternion.identity);
                newPoint.transform.parent = transform;
                newPoint.transform.localPosition = position;
                dataPoints[i] = newPoint;
                mountainInfo[i] = remoteMountains[i].peak + " (Elevation " + remoteMountains[i].elevation + "):\n(" + remoteMountains[i].latitude + "," + remoteMountains[i].longitude + ")";
            }
        }

        // Local Data
        for (int i = 0; i < localMountains.Count; i++)
        {
            var barSize = yAxis.transform.localScale.y * ((Convert.ToSingle(localMountains[i].altitude) - yDomain[0]) / (yDomain[1] - yDomain[0]));
            Vector3 position = new Vector3(xRange[0] + (xRange[1] - xRange[0]) * ((Convert.ToSingle(localMountains[i].longitude) - xDomain[0]) / (xDomain[1] - xDomain[0])),
                                           yRange[0] + 0.5f * (yRange[1] - yRange[0]) * (barSize / yAxis.transform.localScale.y),
                                           zRange[0] + (zRange[1] - zRange[0]) * ((Convert.ToSingle(localMountains[i].latitude) - zDomain[0]) / (zDomain[1] - zDomain[0])));
            DataPoint0.transform.localScale = new Vector3(0.05f, barSize, 0.05f);
            var newPoint = Instantiate(DataPoint0, position, Quaternion.identity);
            newPoint.transform.parent = transform;
            newPoint.transform.localPosition = position;
            dataPoints[i + localOffset] = newPoint;
            mountainInfo[i + localOffset] = localMountains[i].title + " (Elevation " + localMountains[i].altitude + "):\n(" + localMountains[i].latitude + "," + localMountains[i].longitude + ")";
        }
    }
    int getDataPointIndex(GameObject point)
    {
        for (var i = 0; i < dataPoints.Length; i++)
        {
            if (dataPoints[i].Equals(point))
            {
                return i;
            }
        }
        return -1;
    }
}
