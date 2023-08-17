using Cinemachine;
using ExitGames.Client.Photon;
using MyBox;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static KarerangTalonCore;

public class KarerangTalonMultiplayerCore : MonoBehaviourPunCallbacks
{
    public enum KTMultiplayerState
    {
        NONE,
        COUNTDOWN,
        GAME,
        FINISH
    }
    private event EventHandler KTMultplayerStateChange;
    public event EventHandler OnKTMultiplayerStateChange
    {
        add
        {
            if (KTMultplayerStateChange == null || !KTMultplayerStateChange.GetInvocationList().Contains(value))
                KTMultplayerStateChange += value;
        }
        remove { KTMultplayerStateChange -= value; }
    }
    public KTMultiplayerState CurrentKTState
    {
        get => currentKTState;
        set
        {
            currentKTState = value;
            KTMultplayerStateChange?.Invoke(this, EventArgs.Empty);
        }
    }

    public enum KTTeam 
    {
        NONE,
        RED,
        BLUE,
        DRAW
    }
    public KTTeam CurrentKTTeam
    {
        get => currentKTTeam;
        set => currentKTTeam = value;
    }


    private event EventHandler TeamTurnChange;
    public event EventHandler OnTeamTurnChange
    {
        add
        {
            if (TeamTurnChange == null || !TeamTurnChange.GetInvocationList().Contains(value))
                TeamTurnChange += value;
        }
        remove { TeamTurnChange -= value; }
    }
    public KTTeam TeamTurn
    {
        get => teamTurn;
        set
        {
            teamTurn = value;
            TeamTurnChange?.Invoke(this, EventArgs.Empty);
        }
    }

    //  ============================

    [SerializeField] private PlayeData playerData;
    [SerializeField] private AudioClip bgCLip;

    [Header("GAMEPLAY")]
    public Vector3 spawnPoint;
    [SerializeField] private CinemachineVirtualCamera vcamera;

    [Header("JUMP")]
    [SerializeField] private float speedMoveGauge;
    [SerializeField] private float minGauge;
    [SerializeField] private float maxGauge;
    [SerializeField] private float stageOneJumpStrength;
    [SerializeField] private float stageTwoJumpStrength;
    [SerializeField] private float stageThreeJumpStrength;
    [SerializeField] private Slider gaugeSlider;

    [Header("PREFABS")]
    [SerializeField] private GameObject playerPF;

    [Header("UI")]
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject playerIndicator;
    [SerializeField] private GameObject enemyIndicator;
    [SerializeField] private GameObject jumpButtonObj;

    [Header("TIMER")]
    [SerializeField] private GameObject startCountdownTimerObj;
    [SerializeField] private TextMeshProUGUI startCountdownTimerTMP;
    [SerializeField] private TextMeshProUGUI gameplayTimerTMP;

    [Header("FINISH STATUS")]
    [SerializeField] private TextMeshProUGUI finishStatusTMP;
    [SerializeField] private TextMeshProUGUI earningStatusTMP;
    [SerializeField] private GameObject finishStatusObj;

    [Header("PAUSE MENU")]
    [SerializeField] private GameObject pauseMenuObj;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private KTMultiplayerState currentKTState;
    [ReadOnly][SerializeField] private KTTeam currentKTTeam;
    [ReadOnly][SerializeField] private GameObject myPlayerObj;
    [ReadOnly][SerializeField] private GameObject enemyPlayerObj;
    [ReadOnly][SerializeField] private KTTeam teamTurn;
    [ReadOnly][SerializeField] private float playerOneCurrentTimer;
    [ReadOnly][SerializeField] private float playerTwoCurrentTimer;
    [ReadOnly] public int currentStage;
    [ReadOnly][SerializeField] private bool holdJump;
    [ReadOnly] public bool isJumping;
    [ReadOnly][SerializeField] private float currentSpeedMoveGauge;
    [ReadOnly][SerializeField] private bool moveForward;
    [ReadOnly][SerializeField] KTTeam winner;

    //  ============================

    Dictionary<string, bool> readyPlayers;
    Dictionary<KTTeam, bool> donePlayers;

    //  ============================

