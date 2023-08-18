using ExitGames.Client.Photon;
using MyBox;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviourPunCallbacks
{
    public enum LobbyState
    {
        NONE,
        CONNECTING,
        LOBBY,
        FINDING,
        FOUND
    }
    private event EventHandler LobbyStateChange;
    public event EventHandler OnLobbyStateChange
    {
        add
        {
            if (LobbyStateChange == null || !LobbyStateChange.GetInvocationList().Contains(value))
                LobbyStateChange += value;
        }
        remove { LobbyStateChange -= value; }
    }
    public LobbyState CurrentLobbyState
    {
        get => currentLobbyState;
        set
        {
            currentLobbyState = value;
            LobbyStateChange.Invoke(this, EventArgs.Empty);
        }
    }

    //  =======================

    [SerializeField] private GameObject lobbyParentObj;
    [SerializeField] private GameObject connectingToServerObj;
    [SerializeField] private GameObject lobbyObj;
    [SerializeField] private GameObject findingMatchObj;
    [SerializeField] private GameObject matchFoundObj;

    [Header("TMP")]
    [SerializeField] private TextMeshProUGUI findTimerTMP;
    [SerializeField] private TextMeshProUGUI matchFoundTimerTMP;

    [Header("BUTTONS")]
    [SerializeField] private Button findMatchBtn;
    [SerializeField] private Button findingMatchBackBtn;
    [SerializeField] private Button lobbyBackBtn;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private LobbyState currentLobbyState;
    [ReadOnly][SerializeField] private float currentTimeFind;
    [ReadOnly] public string currentMapCode;
    [ReadOnly][SerializeField] private List<Player> playerList;

    //  =========================

    //  NOTES: 
    //  MAP CODES:
    //  001 - KARERANG TALON
    //  002 - PATINTERO
    //  003 - TUMBANG PRESO
    //  004 - AGAWAN BASE

    private void OnEnable()
    {
        base.OnEnable();
        playerList = new List<Player>();
        OnLobbyStateChange += CheckLobbyState;
        PhotonNetwork.NetworkingClient.EventReceived += MatchmakingEvents;
    }

    private void OnDisable()
    {
        base.OnDisable();
        OnLobbyStateChange -= CheckLobbyState;
        PhotonNetwork.NetworkingClient.EventReceived -= MatchmakingEvents;
    }

    private void Update()
    {
        FindMatchTimer();
    }

    private void MatchmakingEvents(EventData obj)
    {
        if (obj.Code == 19)
        {
            object[] dataState = (object[])obj.CustomData;

            CurrentLobbyState = (LobbyState)(int)dataState[0];
        }

        if (obj.Code == 20)
        {
            object[] dataState = (object[])obj.CustomData;

            if (Convert.ToInt32(dataState[0]) > 0)
                matchFoundTimerTMP.text = Convert.ToInt32(dataState[0]).ToString();
            else
                GameManager.Instance.SceneController.MultiplayerScene = true;
        }
    }

    private void CheckLobbyState(object sender, EventArgs e)
    {
        if (CurrentLobbyState == LobbyState.CONNECTING)
        {
            connectingToServerObj.SetActive(true);
            lobbyParentObj.SetActive(true);
        }
        else if (CurrentLobbyState == LobbyState.LOBBY)
        {
            currentTimeFind = 0f;
            lobbyObj.SetActive(true);
            connectingToServerObj.SetActive(false);
            findingMatchObj.SetActive(false);
            lobbyBackBtn.interactable = true;
            findMatchBtn.interactable = true;
        }
        else if (CurrentLobbyState == LobbyState.FINDING)
        {
            lobbyBackBtn.interactable = false;
            findingMatchObj.SetActive(true);
            lobbyObj.SetActive(false);

            findingMatchBackBtn.interactable = true;
        }
        else if (CurrentLobbyState == LobbyState.FOUND)
        {
            findingMatchBackBtn.interactable = false;

            findingMatchObj.SetActive(false);
            matchFoundObj.SetActive(true);
        }
    }

    #region OVERRIDE FUNCTIONS

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        CurrentLobbyState = LobbyState.LOBBY;
        
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            GameManager.Instance.ErrorControl.ShowError("You have been disconnected from the server!" +
                " Please try again later. Error Code: " + cause, () =>
                {
                    connectingToServerObj.SetActive(false);
                    lobbyObj.SetActive(false);
                    findingMatchObj.SetActive(false);
                    matchFoundObj.SetActive(false);
                    lobbyParentObj.SetActive(false);
                });
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        CurrentLobbyState = LobbyState.FINDING;
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.IsMasterClient)
            playerList.Add(PhotonNetwork.LocalPlayer);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("player entered");
            
            if (!playerList.Contains(newPlayer))
                playerList.Add(newPlayer);

            Debug.Log(playerList.Count);

            if (playerList.Count >= 6)
            {
                int status = (int)LobbyState.FOUND;

                object[] data = new object[]
                {
                status
                };

                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.Others
                };
                SendOptions sendOptions = new SendOptions
                {
                    Reliability = true
                };
                PhotonNetwork.RaiseEvent(19, data, raiseEventOptions, sendOptions);
                CurrentLobbyState = LobbyState.FOUND;

                StartCoroutine(MatchFoundTimerCountdown());
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (PhotonNetwork.IsMasterClient)
        {
            if (playerList.Contains(otherPlayer))
                playerList.Remove(otherPlayer);
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //  Create room when joining room failed
        CreateRoom(currentMapCode, 6);
    }

    #endregion

    #region NON MULTIPLAYER FUNCTIONS

    private void FindMatchTimer()
    {
        string minutes, seconds;
        if (CurrentLobbyState == LobbyState.FINDING)
        {
            currentTimeFind += Time.deltaTime;
            seconds = (currentTimeFind % 60).ToString("00");
            minutes = Mathf.Floor(currentTimeFind / 60).ToString("00");
            findTimerTMP.text = minutes + " : " + seconds;

            return;
        }
    }

    #endregion

    #region MULTIPLAYER FUNCTIONS

    private IEnumerator DisconnectToServer(Action action)
    {
        PhotonNetwork.Disconnect();

        while (PhotonNetwork.IsConnected) yield return null;

        action?.Invoke();
    }

    public void CreateRoom(string mapCode, byte maxPlayers)
    {
        //  This is for creating room
        //  Custom room properties using hashtable
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable()
        { { "MAP", mapCode } };
        string[] lobbyProperties = { "MAP" };

        //  This is for setting up room properties provided
        //  by PUN2
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;
        roomOptions.IsVisible = true;
        roomOptions.CustomRoomPropertiesForLobby = lobbyProperties;
        roomOptions.CustomRoomProperties = customRoomProperties;
        roomOptions.CleanupCacheOnLeave = true;

        PhotonNetwork.CreateRoom(null, roomOptions, null);
    }

    IEnumerator MatchFoundTimerCountdown()
    {
        int time = 3;

        object[] data = new object[]
        {
            time
        };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };
        SendOptions sendOptions = new SendOptions
        {
            Reliability = true
        };

        while (time > 0f)
        {
            matchFoundTimerTMP.text = time.ToString();

            data[0] = time;

            PhotonNetwork.RaiseEvent(20, data, raiseEventOptions, sendOptions);

            yield return new WaitForSeconds(1f);

            time -= 1;

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        data[0] = 0;

        PhotonNetwork.RaiseEvent(20, data, raiseEventOptions, sendOptions);

        switch (currentMapCode)
        {
            case "001":
                PhotonNetwork.LoadLevel("KarerangTalonMultiplayer");
                GameManager.Instance.SceneController.MultiplayerScene = true;
                break;
            default:
                PhotonNetwork.LoadLevel("KarerangTalonMultiplayer");
                GameManager.Instance.SceneController.MultiplayerScene = true;
                break;
        }
    }

    #endregion

    #region BUTTON

    public void MultiplayerConnect()
    {
        CurrentLobbyState = LobbyState.CONNECTING;

        PhotonNetwork.ConnectUsingSettings();
    }

    public void CancelFindMatch()
    {
        findingMatchBackBtn.interactable = false;

        if (playerList.Count > 0)
            playerList.Clear();

        StartCoroutine(DisconnectToServer(() =>
        {
            PhotonNetwork.ConnectUsingSettings();
        }));
    }

    public void FindMatchButton()
    {
        findMatchBtn.interactable = false;
        lobbyBackBtn.interactable = false;

        ExitGames.Client.Photon.Hashtable customFilterMap = new ExitGames.Client.Photon.Hashtable { { "MAP", currentMapCode } };
        PhotonNetwork.JoinRandomRoom(customFilterMap, 6);
        CurrentLobbyState = LobbyState.FINDING;
    }

    public void CancelMultiplayer()
    {
        lobbyBackBtn.interactable = false;

        if (playerList.Count > 0)
            playerList.Clear();

        StartCoroutine(DisconnectToServer(() =>
        {
            lobbyParentObj.SetActive(false);
            lobbyParentObj.SetActive(false);
        }));
    }

    #endregion
}
