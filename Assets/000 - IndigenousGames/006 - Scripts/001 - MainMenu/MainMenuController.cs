using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private MainMenuCore menuCore;

    private void Awake()
    {
        menuCore.CanInteract = true;
        menuCore.ResetMainMenuStateHistory();
        menuCore.OnMainMenuStateChange += StateChange;
    }

    private void Start()
    {
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void OnDisable()
    {
        menuCore.OnMainMenuStateChange -= StateChange;
    }

    private void StateChange(object sender, EventArgs e)
    {
        menuCore.Animation();
    }

    #region BUTTONS

    public void ChangeMenuState(int index)
    {
        if (!menuCore.CanInteract) return;

        menuCore.CanInteract = false;

        menuCore.AddMainMenuStateHistory((MainMenuCore.MainMenuState) index);
    }

    public void BackButton()
    {
        if (!menuCore.CanInteract) return;

        menuCore.CanInteract = false;

        menuCore.Back = true;

        menuCore.RemoveMainMenuStateHistory();

    }

    #endregion
}