    object[] data = new object[] { };
    RaiseEventOptions raiseEventOptions = new RaiseEventOptions
    {
        Receivers = ReceiverGroup.Others
    };
    RaiseEventOptions raiseEventOptionsAll = new RaiseEventOptions
    {
        Receivers = ReceiverGroup.All
    };
    SendOptions sendOptions = new SendOptions
    {
        Reliability = true
    };

    Coroutine gameplayTimerCoroutine;

    //  ============================


    private void OnEnable()
    {
        base.OnEnable();

        readyPlayers = new Dictionary<string, bool>
        {
            { "player1", false },
            { "player2", false }
        };
        donePlayers = new Dictionary<KTTeam, bool>
        {
            { KTTeam.RED, false },
            { KTTeam.BLUE, false }
        };

        InitializeHost();
        InitializePlayer();
        OnKTMultiplayerStateChange += KTStateEvents;
        OnTeamTurnChange += TeamTurnEvents;
        PhotonNetwork.NetworkingClient.EventReceived += GameMultiplayerEvents;
    }

    private void OnDisable()
    {
        base.OnDisable();

        OnKTMultiplayerStateChange -= KTStateEvents;
        OnTeamTurnChange += TeamTurnEvents;
        PhotonNetwork.NetworkingClient.EventReceived -= GameMultiplayerEvents;
    }

    private void Update()
    {
        MoveGauge();
    }

    #region MULTIPLAYER

    private void GameMultiplayerEvents(EventData obj)
    {
        //  SET TEAM TO PLAYERS
        if (obj.Code == 21)
        {
            object[] dataState = (object[])obj.CustomData;

            if (Convert.ToInt32(dataState[0]) == 2)
                CurrentKTTeam = KTTeam.BLUE;
            else
                CurrentKTTeam = KTTeam.RED;
        }

        //  INSTANTIATE ENEMY OBJECT
        if (obj.Code == 22)
        {
            object[] dataState = (object[])obj.CustomData;

            GameObject objPlayer = Instantiate(playerPF);

            objPlayer.GetComponent<PhotonView>().ViewID = (int)dataState[0];
            objPlayer.GetComponent<PlayerKTMController>().ActivateCostume(dataState[1].ToString());

            enemyPlayerObj = objPlayer;
        }


        //  SET TEAM TURN
        if (obj.Code == 23)
        {
            object[] dataState = (object[])obj.CustomData;

            TeamTurn = (KTTeam) Convert.ToInt32(dataState[0]);
        }

        //  SET PLAYER READY STATE
        if (obj.Code == 24)
        {
            object[] dataState = (object[])obj.CustomData;

            readyPlayers[dataState[0].ToString()] = true;
        }

        //  PLAYER OBJECT ENABLER
        if (obj.Code == 25)
        {
            object[] dataState = (object[])obj.CustomData;

            enemyPlayerObj.SetActive((bool)dataState[0]);
        }

        //  CHANGE KT STATE
        if (obj.Code == 26)
        {
            object[] dataState = (object[])obj.CustomData;

            CurrentKTState = (KTMultiplayerState)Convert.ToInt32(dataState[0]);
        }

        //  START COUNTDOWN TIMER
        if (obj.Code == 27)
        {
            object[] dataState = (object[])obj.CustomData;

            startCountdownTimerTMP.text = dataState[0].ToString();
        }

        //  GAMEPLAY TIMER
        if (obj.Code == 28)
        {
            object[] dataState = (object[])obj.CustomData;

            gameplayTimerTMP.text = string.Format("{0:00}:{1:00}", float.Parse(dataState[0].ToString()),
                float.Parse(dataState[1].ToString()));
        }

        //  SET TEAM ALL PLAYER SEND
        if (obj.Code == 29)
        {
            object[] dataState = (object[])obj.CustomData;

            TeamTurn = (KTTeam)Convert.ToInt32(dataState[0]);
        }

        if (obj.Code == 30)
        {
            object[] dataState = (object[])obj.CustomData;

            enemyPlayerObj.SetActive((bool)dataState[0]);
            myPlayerObj.SetActive((bool)dataState[1]);
        }

        if (obj.Code == 31)
        {
            object[] dataState = (object[])obj.CustomData;

            donePlayers[(KTTeam)Convert.ToInt32(dataState[0])] = (bool) dataState[1];
        }

        if (obj.Code == 32)
        {
            object[] dataState = (object[])obj.CustomData;

            CurrentKTState = (KTMultiplayerState)Convert.ToInt32(dataState[0]);
        }

        if (obj.Code == 33)
        {
            object[] dataState = (object[])obj.CustomData;


            if ((KTTeam)Convert.ToInt32(dataState[0]) == KTTeam.DRAW)
                finishStatusTMP.text = "THE MATCH IS DRAW!";
            else
                finishStatusTMP.text = (KTTeam)Convert.ToInt32(dataState[0]) == CurrentKTTeam ? 
                    "YOU WIN!" : "YOU LOSE! THE ENEMY TEAM WINS!";

            earningStatusTMP.text = "YOU EARNED 0 COIN";

            finishStatusObj.SetActive(true);
            pauseMenuObj.SetActive(false);
            gameplayUI.SetActive(false);
        }
    }

