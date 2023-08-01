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
        menuCore.OnShopStateChange += ShopChange;
    }

    private void Start()
    {
        GameManager.Instance.SceneController.AddActionLoadinList(menuCore.PopulateCharacterShop());
        GameManager.Instance.SceneController.AddActionLoadinList(menuCore.PopulateTriviaShop());
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void OnDisable()
    {
        menuCore.OnMainMenuStateChange -= StateChange;
        menuCore.OnShopStateChange -= ShopChange;
    }

    private void StateChange(object sender, EventArgs e)
    {
        menuCore.Animation();
    }

    private void ShopChange(object sender, EventArgs e)
    {
        if (menuCore.CurrentShopState == MainMenuCore.ShopState.character)
            menuCore.ShowCharShopItems();
        else
            menuCore.ShowCharTriviaItems();
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

    public void ChangeGameScene(string stageName)
    {
        if (!menuCore.CanInteract) return;

        menuCore.CanInteract = false;

        GameManager.Instance.SceneController.CurrentScene = stageName;
    }

    public void ComingSoon()
    {
        GameManager.Instance.ErrorControl.ShowError("FEATURE COMING SOON!", null);
    }

    #endregion
}
