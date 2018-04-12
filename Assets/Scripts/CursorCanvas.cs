using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorCanvas : MonoBehaviour {
    private float originalPlaneDistance;
	// Use this for initialization
	void Start () {
        originalPlaneDistance = transform.GetComponent<Canvas>().planeDistance;
    }
	
	// Update is called once per frame
	void Update () {
        var rayTransform = OVRGazePointer.instance.rayTransform;
        Ray ray = new Ray(rayTransform.position, rayTransform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
            transform.GetComponent<Canvas>().planeDistance = hit.transform.position.z - hit.transform.localScale.z;
        else transform.GetComponent<Canvas>().planeDistance = originalPlaneDistance;
    }
}
