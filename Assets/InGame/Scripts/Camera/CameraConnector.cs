using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraConnector : MonoBehaviour
{
    public Camera cam;

    void Update()
    {
        if (cam == null) 
        {
            MainCamera mainCam = FindObjectOfType<MainCamera>();
            if (mainCam != null)
            {
                cam = mainCam.GetComponent<Camera>();
            }

            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.worldCamera = cam;
            }
        }
    }
}
