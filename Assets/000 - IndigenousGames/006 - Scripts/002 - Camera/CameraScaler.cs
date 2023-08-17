using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraScaler : MonoBehaviour 
{
    public Camera mainCamera;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private float aspectRatio;

    private void Start()
    {
        aspectRatio = mainCamera.aspect;
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float scale = screenRatio / aspectRatio;

        if (scale < 1.0f)
        {
            Rect rect = mainCamera.rect;
            rect.width = scale;
            rect.height = 1.0f;
            rect.x = (1.0f - scale) / 2.0f;
            rect.y = 0;
            mainCamera.rect = rect;
        }
        else
        {
            float scaleHeight = 1.0f / scale;
            Rect rect = mainCamera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            mainCamera.rect = rect;
        }
    }
}