    #region INITIALIZE

    private void InitializeHost()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        currentStage = 1;
        GameManager.Instance.AudioSystem.SetBGMusic(bgCLip);
        GameManager.Instance.SceneController.AddActionLoadinList(SetPlayerTeam());
        GameManager.Instance.SceneController.AddActionLoadinList(SpawnPlayerHost());
        GameManager.Instance.SceneController.AddActionLoadinList(SetFirstTurn());
        GameManager.Instance.SceneController.AddActionLoadinList(CheckAllPlayerIfReadyHost());
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void InitializePlayer()
    {
        if (PhotonNetwork.IsMasterClient) return;

        currentStage = 1;
        GameManager.Instance.AudioSystem.SetBGMusic(bgCLip);
        GameManager.Instance.SceneController.AddActionLoadinList(WaitForPlayerTeam());
        GameManager.Instance.SceneController.AddActionLoadinList(CheckAllPlayerReadyPlayer());
        GameManager.Instance.SceneController.ActionPass = true;
    }

    #region HOST

    IEnumerator SetPlayerTeam()
    {
        gameplayTimerTMP.text = string.Format("{0:00}:{1:00}", 0f, 0f);
        int randTeam = UnityEngine.Random.Range(0, 2);

        if (randTeam == 0)
        {
            CurrentKTTeam = KTTeam.RED;

            data = new object[]
            {
                (int) KTTeam.BLUE
            };
        }
        else
        {
            CurrentKTTeam = KTTeam.BLUE;

            data = new object[]
            {
                (int) KTTeam.RED
            };
        }
        PhotonNetwork.RaiseEvent(21, data, raiseEventOptions, sendOptions);
        yield return null;
    }

    IEnumerator SpawnPlayerHost()
    {
        GameObject obj = Instantiate(playerPF);

        obj.GetComponent<PlayerKTMController>().ActivateCostume(playerData.EquippedCharacter);
        obj.GetComponent<PlayerKTMController>().core = this;

        PhotonView player = obj.GetComponent<PhotonView>();

        if (PhotonNetwork.AllocateViewID(player))
        {
            data = new object[]
            {
                player.ViewID,
                playerData.EquippedCharacter
            };

            PhotonNetwork.RaiseEvent(22, data, raiseEventOptions, sendOptions);
        }

        myPlayerObj = obj;

        myPlayerObj.SetActive(false);
        data = new object[]
        {
            false
        };

        PhotonNetwork.RaiseEvent(25, data, raiseEventOptions, sendOptions);

        myPlayerObj.transform.position = spawnPoint;

        yield return null;
    }

    IEnumerator SetFirstTurn()
    {
        while (enemyPlayerObj == null && myPlayerObj == null) yield return null;

        int randTeam = UnityEngine.Random.Range(0, 2);

        if (randTeam == 0)
            TeamTurn = KTTeam.RED;
        else
            TeamTurn = KTTeam.BLUE;

        data = new object[]
        {
            (int) TeamTurn
        };
        PhotonNetwork.RaiseEvent(23, data, raiseEventOptions, sendOptions);

        yield return null;
    }

