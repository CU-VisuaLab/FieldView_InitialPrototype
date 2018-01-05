/**********************************************************************
 * Alex Thompson
 * 8/11/2016
 * Class file for setting up use of the webcamera on the android device
 * that will serve as the background of the visualization. The visualization
 * will be rendered on top of the feed from the webcam to incorporate
 * VR/AR elements.
 * This class is re-used in all visualization types.
 *********************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WebCam : MonoBehaviour {

    WebCamTexture phoneCam;

    void Start () {
        phoneCam = new WebCamTexture();
        RawImage vidScreen = gameObject.GetComponent<RawImage>();
        vidScreen.texture = phoneCam;
        vidScreen.material.mainTexture = phoneCam;
        phoneCam.Play();
	}
	
}
