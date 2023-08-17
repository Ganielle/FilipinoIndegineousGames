using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgawanBaseGameController : MonoBehaviour
{
    //  ====================================

    private GameController gameController;

    //  ====================================

    private void Awake()
    {
        gameController = new GameController();
    }

    private void OnEnable()
    {
        gameController.Enable();
    }

    private void OnDisable()
    {
        gameController.Disable();
    }

    public Vector2 GetPlayerMovement()
    {
        return gameController.AgawanBase.Movement.ReadValue<Vector2>();
    }

    public Vector2 GetCameraDelta()
    {
        return gameController.AgawanBase.Movement.ReadValue<Vector2>();
    }
}
