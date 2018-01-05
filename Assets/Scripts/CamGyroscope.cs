/**********************************************************************
 * Alex Thompson
 * 8/11/2016
 * Class file for setting up the use of the android device's accelerometer
 * for the use of controlling the camera/perspective of the user.
 * This class is re-used in all visualization types.
 *********************************************************************/

using UnityEngine;
using System.Collections;

public class CamGyroscope : MonoBehaviour {

    void Start()
    {
        Input.compensateSensors = true;
        Input.gyro.enabled = true;
    }

	void Update () {
	    transform.Rotate(-Input.gyro.rotationRateUnbiased.x, -Input.gyro.rotationRateUnbiased.y, Input.gyro.rotationRateUnbiased.z);
	}
}
