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
using System.Collections.Generic;
using System.IO;

public class GraphRender : MonoBehaviour {

    public CSVReader CSV;
    public GameObject DataPoint0, DataPoint1, DataPoint2;
    public string[] columnHeaders;
    public Vector3 position;
    public float size_scale;
    private GameObject XLabel, YLabel;
    private ArrayList Points;
    private GameObject visCanvas;
    private float[] xDomain;
    private float[] yDomain;
    private float[] xRange;
    private float[] yRange;

    // Use this for initialization
    void Start () {
        CSV.ParseData();
        CSV.SetDataSets();

        string[] headers = CSV.GetColumnHeads();

        XLabel = GameObject.Find("XLabel");
        YLabel = GameObject.Find("YLabel");
        TextMesh xText = XLabel.GetComponent<TextMesh>();
        TextMesh yText = YLabel.GetComponent<TextMesh>();

        xText.text = headers[1];
        xText.fontSize = 20;
        xText.color = Color.black;
        yText.text = headers[2];
        yText.fontSize = 20;
        yText.color = Color.black;
        Points = new ArrayList();

        visCanvas = GameObject.FindGameObjectsWithTag("VisCanvas")[0];

        GameObject newCanvas = GameObject.Instantiate(visCanvas);
        newCanvas.transform.parent = visCanvas.transform.parent;
        newCanvas.transform.localScale = Vector3.one;
        visCanvas.transform.position = position;
        
        LoadData();
        ScaleGraph(size_scale, Vector2.zero);
        SetTickMarks();
        DisplayData();
        visCanvas.gameObject.tag = "Untagged";
    }

    // Update is called once per frame
    void Update () {

        var rayTransform = OVRGazePointer.instance.rayTransform;
        RaycastHit hit;
        YLabel = GameObject.Find("YLabel");
        TextMesh yText = YLabel.GetComponent<TextMesh>();

        if (Input.GetMouseButtonDown(0)) // Tap on the Gear VR touchpad
        {
            Ray ray = new Ray(rayTransform.position, rayTransform.forward); // Get the ray from the OVRGazePointer and detect the object hovered
            if (Physics.Raycast(ray, out hit))
            {
                Destroy(hit.transform.gameObject);
            }
        }
	}

    void LoadData()
    {

        xDomain = new float[2];  xRange = new float[2];
        yDomain = new float[2];  yRange = new float[2];

        xDomain[0] = (float) CSV.MinXValue();
        xDomain[1] = (float)CSV.MaxXValue();

        yDomain[0] = (float) CSV.MinYValue();
        yDomain[1] = (float) CSV.MaxYValue();
    }

    /*
     * Function to scale the graph--tested manually with Unity Editor with 150 and 250 as scale values
     * Experimentally found linear functions commented
     */
    void ScaleGraph (float scale, Vector2 center)
    {
        Debug.Log(scale);
        /*
         * Set the axes and labels
         */

        xRange[0] = center.x - .0402f * scale -1.45f;  xRange[1] = center.y + 0.04325f * scale - 1.813f;
        // 150: [-7.48, 4.675] 250: [-11.5, 9]

        yRange[0] = center.x - .05785f * scale + 14.46f;  yRange[1] = center.y + 0.015f * scale + 14.75f;
        // 150: [5.784616, 17] 250: [0, 18.5]
        
        GameObject xAxis = GameObject.Find("XAxis");
        GameObject yAxis = GameObject.Find("YAxis");

        XLabel.transform.parent = visCanvas.transform;
        YLabel.transform.parent = visCanvas.transform;
        xAxis.transform.parent = visCanvas.transform;
        yAxis.transform.parent = visCanvas.transform;

        xAxis.transform.position = new Vector3(center.x - 1.6f, center.y - 0.064f * scale + 14.2f, 0);
        xAxis.transform.localScale = new Vector3(scale * 55, 50, 50);
        // 150: Pos (-1.6, 4.6, 0.0); Scale (8250.0, 50.0, 50.0); 250: Pos (-1.6, -1.8, 0.0); Scale (13750.0, 50.0, 50.0)

        yAxis.transform.position = new Vector3(center.x - 1.7f - 0.048f * scale , center.y - .0195f * scale + 14.13f, 0);
        yAxis.transform.localScale = new Vector3(50, scale * 25, 50);
        Debug.Log("YAxis: Pos " + yAxis.transform.position.x + ", " + yAxis.transform.position.y + ", " + yAxis.transform.position.z + "; Scale " + +yAxis.transform.localScale.x + ", " + yAxis.transform.localScale.y + ", " + yAxis.transform.localScale.z);
        // 150:  Pos (-8.9, 11.2, 0.0); Scale (50.0, 3750.0, 50.0); 250: Pos (-13.7, 9.25, 0.0); Scale (50.0, 6250.0, 50.0)
        

        XLabel.transform.position = new Vector3(xAxis.transform.position.x, -0.064f * scale + 13, 0);
        YLabel.transform.position = new Vector3(-4.75f - 0.048f * scale, yAxis.transform.position.y, 0);
        XLabel.transform.localScale = new Vector3(450, 450, 450);
        YLabel.transform.localScale = new Vector3(450, 450, 450);

        XLabel.GetComponent<TextMesh>().fontSize = (int) Math.Floor(0.04f * scale + 6);
        YLabel.GetComponent<TextMesh>().fontSize = (int) Math.Floor(0.04f * scale + 6);
        YLabel.transform.Rotate(new Vector3(0, 0, 90));

        float pointSize = scale / 450;
        DataPoint0.transform.localScale = new Vector3(pointSize, pointSize, pointSize);
        DataPoint1.transform.localScale = new Vector3(pointSize, pointSize, pointSize);
        DataPoint2.transform.localScale = new Vector3(pointSize, pointSize, pointSize);
    }