    IEnumerator CheckAllPlayerIfReadyHost()
    {
        while (enemyPlayerObj == null && myPlayerObj == null) yield return null;

        readyPlayers["player1"] = true;

        data = new object[]
        {
            "player1"
        };

        PhotonNetwork.RaiseEvent(24, data, raiseEventOptions, sendOptions);

        while (readyPlayers.Values.Contains(false)) yield return null;

        StartCoroutine(ChangeToTimer(3f));
    }

    IEnumerator ChangeToTimer(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        data = new object[]
        {
            (int) KTMultiplayerState.COUNTDOWN
        };
        PhotonNetwork.RaiseEvent(26, data, raiseEventOptionsAll, sendOptions);
    }

    #endregion

    #region PLAYER

    IEnumerator WaitForPlayerTeam()
    {
        gameplayTimerTMP.text = string.Format("{0:00}:{1:00}", 0f, 0f);

        while (CurrentKTTeam == KTTeam.NONE) yield return null;

        //  INSTANTIATE PLAYER HERE

        GameObject obj = Instantiate(playerPF);

        obj.GetComponent<PlayerKTMController>().ActivateCostume(playerData.EquippedCharacter);
        obj.GetComponent<PlayerKTMController>().core = this;

        PhotonView player = obj.GetComponent<PhotonView>();

        if (PhotonNetwork.AllocateViewID(player))
        {
            data = new object[]
            {
                player.ViewID,
                playerData.EquippedCharacter
            };

            PhotonNetwork.RaiseEvent(22, data, raiseEventOptions, sendOptions);
        }

        myPlayerObj = obj;

        data = new object[]
        {
            false
        };

        PhotonNetwork.RaiseEvent(25, data, raiseEventOptions, sendOptions);

        myPlayerObj.SetActive(false);
        myPlayerObj.transform.position = spawnPoint;

        data = new object[]
        {
            false
        };

        PhotonNetwork.RaiseEvent(25, data, raiseEventOptions, sendOptions);
    }

    IEnumerator CheckAllPlayerReadyPlayer()
    {
        while (enemyPlayerObj == null && myPlayerObj == null) yield return null;

        readyPlayers["player2"] = true;

        data = new object[]
        {
            "player2"
        };

        PhotonNetwork.RaiseEvent(24, data, raiseEventOptions, sendOptions);

        while (readyPlayers.Values.Contains(false)) yield return null;

    }

    #endregion

    #endregion

    #region GAMEPLAY

    #region HOST

    IEnumerator StartCountdownTimer()
    {
        yield return new WaitForSecondsRealtime(2f);
        int time = 3;

        data = new object[]
        {
            time
        };

        while (time > 0)
        {
            startCountdownTimerTMP.text = time.ToString();

            PhotonNetwork.RaiseEvent(27, data, raiseEventOptions, sendOptions);

            yield return new WaitForSecondsRealtime(1f);

            time -= 1;

            data[0] = time;

            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);

        startCountdownTimerTMP.text = "GO";

        data = new object[]
        {
            "GO!"
        };
        PhotonNetwork.RaiseEvent(27, data, raiseEventOptions, sendOptions);

        yield return new WaitForSecondsRealtime(0.5f);

        CurrentKTState = KTMultiplayerState.GAME;

        data = new object[]
        {
            (int) CurrentKTState
        };
        PhotonNetwork.RaiseEvent(26, data, raiseEventOptions, sendOptions);
    }

    IEnumerator GameplayeTimerCount()
    {
        float currentTime = TeamTurn == CurrentKTTeam ? playerOneCurrentTimer : playerTwoCurrentTimer;
        float minutes, seconds;

        minutes = Mathf.FloorToInt(currentTime / 60);
        seconds = Mathf.FloorToInt(currentTime % 60);

        data = new object[]
        {
            minutes, seconds
        };

        while (true)
        {
            minutes = Mathf.FloorToInt(currentTime / 60);
            seconds = Mathf.FloorToInt(currentTime % 60);

            gameplayTimerTMP.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            data = new object[]
            {
                minutes, seconds
            };
            PhotonNetwork.RaiseEvent(28, data, raiseEventOptions, sendOptions);

            yield return new WaitForSecondsRealtime(1f);

            currentTime += 1f;

            if (TeamTurn == CurrentKTTeam)
                playerOneCurrentTimer = currentTime;
            else
                playerTwoCurrentTimer = currentTime;

            yield return null;
        }
    }

