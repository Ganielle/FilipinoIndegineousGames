using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgawanBaseLoader : MonoBehaviour
{
    [SerializeField] private AgawanBaseCOre core;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.SceneController.AddActionLoadinList(core.FirstLoading());
        GameManager.Instance.SceneController.ActionPass = true;
    }
}