    void SetTickMarks()
    {
        int numberOfTicks = (int) Math.Floor(size_scale / 50) + 1;
        if (numberOfTicks < 2) return;
        GameObject tickmarkObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tickmarkObject.transform.localScale = new Vector3(.07f, .3f, .07f);

        GameObject tickLabel = new GameObject("Tick Label");
        tickLabel.transform.localScale = new Vector3(.3f, .3f, .3f);
        tickLabel.AddComponent<TextMesh>();
        tickLabel.GetComponent<TextMesh>().color = Color.black;

        // X Tickmarks
        for (int i = 0; i < numberOfTicks; i++)
        {
            GameObject newTick = GameObject.Instantiate(tickmarkObject);
            newTick.transform.parent = visCanvas.transform;
            newTick.transform.position = new Vector3(xRange[0] + (xRange[1] - xRange[0]) * ((float)i / (numberOfTicks - 1)),
                -0.064f * size_scale + 14.2f, 0);
            GameObject newTickLabel = GameObject.Instantiate(tickLabel);
            newTickLabel.GetComponent<TextMesh>().text = string.Format("{0:N2}", xDomain[0] + (xDomain[1] - xDomain[0]) * ((float)i / (numberOfTicks - 1)));
            newTickLabel.transform.position = new Vector3(newTick.transform.position.x, newTick.transform.position.y - 0.5f, newTick.transform.position.z);
            newTickLabel.transform.parent = visCanvas.transform;
        }

        // Y Tickmarks
        for (int i = 0; i < numberOfTicks; i++)
        {
            GameObject newTick = GameObject.Instantiate(tickmarkObject);
            newTick.transform.Rotate(0, 0, 90);
            newTick.transform.parent = visCanvas.transform;
            newTick.transform.position = new Vector3(-1.7f - 0.048f * size_scale,
                yRange[0] + (yRange[1] - yRange[0]) * ((float)i / (numberOfTicks - 1)), 0);
            GameObject newTickLabel = GameObject.Instantiate(tickLabel);
            newTickLabel.GetComponent<TextMesh>().text = string.Format("{0:N2}", yDomain[0] + (yDomain[1] - yDomain[0]) * ((float)i / (numberOfTicks - 1)));
            newTickLabel.transform.position = new Vector3(newTick.transform.position.x - 1.5f, newTick.transform.position.y + 0.2f, newTick.transform.position.z);
            newTickLabel.transform.parent = visCanvas.transform;
        }
        GameObject.Destroy(tickmarkObject);
        GameObject.Destroy(tickLabel);
    }
    void DisplayData()
    {
        for (int i = 1; i < CSV.NumberOfData(); i++)
        {
            Vector3 position = new Vector3(xRange[0] + (xRange[1] - xRange[0]) * ((Convert.ToSingle(CSV.rowData[i][1]) - xDomain[0]) / (xDomain[1] - xDomain[0])),
                                           yRange[0] + (yRange[1] - yRange[0]) * ((Convert.ToSingle(CSV.rowData[i][2]) - yDomain[0]) / (yDomain[1] - yDomain[0])), 0);
            if (CSV.rowData[i][0] == CSV.dataSets[0])
            {
                var newPoint = Instantiate(DataPoint0, position, Quaternion.identity);
                newPoint.transform.parent = visCanvas.transform;
            }
            else if (CSV.rowData[i][0] == CSV.dataSets[1])
            {
                var newPoint = Instantiate(DataPoint1, position, Quaternion.identity);
                newPoint.transform.parent = visCanvas.transform;
            }
            else if (CSV.rowData[i][0] == CSV.dataSets[2])
            {
                var newPoint = Instantiate(DataPoint2, position, Quaternion.identity);
                newPoint.transform.parent = visCanvas.transform;
            }
        }
    }
}
