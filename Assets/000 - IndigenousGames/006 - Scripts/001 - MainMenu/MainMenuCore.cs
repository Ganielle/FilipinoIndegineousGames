using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        SHOP,
        CREDITS
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

    public enum ShopState
    {
        character,
        trivia
    };

    public event EventHandler ShopStateChange;
    public event EventHandler OnShopStateChange
    {
        add
        {
            if (ShopStateChange == null || !ShopStateChange.GetInvocationList().Contains(value))
                ShopStateChange += value;
        }
        remove { ShopStateChange -= value; }
    }
    public ShopState CurrentShopState
    {
        get => currentShopState;
        set
        {
            currentShopState = value;
            ShopStateChange?.Invoke(this, EventArgs.Empty);
        }
    }

    //  ===================================

    [SerializeField] private PlayeData playerData;
    [SerializeField] private AudioClip bgMusic;

    [Header("PANELS")]
    [SerializeField] private GameObject home;
    [SerializeField] private GameObject agawanBase;
    [SerializeField] private GameObject patintero;
    [SerializeField] private GameObject karerangTalon;
    [SerializeField] private GameObject tumbangPreso;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject journal;
    [SerializeField] private GameObject shop;

    [Header("SHOP")]
    [SerializeField] private GameObject characterObj;
    [SerializeField] private GameObject triviaObj;
    [SerializeField] private Transform characterContentTF;
    [SerializeField] private Transform triviaContentTF;
    [SerializeField] private GameObject charItemShop;
    [SerializeField] private GameObject triviaItemShop;
    [SerializeField] private TextMeshProUGUI creditsTMP;
    [SerializeField] private List<ShopCharData> charDataList;
    [SerializeField] private List<TriviaData> triviaDataList;

    [Header("JOURNAL")]
    [SerializeField] private GameObject journalItemPrefab;
    [SerializeField] private GameObject noItemObj;
    [SerializeField] private GameObject journalViewport;
    [SerializeField] private GameObject journalLoading;
    [SerializeField] private Transform journalTF;
    [SerializeField] private TextMeshProUGUI journalTriviaTMP;
    [SerializeField] private GameObject journalTriviaObj;

    [Header("CREDITS")]
    [SerializeField] private GameObject creditsObj;
    [SerializeField] private Slider volumeSlider;


    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private List<MainMenuState> appStateHistory;
    [ReadOnly][SerializeField] private MainMenuState lastAppState;
    [ReadOnly][SerializeField] private ShopState currentShopState;
    [field: ReadOnly][field: SerializeField] public bool Back { get; set; }
    [field: ReadOnly][field: SerializeField] public bool CanInteract { get; set; }

    //  ====================

    Coroutine populateJournal;

    //  ====================

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

                if (Back)
                {
                    if (LastMainMenuState == MainMenuState.CREDITS)
                        creditsObj.SetActive(false);
                }

                volumeSlider.value = GameManager.Instance.AudioSystem.CurrentVolume;
                settings.SetActive(true);

                Back = false;
                CanInteract = true;
                break;

            case MainMenuState.JOURNAL:

                if (populateJournal != null) StopCoroutine(populateJournal);

                populateJournal = StartCoroutine(PopulateJournal());

                journal.SetActive(true);

                Back = false;
                CanInteract = true;
                break;

            case MainMenuState.SHOP:

                CurrentShopState = ShopState.character;

                creditsTMP.text = "Credits: " + playerData.Credits.ToString("n0");

                shop.SetActive(true);

                Back = false;
                CanInteract = true;
                break;

            case MainMenuState.CREDITS:

                settings.SetActive(false);
                creditsObj.SetActive(true);

                Back = false;
                CanInteract = true;

                break;
        }
    }

    public void ChangeShopState(int index)
    {
        switch (index)
        {
            case 0:
                CurrentShopState = ShopState.character;
                break;
            case 1:
                CurrentShopState = ShopState.trivia;
                break;
        }
    }

    public IEnumerator PopulateCharacterShop()
    {
        if (playerData.UnlockedCharacters.Count == 0)
            playerData.SetPlayerData();

        while (playerData.UnlockedCharacters.Count == 0) yield return null;

        for (int a = 0; a < charDataList.Count; a++)
        {
            GameObject obj = Instantiate(charItemShop, characterContentTF);

            obj.GetComponent<ShopCharController>().shopCharData = charDataList[a];
            obj.GetComponent<ShopCharController>().playerData = playerData;
            obj.GetComponent<ShopCharController>().SetData(() => creditsTMP.text = playerData.Credits.ToString("n0"));
            yield return null;
        }
    }

    public IEnumerator PopulateTriviaShop()
    {
        for (int a = 0; a < triviaDataList.Count; a++)
        {
            GameObject obj = Instantiate(triviaItemShop, triviaContentTF);

            obj.GetComponent<TriviaController>().triviaData = triviaDataList[a];
            obj.GetComponent<TriviaController>().CheckIfUnlocked();

            yield return null;
        }
    }

    private IEnumerator PopulateJournal()
    {
        journalLoading.SetActive(true);
        noItemObj.SetActive(false);
        journalViewport.SetActive(false);

        while (journalTF.childCount > 0)
        {
            for (int a = 0; a < journalTF.childCount; a++)
            {
                Destroy(journalTF.GetChild(a).gameObject);
                yield return null;
            }

            yield return null;
        }

        if (playerData.UnlockedTrivias.Count <= 0)
        {
            journalLoading.SetActive(false);
            noItemObj.SetActive(true);
            populateJournal = null;

            yield break;
        }

        for (int a = 0; a < playerData.UnlockedTrivias.Count; a++)
        {
            GameObject obj = Instantiate(journalItemPrefab, journalTF);

            obj.GetComponent<JournalItemController>().triviaData = triviaDataList.Find(e => e.itemID == playerData.UnlockedTrivias[a]);
            obj.GetComponent<JournalItemController>().SetData(this);
        }


        journalLoading.SetActive(false);
        journalViewport.gameObject.SetActive(true);

        populateJournal = null;
    }

    public void ShowJournalContent(string content)
    {
        journalTriviaTMP.text = content;
        journalTriviaObj.SetActive(true);
    }

    public void ShowCharShopItems()
    {
        characterObj.SetActive(true);
        triviaObj.SetActive(false);
    }

    public void ShowCharTriviaItems()
    {
        characterObj.SetActive(false);
        triviaObj.SetActive(true);
    }

    public IEnumerator ChangeBackgroundMusic()
    {
        GameManager.Instance.AudioSystem.SetBGMusic(bgMusic);

        yield return null;
    }

    public void ChangeVolumeSettings()
    {
        GameManager.Instance.AudioSystem.CurrentVolume = volumeSlider.value;
    }

    public void SelfBug()
    {
        Application.Quit();
    }
}
