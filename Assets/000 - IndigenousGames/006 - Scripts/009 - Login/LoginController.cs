using PlayFab;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [SerializeField] private MainMenuController menuController;

    [Space]
    [SerializeField] private TMP_InputField usernameTMP;
    [SerializeField] private TMP_InputField passwordTMP;
    [SerializeField] private Button loginBtn;
    [SerializeField] private Button registerBtn;


    private void Login()
    {
        PlayFabClientAPI.LoginWithPlayFab(new PlayFab.ClientModels.LoginWithPlayFabRequest()
        {
            Username = usernameTMP.text,
            Password = passwordTMP.text
        }, resultCallback =>
        {
            menuController.ChangeMenuState(0);
            GameManager.Instance.LoadingNoBG.SetActive(false);
            ButtonEnabler(true);
        }, errorCallback =>
        {
            GameManager.Instance.LoadingNoBG.SetActive(false);
            switch (errorCallback.Error)
            {
                case PlayFabErrorCode.AccountNotFound:
                    GameManager.Instance.ErrorControl.ShowError("Username and password not found! Please input your account details" +
                        " and try again.", () => ButtonEnabler(true));
                    return;
                default:
                    GameManager.Instance.ErrorControl.ShowError("There's a problem with your internet connection, " +
                        "Please try again later!", () => ButtonEnabler(true));
                    return;
            }
        });
    }

    private void ButtonEnabler(bool value)
    {
        loginBtn.interactable = value;
        registerBtn.interactable = value;
        usernameTMP.interactable = value;
        passwordTMP.interactable = value;
    }

    public void LoginButton()
    {
        ButtonEnabler(false);
        GameManager.Instance.LoadingNoBG.SetActive(true);
        if (usernameTMP.text == "")
        {
            GameManager.Instance.ErrorControl.ShowError("Please input username and try again!", () => ButtonEnabler(true));

            GameManager.Instance.LoadingNoBG.SetActive(false);
            return;
        }

        else if (passwordTMP.text == "")
        {
            GameManager.Instance.ErrorControl.ShowError("Please input password and try again!", () => ButtonEnabler(true));
            GameManager.Instance.LoadingNoBG.SetActive(false);
            return;
        }

        Login();
    }
}
