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

public class GraphRender : MonoBehaviour {

    public CSVReader CSV;
    public GameObject DataPoint0, DataPoint1, DataPoint2;
    public string[] columnHeaders;
    //public Vector3 position;
    public float size_scale;
    private GameObject XLabel, YLabel, xAxis, yAxis;
    private ArrayList Points;
    //private GameObject visCanvas;
    public float[] xDomain;
    public float[] yDomain;
    private float[] xRange;
    private float[] yRange;
    public float yx_ratio;
    private string[] mountainInfo;
    private GameObject[] dataPoints;

    // Use this for initialization
    void Start () {
        CSV.ParseData();
        CSV.SetDataSets();

        string[] headers = CSV.GetColumnHeads();
        //visCanvas = GameObject.FindGameObjectsWithTag("VisCanvas")[0];

        XLabel = new GameObject("XLabel");// visCanvas.transform.Find("XLabel").gameObject;
        YLabel = new GameObject("YLabel"); // visCanvas.transform.Find("YLabel").gameObject;
        xAxis = transform.Find("XAxis").gameObject;// visCanvas.transform.Find("XAxis").gameObject;
        yAxis = transform.Find("YAxis").gameObject;// visCanvas.transform.Find("YAxis").gameObject;

        XLabel.AddComponent<TextMesh>();
        YLabel.AddComponent<TextMesh>();
        TextMesh xText = XLabel.GetComponent<TextMesh>();
        TextMesh yText = YLabel.GetComponent<TextMesh>();
        
        xText.text = headers[1];
        xText.fontSize = 100;
        xText.color = Color.black;
        yText.text = headers[2];
        yText.fontSize = 100;
        yText.color = Color.black;
        Points = new ArrayList();

        LoadData();
        ScaleGraph(size_scale);
        SetTickMarks(size_scale);
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
                if (hit.transform.GetComponent<SphereCollider>() != null) // If it is a data point
                {
                    GameObject.Find("MountainInfo").GetComponent<Text>().text = mountainInfo[getDataPointIndex(hit.transform.gameObject)];// hit.transform.Find("label").GetComponent<TextMesh>().text;
                }
            }
        }
        //Debug.Log(GameObject.Find("label").GetComponent<TextMesh>());
        //GameObject.Find("MountainInfo").GetComponent<Text>().text = GameObject.Find("label").GetComponent<TextMesh>().text;
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
        xRange = new float[2];
        yRange = new float[2];

    }

    void ScaleGraph (float scale)
    {
        /*
         * Set the axes and labels
         */

        xRange[0] = -scale / 25f;  xRange[1] = scale / 25f;

        yRange[0] = -scale * yx_ratio / 25f;  yRange[1] = scale * yx_ratio / 25f;
        
        
        XLabel.transform.parent = transform;
        YLabel.transform.parent = transform;
        xAxis.transform.parent = transform;
        yAxis.transform.parent = transform;

        xAxis.transform.localPosition = new Vector3(0, -yx_ratio * scale / 25f, 0);
        xAxis.transform.localEulerAngles = new Vector3(0, 0, 90);
        xAxis.transform.localScale = new Vector3(0.05f, scale / 25f, 0.05f);
        // 150: Pos (-1.6, 4.6, 0.0); Scale (8250.0, 50.0, 50.0); 250: Pos (-1.6, -1.8, 0.0); Scale (13750.0, 50.0, 50.0)

        yAxis.transform.localPosition = new Vector3(-scale / 25f , 0, 0);
        yAxis.transform.localScale = new Vector3(0.05f, yx_ratio * scale / 25f, 0.05f);
        //Debug.Log("YAxis: Pos " + yAxis.transform.position.x + ", " + yAxis.transform.position.y + ", " + yAxis.transform.position.z + "; Scale " + +yAxis.transform.localScale.x + ", " + yAxis.transform.localScale.y + ", " + yAxis.transform.localScale.z);
        // 150:  Pos (-8.9, 11.2, 0.0); Scale (50.0, 3750.0, 50.0); 250: Pos (-13.7, 9.25, 0.0); Scale (50.0, 6250.0, 50.0)
        XLabel.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
        XLabel.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;
        YLabel.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
        YLabel.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;

        XLabel.transform.localPosition = new Vector3(0, -scale * yx_ratio / 25f - 0.6f, 0);
        YLabel.transform.localPosition = new Vector3(-scale / 25f - 0.9f, 0, 0);
        XLabel.transform.localScale = new Vector3(.02f, .02f, .02f);
        YLabel.transform.localScale = new Vector3(.02f, .02f, .02f);
        YLabel.transform.Rotate(new Vector3(0, 0, 90));

        float pointSize = scale / 750f;
        DataPoint0.transform.localScale = new Vector3(pointSize, pointSize, pointSize);
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
            newTick.transform.localPosition = new Vector3(xRange[0] + (xRange[1] - xRange[0]) * ((float)i / (numberOfXTicks - 1)),
                -scale * yx_ratio / 25f, 0);
            GameObject newTickLabel = GameObject.Instantiate(tickLabel);
            newTickLabel.GetComponent<TextMesh>().text = string.Format("{0:N2}", xDomain[0] + (xDomain[1] - xDomain[0]) * ((float)i / (numberOfXTicks - 1)));
            newTickLabel.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            newTickLabel.GetComponent<TextMesh>().fontSize = 100;
            newTickLabel.transform.parent = transform;
            newTickLabel.transform.localPosition = new Vector3(xRange[0] + (xRange[1] - xRange[0]) * ((float)i / (numberOfXTicks - 1)), -scale * yx_ratio / 25f - 0.2f, 0);
        }

        int numberOfYTicks = (int)Math.Floor(yx_ratio * size_scale / 10) + 1;
        // Y Tickmarks
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
        GameObject.Destroy(tickmarkObject);
        GameObject.Destroy(tickLabel);
    }
    
    void DisplayData()
    {
        mountainInfo = new string[CSV.NumberOfData()];
        dataPoints = new GameObject[CSV.NumberOfData()];
        for (int i = 1; i <= CSV.NumberOfData(); i++)
        {

            Vector3 position = new Vector3(xRange[0] + (xRange[1] - xRange[0]) * ((Convert.ToSingle(CSV.rowData[i][1]) - xDomain[0]) / (xDomain[1] - xDomain[0])),
                                           yRange[0] + (yRange[1] - yRange[0]) * ((Convert.ToSingle(CSV.rowData[i][2]) - yDomain[0]) / (yDomain[1] - yDomain[0])), 0.0f);
            if (CSV.dataSets.Count > 2) //if (CSV.rowData[i][0] == CSV.dataSets[2])
            {
                var newPoint = Instantiate(DataPoint0, position, Quaternion.identity);
                newPoint.transform.parent = transform;
                newPoint.transform.localPosition = position;
                dataPoints[i - 1] = newPoint;
                mountainInfo[i - 1] = CSV.rowData[i][0] + " (Elevation " + CSV.rowData[i][3] + "):\n(" + CSV.rowData[i][2] + "," + CSV.rowData[i][1] + ")";
            }
            else if (CSV.rowData[i][0] == CSV.dataSets[0])
            {

                var newPoint = Instantiate(DataPoint0, position, Quaternion.identity);
                newPoint.transform.parent = transform;
                newPoint.transform.localPosition = position;
            }
            else if (CSV.rowData[i][0] == CSV.dataSets[1])
            {
                var newPoint = Instantiate(DataPoint1, position, Quaternion.identity);
                newPoint.transform.parent = transform;
                newPoint.transform.localPosition = position;
            }
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
