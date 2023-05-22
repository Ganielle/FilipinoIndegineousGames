using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class MainMenuCore : MonoBehaviour
{
    public enum MainMenuState
    {
        HOME,
        AGAWANBASE,
        KARERANGTALON,
        PATINTERO,
        TUMBANGPRESO,
        LEADERBOARDS,
        SETTINGS,
        JOURNAL,
        SHOP
    }

    public event EventHandler MainMenuStateChange;
    public event EventHandler OnMainMenuStateChange
    {
        add
        {
            if (MainMenuStateChange == null || !MainMenuStateChange.GetInvocationList().Contains(value))
                MainMenuStateChange = value;
        }
        remove { MainMenuStateChange -= value; }
    }

    public void AddMainMenuStateHistory(MainMenuState state)
    {
        if (appStateHistory.Count != 0) lastAppState = appStateHistory[appStateHistory.Count - 1];

        appStateHistory.Add(state);
        MainMenuStateChange?.Invoke(this, EventArgs.Empty);
    }
    public List<MainMenuState> GetMainMenuStateHistory
    {
        get => appStateHistory;
    }
    public MainMenuState GetCurrentMainMenuState
    {
        get => appStateHistory[appStateHistory.Count - 1];
    }
    public void RemoveMainMenuStateHistory()
    {
        lastAppState = appStateHistory[appStateHistory.Count -1];
        appStateHistory.RemoveAt(appStateHistory.Count - 1);
        MainMenuStateChange?.Invoke(this, EventArgs.Empty);
    }
    public void ResetMainMenuStateHistory()
    {
        lastAppState = MainMenuState.HOME;
        appStateHistory.Clear();
        appStateHistory.Add(MainMenuState.HOME);
        MainMenuStateChange?.Invoke(this, EventArgs.Empty);
    }
    public MainMenuState LastMainMenuState
    {
        get => lastAppState;
    }

    //  ===================================

    [Header("PANELS")]
    [SerializeField] private GameObject home;
    [SerializeField] private GameObject agawanBase;
    [SerializeField] private GameObject patintero;
    [SerializeField] private GameObject karerangTalon;
    [SerializeField] private GameObject tumbangPreso;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject journal;
    [SerializeField] private GameObject shop;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private List<MainMenuState> appStateHistory;
    [ReadOnly][SerializeField] private MainMenuState lastAppState;
    [field: ReadOnly][field: SerializeField] public bool Back { get; set; }
    [field: ReadOnly][field: SerializeField] public bool CanInteract { get; set; }

    public void Animation()
    {
        switch (GetCurrentMainMenuState)
        {
            case MainMenuState.HOME:

                if (Back)
                {
                    if (LastMainMenuState == MainMenuState.AGAWANBASE)
                        agawanBase.SetActive(false);
                    else if (LastMainMenuState == MainMenuState.PATINTERO)
                        patintero.SetActive(false);
                    else if (LastMainMenuState == MainMenuState.KARERANGTALON)
                        karerangTalon.SetActive(false);
                    else if (LastMainMenuState == MainMenuState.TUMBANGPRESO)
                        tumbangPreso.SetActive(false);
                    else if (LastMainMenuState == MainMenuState.SETTINGS)
                        settings.SetActive(false);
                    else if (LastMainMenuState == MainMenuState.JOURNAL)
                        journal.SetActive(false);
                    else if (LastMainMenuState == MainMenuState.SHOP)
                        shop.SetActive(false);
                }

                home.SetActive(true);

                Back = false;
                CanInteract = true;
                break;

            case MainMenuState.AGAWANBASE:

                home.SetActive(false);
                agawanBase.SetActive(true);

                Back = false;
                CanInteract = true;
                break;

            case MainMenuState.PATINTERO:

                home.SetActive(false);
                patintero.SetActive(true);

                Back = false;
                CanInteract = true;
                break;

            case MainMenuState.KARERANGTALON:

                home.SetActive(false);
                karerangTalon.SetActive(true);

                Back = false;
                CanInteract = true;
                break;

            case MainMenuState.TUMBANGPRESO:

                home.SetActive(false);
                tumbangPreso.SetActive(true);

                Back = false;
                CanInteract = true;
                break;

            case MainMenuState.SETTINGS: 
                
                settings.SetActive(true);

                Back = false;
                CanInteract = true;
                break;

            case MainMenuState.JOURNAL:

                journal.SetActive(true);

                Back = false;
                CanInteract = true;
                break;

            case MainMenuState.SHOP:

                shop.SetActive(true);

                Back = false;
                CanInteract = true;
                break;

        }
    }
}