    private void ActivateTimer()
    {
        if (CurrentKTState != KTMultiplayerState.COUNTDOWN)
        {
            startCountdownTimerObj.SetActive(false);
            return;
        }

        startCountdownTimerObj.SetActive(true);

        if (!PhotonNetwork.IsMasterClient) return;

        StartCoroutine(StartCountdownTimer());
    }

    private void EnablePlayerObject()
    {
        if (CurrentKTState != KTMultiplayerState.COUNTDOWN) return;

        if (teamTurn == CurrentKTTeam)
        {
            myPlayerObj.SetActive(true);

            data = new object[]
            {
                true
            };
        }
        else
        {
            myPlayerObj.SetActive(false);

            data = new object[]
            {
                false
            };
        }
        PhotonNetwork.RaiseEvent(25, data, raiseEventOptions, sendOptions);
    }

    private void ActivateGameplayTimer()
    {
        if (CurrentKTState != KTMultiplayerState.GAME) return;

        if (!PhotonNetwork.IsMasterClient) return;

        gameplayTimerCoroutine = StartCoroutine(GameplayeTimerCount());
    }


    #endregion

    private void CheckIfDoneRound()
    {
        if (CurrentKTState != KTMultiplayerState.GAME) return;

        if (!PhotonNetwork.IsMasterClient) return;

        StartCoroutine(ChangeToTimer(0f));

        if (CurrentKTTeam == TeamTurn)
        {
            myPlayerObj.SetActive(true);
            enemyPlayerObj.SetActive(false);

            data = new object[]
            {
                true,
                false
            };
        }
        else
        {
            myPlayerObj.SetActive(false);
            enemyPlayerObj.SetActive(true);

            data = new object[]
            {
                false,
                true
            };
        }

        PhotonNetwork.RaiseEvent(30, data, raiseEventOptions, sendOptions);

        //  change to countdown
    }

    public void TellServerToNextPlayer()
    {
        KTTeam nextTeam = CurrentKTTeam == KTTeam.BLUE ? KTTeam.RED : KTTeam.BLUE;

        currentStage = 1;

        myPlayerObj.SetActive(false);

        if (donePlayers.ContainsValue(false))
        {
            if (!donePlayers[nextTeam])
            {
                data = new object[]
                {
                (int) nextTeam
                };
            }
            else
            {
                data = new object[]
                {
                (int) CurrentKTTeam
                };
            }
            PhotonNetwork.RaiseEvent(29, data, raiseEventOptionsAll, sendOptions);
        }
        else
        {
            CurrentKTState = KTMultiplayerState.FINISH;

            data = new object[]
            {
                (int) CurrentKTState,
            };
        }
    }

    private void SetWinner()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (CurrentKTState != KTMultiplayerState.FINISH) return;

        if (playerOneCurrentTimer > playerTwoCurrentTimer)
        {
            if (CurrentKTTeam == KTTeam.RED)
                winner = KTTeam.BLUE;
            else
                winner = KTTeam.RED;
        }
        else if (playerOneCurrentTimer == playerTwoCurrentTimer || playerTwoCurrentTimer == playerOneCurrentTimer)
            winner = KTTeam.DRAW;
        else
            winner = CurrentKTTeam;

        data = new object[]
        {
            (int) CurrentKTTeam
        };
        PhotonNetwork.RaiseEvent(33, data, raiseEventOptionsAll, sendOptions);

        if (playerOneCurrentTimer == playerTwoCurrentTimer || playerTwoCurrentTimer == playerOneCurrentTimer)
            finishStatusTMP.text = "THE MATCH IS DRAW!";
        else
            finishStatusTMP.text = playerOneCurrentTimer > playerTwoCurrentTimer ? "YOU LOSE! THE ENEMY TEAM WINS!" : "YOU WIN!";

        earningStatusTMP.text = "YOU EARNED 0 COIN";

