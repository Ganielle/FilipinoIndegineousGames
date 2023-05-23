using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgawanBaseController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.SceneController.ActionPass = true;
    }
}
