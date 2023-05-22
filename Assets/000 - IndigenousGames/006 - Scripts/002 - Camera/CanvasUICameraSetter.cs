using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasUICameraSetter : MonoBehaviour
{
    public Canvas canvas;

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;

        canvas.worldCamera = GameManager.Instance.MyUICamera;
    }
}