        finishStatusObj.SetActive(true);
        pauseMenuObj.SetActive(false);
        gameplayUI.SetActive(false);
    }

    public void FinishPlayer()
    {
        donePlayers[CurrentKTTeam] = true;

        data = new object[]
        {
            (int) CurrentKTTeam, true
        };

        PhotonNetwork.RaiseEvent(31, data, raiseEventOptions, sendOptions);
    }

    #endregion

    #endregion

    #region NON MULTIPLAYER

    private void KTStateEvents(object sender, EventArgs e)
    {
        IndicatorEnabler();
        GameplayUIEnabler();
        ActivateTimer();
        EnablePlayerObject();
        ActivateGameplayTimer();
        SetWinner();
    }

    private void TeamTurnEvents(object sender, EventArgs e)
    {
        ChangeVCamPlayer();
        CheckIfDoneRound();
    }

    #region GAMEPLAY

    private void ChangeVCamPlayer()
    {
        if (TeamTurn == CurrentKTTeam)
        {
            vcamera.m_Follow = myPlayerObj.transform;
            vcamera.m_LookAt = myPlayerObj.transform;
        }
        else
        {
            vcamera.m_Follow = enemyPlayerObj.transform;
            vcamera.m_LookAt = enemyPlayerObj.transform;
        }
    }

    private void IndicatorEnabler()
    {
        if (CurrentKTState != KTMultiplayerState.COUNTDOWN)
        {
            playerIndicator.SetActive(false);
            enemyIndicator.SetActive(false);
            return;
        }

        if (TeamTurn == CurrentKTTeam)
            playerIndicator.SetActive(true);
        else
            enemyIndicator.SetActive(true);
    }

    private void GameplayUIEnabler()
    {
        if (CurrentKTState != KTMultiplayerState.GAME)
        {
            gameplayUI.SetActive(false);
            return;
        }

        gameplayUI.SetActive(true);
        jumpButtonObj.SetActive(TeamTurn == CurrentKTTeam ? true : false);
    }

    private void MoveGauge()
    {
        if (!holdJump) return;

        if (gaugeSlider.value >= 1f)
            moveForward = false;
        else if (gaugeSlider.value <= 0f)
            moveForward = true;

        if (currentStage == 1)
            currentSpeedMoveGauge = speedMoveGauge;
        else if (currentStage == 2)
            currentSpeedMoveGauge = speedMoveGauge + 0.5f;
        else if (currentStage == 3)
            currentSpeedMoveGauge = speedMoveGauge + 1;

        if (moveForward)
            gaugeSlider.value += currentSpeedMoveGauge * Time.deltaTime;
        else
            gaugeSlider.value -= currentSpeedMoveGauge * Time.deltaTime;
    }

    private void JumpPlayer()
    {
        if (CurrentKTState != KTMultiplayerState.GAME) return;

        isJumping = true;

        float jumpStrength = 0f;

        if (gaugeSlider.value < minGauge)
        {
            if (currentStage == 1)
                jumpStrength = 2;
            else if (currentStage == 2)
                jumpStrength = 3;
            else
                jumpStrength = 4;
        }
        else
        {
            if (currentStage == 1)
                jumpStrength = stageOneJumpStrength + 2;
            else if (currentStage == 2)
                jumpStrength = stageTwoJumpStrength + 2;
            else
                jumpStrength = stageThreeJumpStrength + 2;
        }

        myPlayerObj.GetComponent<PlayerKTMController>().JumpPlayer(jumpStrength);
        gaugeSlider.gameObject.SetActive(false);
    }

    #endregion

    #region BUTTON

    public void JumpPressed()
    {
        if (CurrentKTState != KTMultiplayerState.GAME) return;

        if (TeamTurn != CurrentKTTeam) return;

        if (holdJump) return;

        if (isJumping) return;

        gaugeSlider.value = 0f;
        moveForward = true;
        gaugeSlider.gameObject.SetActive(true);
        holdJump = true;
    }

    public void JumpRelease()
    {
        if (!holdJump) return;

        holdJump = false;
        JumpPlayer();
    }

    public void BackToMainMenu()
    {
        PhotonNetwork.Disconnect();
        GameManager.Instance.SceneController.MultiplayerScene = false;
        GameManager.Instance.SceneController.CurrentScene = "MainMenu";
    }

    public void PauseMenuEnabler(bool value) => pauseMenuObj.SetActive(value);

    #endregion

    #endregion
}
