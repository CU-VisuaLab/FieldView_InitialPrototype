/**********************************************************************
 * Alex Thompson
 * 8/11/2016
 * Class file for initializing the axes in the current visualization.
 * This class is re-used in all visualization types.
 *********************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class RenderedVolume : MonoBehaviour {

    private GameObject XAxis, YAxis;

	void Start () {
        
        XAxis = GameObject.Find("XAxis");
        YAxis = GameObject.Find("YAxis");

        LineRenderer xRend = XAxis.GetComponent<LineRenderer>();
        LineRenderer yRend = YAxis.GetComponent<LineRenderer>();

        //xRend.SetPosition(0, new Vector3(0, 10 , 0));
        //xRend.SetPosition(1, new Vector3(50, 10, 0));
        //xRend.SetWidth(0.1f, 0.1f);
        //xRend.SetColors(Color.black, Color.black);

        //yRend.SetPosition(0, new Vector3(0, 10, 0));
        //yRend.SetPosition(1, new Vector3(0, 60, 0));
        //yRend.SetWidth(0.1f, 0.1f);
        //yRend.SetColors(Color.black, Color.black);
    }

}
