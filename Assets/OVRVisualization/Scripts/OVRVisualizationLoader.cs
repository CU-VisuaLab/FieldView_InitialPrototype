/************************************************************************************

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.2 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.2

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class OVRVisualizationLoader : MonoBehaviour
{
    [Tooltip("Allow the player to close the UI")]
    public bool allowClose = true;
    [Tooltip("Panel which will be added to the main UI when it is loaded")]
    public Transform donorPanel;
    [Tooltip("You may want to delay summoning the menu a small amount of time to allow for physics to settle the players position. -1 means don't summon")]
    public float summonMenuDelay = 0.65f;

    [Header("Events")]
    [SerializeField]
    public OVRVisualization.VisualizationShowEvent onVisualizationShow = new OVRVisualization.VisualizationShowEvent();

    [SerializeField]
    public OVRVisualization.VisualizationHideEvent onVisualizationHide = new OVRVisualization.VisualizationHideEvent();

    // Use this for initialization
    void Awake()
    {
        var visualizationLoaders = GameObject.FindObjectsOfType(typeof(OVRVisualizationLoader));

        if (visualizationLoaders.Length > 1)
        {
            Debug.LogError("More than 1 VisualizationLoader in scene");
        }
        if (!OVRVisualization.instance)
        {
            OVRVisualization visualizationPrefab = (OVRVisualization)Resources.Load("Prefabs/OVRVisualization", typeof(OVRVisualization));
            Instantiate(visualizationPrefab).name = "OVRVisualization";
        }

        // Register event handlers
        OVRVisualization.instance.onVisualizationShow = onVisualizationShow;
        OVRVisualization.instance.onVisualizationHide = onVisualizationHide;
        // Add our context to the main UI

        OVRVisualization.instance.allowClose = allowClose;
    }

    void Start()
    {
        if (summonMenuDelay > 0)
        {
            StartCoroutine(DelayedMenuSummon());
        }
        else if (summonMenuDelay == 0)
            OVRVisualization.instance.Show();
    }

    IEnumerator DelayedMenuSummon()
    {
        yield return new WaitForSeconds(summonMenuDelay);
        OVRVisualization.instance.Show();
        // Now we don't need this anymore, disable it to reduce the draw call overhead
        gameObject.SetActive(false);
    }
}
