using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AgawanBaseCOre : MonoBehaviour
{
    public enum MatchState
    {
        NONE,
        GETREADY,
        PLAY,
        FINISH
    }
    private event EventHandler MatchStateChange;
    public event EventHandler OnMatchStateChange
    {
        add
        {
            if (MatchStateChange == null || !MatchStateChange.GetInvocationList().Contains(value))
                MatchStateChange += value;
        }
        remove { MatchStateChange -= value; }
    }
    public MatchState CurrentMatchState
    {
        get => currentMatchState;
        set
        {
            currentMatchState = value;
            MatchStateChange?.Invoke(this, EventArgs.Empty);
        }
    }

    public enum Team
    {
        NONE,
        BLUE,
        RED
    }
    private event EventHandler TeamStateChange;
    public event EventHandler OnTeamStateChange
    {
        add
        {
            if (TeamStateChange == null || !TeamStateChange.GetInvocationList().Contains(value))
                TeamStateChange += value;
        }
        remove { TeamStateChange -= value; }
    }
    public Team CurrentTeam
    {
        get => currentTeam;
        set
        {
            currentTeam = value;
            TeamStateChange?.Invoke(this, EventArgs.Empty);
        }
    }

    public int CurrentCollectedCoin
    {
        get => currentCollectedCoin;
        set => currentCollectedCoin = value;
    }

    //  ================================================

    [SerializeField] private CharacterController playerCC;
    [SerializeField] private Transform playerTF;

    [Header("SPAWN POINTS")]
    [SerializeField] private Transform blueSpawnPoint;
    [SerializeField] private Transform redSpawnPoint;

    [Header("PANELS")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject countdownPanel;

    [Header("COUNTDOWN")]
    [SerializeField] private TextMeshProUGUI countdownTMP;

    [Header("SCORE BOARD")]
    [SerializeField] private float startTime;
    [SerializeField] private TextMeshProUGUI timerTMP;
    [SerializeField] private TextMeshProUGUI blueScoreTMP;
    [SerializeField] private TextMeshProUGUI redScoreTMP;

    [Header("COINS")]
    [SerializeField] private TextMeshProUGUI currentCoinTMP;

    [Header("PAUSE MENU")]
    [SerializeField] private GameObject pauseMenuObj;

    [Header("FINISH STATUS")]
    [SerializeField] private TextMeshProUGUI finishStatusTMP;
    [SerializeField] private TextMeshProUGUI earningStatusTMP;
    [SerializeField] private GameObject finishStatusObj;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private MatchState currentMatchState;
    [ReadOnly][SerializeField] private Team currentTeam;
    [ReadOnly][SerializeField] private int currentRedScore;
    [ReadOnly][SerializeField] private int currentBlueScore;
    [ReadOnly][SerializeField] private int currentCollectedCoin;

    //  ================================================

    Coroutine countdownStart;
    Coroutine timerStart;

    //  ================================================

    private void Awake()
    {
        currentCoinTMP.text = "0";
    }

    private void OnDisable()
    {
        if (countdownStart != null) StopCoroutine(countdownStart);

        if (timerStart != null) StopCoroutine(timerStart);
    }

    #region INITIALIZER

    public IEnumerator FirstLoading()
    {
        gameplayPanel.SetActive(false);

        int rand = UnityEngine.Random.Range(0, 2);

        CurrentTeam = rand == 0 ? Team.BLUE : Team.RED;

        BrinPlayerToSpawnPoint();

        countdownStart = StartCoroutine(StartCountdownAtStart());

        yield return null;
    }

    private IEnumerator StartCountdownAtStart()
    {
        while (!GameManager.Instance.SceneController.DoneLoading) yield return null;

        CurrentMatchState = MatchState.GETREADY;

        float time = 3f;
        countdownTMP.text = time.ToString("n0");

        countdownPanel.SetActive(true);

        while (time > 0f)
        {
            countdownTMP.text = time.ToString("n0");
            time -= Time.deltaTime;

            yield return null;
        }

        countdownTMP.text = "GO!";

        countdownStart = null;

        timerStart = StartCoroutine(PlayTimer());

        gameplayPanel.SetActive(true);

        CurrentMatchState = MatchState.PLAY;

        yield return new WaitForSecondsRealtime(2f);

        countdownPanel.SetActive(false);
    }

    #endregion

    #region GAMEPLAY

    public void BrinPlayerToSpawnPoint()
    {
        playerCC.enabled = false;
        playerTF.position = CurrentTeam == Team.BLUE ? blueSpawnPoint.position : redSpawnPoint.position;
        playerTF.rotation = CurrentTeam == Team.BLUE ? blueSpawnPoint.rotation : redSpawnPoint.rotation;
        playerCC.enabled = true;
    }

    private IEnumerator PlayTimer()
    {
        blueScoreTMP.text = "0";
        redScoreTMP.text = "0";

        timerTMP.text = "00:00";

        float timeRemaining = startTime;
        float minutes;
        float seconds;

        while (timeRemaining > 0)
        {
            minutes = Mathf.FloorToInt(timeRemaining / 60);
            seconds = Mathf.FloorToInt(timeRemaining % 60);

            timerTMP.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            timeRemaining -= Time.deltaTime;

            yield return null;
        }

        CurrentMatchState = MatchState.FINISH;

        gameplayPanel.SetActive(false);
        finishStatusObj.SetActive(true);


        if (currentBlueScore == 0 && currentRedScore == 0)
            finishStatusTMP.text = "THE MATCH IS DRAW!";
        else
            finishStatusTMP.text = CurrentTeam == Team.BLUE ? "BLUE TEAM WINS!" : "RED TEAM WINS!";

        earningStatusTMP.text = "YOU EARNED " + currentCollectedCoin.ToString("n0") + " COINS";
    }

    public void AddRedTeamScore()
    {
        currentRedScore++;
        redScoreTMP.text = currentRedScore.ToString();
    }

    public void AddBlueTeamScore()
    {
        currentBlueScore++;
        blueScoreTMP.text = currentBlueScore.ToString();
    }

    public void ChangeCurrentCoin() => currentCoinTMP.text = currentCollectedCoin.ToString("n0");

    #endregion

    #region BUTTON

    public void PauseMenu()
    {
        Time.timeScale = 0f;
        pauseMenuObj.SetActive(true);
    }

    public void ExitPauseMenu()
    {
        Time.timeScale = 1f;
        pauseMenuObj.SetActive(false);
    }

    public void ReturnMainMenu()
    {
        GameManager.Instance.SceneController.CurrentScene = "MainMenu";
    }

    #endregion
}
