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

    private event EventHandler CurrentPlayerIndexChange;
    public event EventHandler OnCurrentPlayerIndexChange
    {
        add
        {
            if (CurrentPlayerIndexChange == null || !CurrentPlayerIndexChange.GetInvocationList().Contains(value))
                CurrentPlayerIndexChange += value;
        }
        remove { CurrentPlayerIndexChange -= value; }
    }
    public int CurrentPlayerIndex
    {
        get => currentTurnIndex;
        set
        {
            currentTurnIndex = value;
            CurrentPlayerIndexChange?.Invoke(this, EventArgs.Empty);
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
    [SerializeField] private GameObject teamIndicator;
    [SerializeField] private TextMeshProUGUI playerIndicatorTMP;

    [Header("TIMER")]
    [SerializeField] private GameObject startCountdownTimerObj;
    [SerializeField] private TextMeshProUGUI startCountdownTimerTMP;
    [SerializeField] private TextMeshProUGUI gameplayTimerTMP;

    [Header("FINISH STATUS")]
    [SerializeField] private TextMeshProUGUI finishStatusTMP;
    [SerializeField] private TextMeshProUGUI earningStatusTMP;
    [SerializeField] private GameObject finishStatusObj;

    [Header("TEAM")]
    [SerializeField] private List<Button> playerItemTeam; 

    [Header("PAUSE MENU")]
    [SerializeField] private GameObject pauseMenuObj;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private KTMultiplayerState currentKTState;
    [ReadOnly][SerializeField] private KTTeam currentKTTeam;
    [ReadOnly][SerializeField] private GameObject myPlayerObj;
    [ReadOnly][SerializeField] private float playerOneCurrentTimer;
    [ReadOnly][SerializeField] private float playerTwoCurrentTimer;
    [ReadOnly] public int currentStage;
    [ReadOnly][SerializeField] private bool holdJump;
    [ReadOnly] public bool isJumping;
    [ReadOnly][SerializeField] private float currentSpeedMoveGauge;
    [ReadOnly][SerializeField] private bool moveForward;
    [ReadOnly][SerializeField] KTTeam winner;
    [ReadOnly][SerializeField] private int currentTurnIndex;

    //  ============================

    Dictionary<Player, bool> readyPlayers;
    Dictionary<Player, bool> donePlayers;
    Dictionary<Player, GameObject> playerObjs;

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

    List<Player> playerList;

    List<Player> redTeamList;
    List<Player> blueTeamList;

    //  ============================


    private void OnEnable()
    {
        base.OnEnable();

        playerList = new List<Player>();
        redTeamList = new List<Player>();
        blueTeamList = new List<Player>();
        readyPlayers = new Dictionary<Player, bool>();
        donePlayers = new Dictionary<Player, bool>();
        playerObjs = new Dictionary<Player, GameObject>();

        InitializeHost();
        InitializePlayer();
        OnKTMultiplayerStateChange += KTStateEvents;
        OnCurrentPlayerIndexChange += TeamTurnEvents;
        PhotonNetwork.NetworkingClient.EventReceived += GameMultiplayerEvents;
    }

    private void OnDisable()
    {
        base.OnDisable();

        OnKTMultiplayerStateChange -= KTStateEvents;
        OnCurrentPlayerIndexChange -= TeamTurnEvents;
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

            if ((KTTeam)Convert.ToInt32(dataState[0]) == KTTeam.RED)
                redTeamList.Add((Player)dataState[1]);
            else
                blueTeamList.Add((Player)dataState[1]);

            if ((Player)dataState[1] == PhotonNetwork.LocalPlayer)
                CurrentKTTeam = (KTTeam)Convert.ToInt32(dataState[0]);
        }

        //  INSTANTIATE ENEMY OBJECT
        if (obj.Code == 22)
        {
            object[] dataState = (object[])obj.CustomData;

            GameObject objPlayer = Instantiate(playerPF);

            objPlayer.GetComponent<PhotonView>().ViewID = (int)dataState[0];
            objPlayer.GetComponent<PlayerKTMController>().ActivateCostume(dataState[1].ToString());

            playerObjs[(Player)dataState[2]] = objPlayer;
        }


        //  SET TEAM TURN
        if (obj.Code == 23)
        {
            object[] dataState = (object[])obj.CustomData;

            playerList.Add((Player)dataState[0]);
            readyPlayers.Add((Player)dataState[0], false);
            donePlayers.Add((Player)dataState[0], false);
            playerObjs.Add((Player)dataState[0], null);
        }

        //  SET PLAYER READY STATE
        if (obj.Code == 24)
        {
            object[] dataState = (object[])obj.CustomData;

            readyPlayers[(Player)dataState[0]] = true;
            donePlayers[(Player)dataState[0]] = false;
        }

        //  PLAYER OBJECT ENABLER
        if (obj.Code == 25)
        {
            object[] dataState = (object[])obj.CustomData;

            for (int a = 0; a < playerObjs.Count; a++)
            {
                if (playerObjs.ElementAt(0).Value != null)
                    playerObjs.ElementAt(a).Value.SetActive(false);
            }

            playerObjs[(Player)dataState[1]].SetActive((bool)dataState[0]);
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

            CurrentPlayerIndex = Convert.ToInt32(dataState[0]);

            for (int a = 0; a < playerItemTeam.Count; a++)
            {
                playerItemTeam[a].interactable = false;
            }

            playerItemTeam[CurrentPlayerIndex].interactable = true;
        }

        if (obj.Code == 30)
        {
            object[] dataState = (object[])obj.CustomData;

            //enemyPlayerObj.SetActive((bool)dataState[0]);
            //myPlayerObj.SetActive((bool)dataState[1]);
        }

        if (obj.Code == 31)
        {
            object[] dataState = (object[])obj.CustomData;

            donePlayers[(Player)dataState[0]] = (bool) dataState[1];
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
            {
                playerData.Credits += 15;
                finishStatusTMP.text = "THE MATCH IS DRAW!";
                earningStatusTMP.text = "YOU EARNED 15 COIN";
            }
            else
            {

                if ((KTTeam)Convert.ToInt32(dataState[0]) == CurrentKTTeam)
                {
                    playerData.Credits += 25;
                    finishStatusTMP.text = "YOU LOSE! THE ENEMY TEAM WINS!";
                    earningStatusTMP.text = "YOU EARNED 25 COIN";
                }
                else
                {
                    playerData.Credits += 50;
                    finishStatusTMP.text = "YOU WIN!";
                    earningStatusTMP.text = "YOU EARNED 50 COIN";
                }
            }

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
        GameManager.Instance.SceneController.AddActionLoadinList(SetTeamUI());
        GameManager.Instance.SceneController.AddActionLoadinList(CheckAllPlayerIfReadyHost());
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void InitializePlayer()
    {
        if (PhotonNetwork.IsMasterClient) return;

        currentStage = 1;
        GameManager.Instance.AudioSystem.SetBGMusic(bgCLip);
        GameManager.Instance.SceneController.AddActionLoadinList(WaitForPlayerTeam());
        GameManager.Instance.SceneController.AddActionLoadinList(SetTeamUI());
        GameManager.Instance.SceneController.AddActionLoadinList(CheckAllPlayerReadyPlayer());
        GameManager.Instance.SceneController.ActionPass = true;
    }

    #region HOST

    IEnumerator SetPlayerTeam()
    {
        gameplayTimerTMP.text = string.Format("{0:00}:{1:00}", 0f, 0f);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerList.Add(player);
            yield return null;
        }

        yield return playerList.Shuffle();

        for (int a = 0; a < playerList.Count; a++)
        {
            #region POPULATE PLAYER LIST DICTIONARY

            readyPlayers.Add(playerList[a], false);
            donePlayers.Add(playerList[a], false);
            playerObjs.Add(playerList[a], null);

            data = new object[]
            {
                playerList[a]
            };

            PhotonNetwork.RaiseEvent(23, data, raiseEventOptions, sendOptions);

            #endregion
            #region ASSIGN TEAM
            if (a <= 2)
            {
                redTeamList.Add(playerList[a]);
                if (playerList[a] == PhotonNetwork.LocalPlayer)
                    CurrentKTTeam = KTTeam.RED;
                else
                {
                    data = new object[]
                    {
                        (int) KTTeam.RED, playerList[a]
                    };
                    PhotonNetwork.RaiseEvent(21, data, raiseEventOptions, sendOptions);
                }
            }
            else
            {
                blueTeamList.Add(playerList[a]);
                if (playerList[a] == PhotonNetwork.LocalPlayer)
                    CurrentKTTeam = KTTeam.BLUE;
                else
                {
                    data = new object[]
                    {
                        (int) KTTeam.BLUE, playerList[a]
                    };
                    PhotonNetwork.RaiseEvent(21, data, raiseEventOptions, sendOptions);
                }
            }
            #endregion
            yield return null;
        }

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
                playerData.EquippedCharacter,
                PhotonNetwork.LocalPlayer
            };

            PhotonNetwork.RaiseEvent(22, data, raiseEventOptions, sendOptions);
        }

        playerObjs[PhotonNetwork.LocalPlayer] = obj;
        obj.SetActive(false);
        myPlayerObj = obj;

        data = new object[]
        {
            false, PhotonNetwork.LocalPlayer
        };

        PhotonNetwork.RaiseEvent(25, data, raiseEventOptions, sendOptions);

        obj.transform.position = spawnPoint;

        yield return null;
    }

    //IEnumerator SetFirstTurn()
    //{
    //    while (playerObjs.Count < 6) yield return null;

    //    int randTeam = UnityEngine.Random.Range(0, 2);

    //    if (randTeam == 0)
    //        TeamTurn = KTTeam.RED;
    //    else
    //        TeamTurn = KTTeam.BLUE;

    //    data = new object[]
    //    {
    //        (int) TeamTurn
    //    };
    //    PhotonNetwork.RaiseEvent(23, data, raiseEventOptions, sendOptions);

    //    yield return null;
    //}

    IEnumerator SetTeamUI()
    {
        for (int a = 0; a < playerItemTeam.Count; a++)
        {
            playerItemTeam[a].interactable = false;
            yield return null;
        }

        playerItemTeam[CurrentPlayerIndex].interactable = true;
    }

    IEnumerator CheckAllPlayerIfReadyHost()
    {
        while (playerObjs.ContainsValue(null)) yield return null;

        ChangeVCamPlayer();

        readyPlayers[PhotonNetwork.LocalPlayer] = true;
        donePlayers[PhotonNetwork.LocalPlayer] = false;

        data = new object[]
        {
            PhotonNetwork.LocalPlayer
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
                playerData.EquippedCharacter,
                PhotonNetwork.LocalPlayer
            };

            PhotonNetwork.RaiseEvent(22, data, raiseEventOptions, sendOptions);
        }

        playerObjs[PhotonNetwork.LocalPlayer] = obj;
        myPlayerObj = obj;
        data = new object[]
        {
            false, PhotonNetwork.LocalPlayer
        };

        PhotonNetwork.RaiseEvent(25, data, raiseEventOptions, sendOptions);

        obj.SetActive(false);
        obj.transform.position = spawnPoint;
    }

    IEnumerator CheckAllPlayerReadyPlayer()
    {
        while (playerObjs.ContainsValue(null)) yield return null;

        readyPlayers[PhotonNetwork.LocalPlayer] = true;
        donePlayers[PhotonNetwork.LocalPlayer] = false;

        data = new object[]
        {
            PhotonNetwork.LocalPlayer
        };

        PhotonNetwork.RaiseEvent(24, data, raiseEventOptions, sendOptions);

        ChangeVCamPlayer();

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
        float currentTime = CurrentPlayerIndex <= 2 ? playerOneCurrentTimer : playerTwoCurrentTimer;
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

            if (CurrentPlayerIndex <= 2)
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

        for (int a = 0; a < playerObjs.Count; a++)
        {
            if (playerObjs.ElementAt(a).Value != null)
                playerObjs.ElementAt(a).Value.SetActive(false);
        }

        playerObjs.ElementAt(CurrentPlayerIndex).Value.SetActive(true);

        data = new object[]
        {
            true, playerList.ElementAt(CurrentPlayerIndex)
        };

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
    }

    IEnumerator FindPlayerNextTurn()
    {
        if (CurrentPlayerIndex >= 5)
            CurrentPlayerIndex = 0;
        else
            CurrentPlayerIndex++;

        bool findPlayer = false;
        int tempIndex = CurrentPlayerIndex;

        while (!findPlayer)
        {
            if (donePlayers.ElementAt(tempIndex).Value)
            {
                if (tempIndex >= 5)
                    tempIndex = 0;

                tempIndex++;
            }
            else
            {
                CurrentPlayerIndex = tempIndex;
                findPlayer = true;
            }
            yield return null;
        }

        for (int a = 0; a < playerItemTeam.Count; a++)
        {
            playerItemTeam[a].interactable = false;
        }

        playerItemTeam[CurrentPlayerIndex].interactable = true;

        data = new object[]
        {
                CurrentPlayerIndex
        };

        PhotonNetwork.RaiseEvent(29, data, raiseEventOptions, sendOptions);
    }

    public void TellServerToNextPlayer()
    {
        currentStage = 1;

        myPlayerObj.SetActive(false);

        if (donePlayers.ContainsValue(false))
        {
            StartCoroutine(FindPlayerNextTurn());
        }
        else
        {
            //  FINISH MATCH NOW
            CurrentKTState = KTMultiplayerState.FINISH;
            data = new object[]
            {
                    (int) CurrentKTState,
            };
            PhotonNetwork.RaiseEvent(32, data, raiseEventOptions, sendOptions);
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
        {
            playerData.Credits += 15;
            finishStatusTMP.text = "THE MATCH IS DRAW!";
            earningStatusTMP.text = "YOU EARNED 15 COIN";
        }
        else
        {
            if (playerOneCurrentTimer > playerTwoCurrentTimer)
            {
                playerData.Credits += 25;
                finishStatusTMP.text = "YOU LOSE! THE ENEMY TEAM WINS!";
                earningStatusTMP.text = "YOU EARNED 25 COIN";
            }
            else
            {
                playerData.Credits += 50;
                finishStatusTMP.text = "YOU WIN!";
                earningStatusTMP.text = "YOU EARNED 50 COIN";
            }
        }


        finishStatusObj.SetActive(true);
        pauseMenuObj.SetActive(false);
        gameplayUI.SetActive(false);
    }

    public void FinishPlayer()
    {
        donePlayers[PhotonNetwork.LocalPlayer] = true;

        data = new object[]
        {
            PhotonNetwork.LocalPlayer, true
        };

        PhotonNetwork.RaiseEvent(31, data, raiseEventOptions, sendOptions);
    }

    #endregion

    #endregion

    #region NON MULTIPLAYER

    private void KTStateEvents(object sender, EventArgs e)
    {
        EnablePlayerObject();
        IndicatorEnabler();
        GameplayUIEnabler();
        ActivateTimer();
        ActivateGameplayTimer();
        SetWinner();
    }

    private void TeamTurnEvents(object sender, EventArgs e)
    {
        ChangeVCamPlayer();
        CheckIfDoneRound();
        EnablePlayerObject();
    }

    #region GAMEPLAY

    private void ChangeVCamPlayer()
    {
        vcamera.m_Follow = playerObjs.ElementAt(CurrentPlayerIndex).Value.transform;
        vcamera.m_LookAt = playerObjs.ElementAt(CurrentPlayerIndex).Value.transform;
    }

    private void IndicatorEnabler()
    {
        if (CurrentKTState != KTMultiplayerState.COUNTDOWN)
        {
            playerIndicator.SetActive(false);
            enemyIndicator.SetActive(false);
            return;
        }

        if (playerObjs.ElementAt(currentTurnIndex).Value == myPlayerObj)
        {
            playerIndicatorTMP.text = "YOUR TURN\n YOU'RE PLAYER " + (currentTurnIndex + 1); 
            playerIndicator.SetActive(true);
        }
        else
        {
            if (CurrentKTTeam == KTTeam.RED)
            {
                if (redTeamList.Contains(playerList.ElementAt(currentTurnIndex)))
                    teamIndicator.SetActive(true);
                else
                    enemyIndicator.SetActive(true);
            }
            else
            {
                if (blueTeamList.Contains(playerList.ElementAt(currentTurnIndex)))
                    teamIndicator.SetActive(true);
                else
                    enemyIndicator.SetActive(true);
            }
        }
    }

    private void GameplayUIEnabler()
    {
        if (CurrentKTState != KTMultiplayerState.GAME)
        {
            gameplayUI.SetActive(false);
            return;
        }
        gameplayUI.SetActive(true);
        jumpButtonObj.SetActive(playerList[currentTurnIndex].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber ? true : false);
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

        if (playerList[currentTurnIndex] != PhotonNetwork.LocalPlayer) return;

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
