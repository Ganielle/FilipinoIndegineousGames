using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //  ===========================================

    public static GameManager Instance;

    //  ==========================================

    [field: SerializeField] public List<GameObject> GameMangerObj { get; set; }

    [Space]
    [SerializeField] private string SceneToLoad;

    [field: Header("CAMERA")]
    [field: SerializeField] public Camera MyUICamera { get; set; }
    [field: SerializeField] public Camera MainCamera { get; set; }

    [field: Header("MISCELLANEOUS SCRIPTS")]
    [field: SerializeField] public SceneController SceneController { get; set; }
    [field: SerializeField] public ErrorController ErrorControl { get; set; }

    //  ==========================================

    private void Awake()
    {
        for (int a = 0; a < GameMangerObj.Count; a++)
            DontDestroyOnLoad(GameMangerObj[a]);

        DontDestroyOnLoad(gameObject);

        Instance = this;
    }

    private void Start()
    {
        SceneController.CurrentScene = SceneToLoad;
    }
}
