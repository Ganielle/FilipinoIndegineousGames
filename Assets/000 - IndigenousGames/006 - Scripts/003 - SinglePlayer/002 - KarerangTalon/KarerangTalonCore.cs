using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static AgawanBaseCOre;

public class KarerangTalonCore : MonoBehaviour
{
    public enum GameStateKT
    {
        NONE,
        FLIPCOIN,
        COUNTDOWN,
        GAME,
        FINISH
    }
    private event EventHandler GameStateKTChange;
    public event EventHandler OnGameStateKTChange
    {
        add
        {
            if (GameStateKTChange == null || !GameStateKTChange.GetInvocationList().Contains(value))
                GameStateKTChange += value;
        }
        remove { GameStateKTChange -= value; }
    }
    public GameStateKT CurrentGameStateKT
    {
        get => currentGameStateKT;
        set
        {
            currentGameStateKT = value;
            GameStateKTChange?.Invoke(this, EventArgs.Empty);
        }
    }

    //  =================================

    [SerializeField] private PlayeData playerData;
    [SerializeField] private AudioClip bgMusic;

    [Header("PLAYERS")]
    [SerializeField] private float stageOneJumpStrength;
    [SerializeField] private float stageTwoJumpStrength;
    [SerializeField] private float stageThreeJumpStrength;
    [SerializeField] private Rigidbody playerRB;

    [Header("ENEMY")]
    [SerializeField] private Rigidbody enemyRB;
    [SerializeField] private Animator enemyAnimator;

    [Header("CINEMACHINE")]
    [SerializeField] private GameObject playerVcam;
    [SerializeField] private GameObject enemyVcam;

    [Header("COUNTDOWN")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownTMP;

    [Header("GAMEPLAY")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject jumpBtnGO;
    [SerializeField] private TextMeshProUGUI timerTMP;
    [SerializeField] private GameObject playerTurnNowObj;
    [SerializeField] private GameObject AITurnNowObj;
    [SerializeField] private List<GameObject> playerCharacterList;

    [Header("JUMP")]
    [SerializeField] private float speedMoveGauge;
    [SerializeField] private float minGauge;
    [SerializeField] private float maxGauge;
    [SerializeField] private Slider gaugeSlider;

    [Header("MOVE")]
    [SerializeField] private float moveSpeed;

    [Header("FINISH STATUS")]
    [SerializeField] private TextMeshProUGUI finishStatusTMP;
    [SerializeField] private TextMeshProUGUI earningStatusTMP;
    [SerializeField] private GameObject finishStatusObj;

    [Header("PAUSE MENU")]
    [SerializeField] private GameObject pauseMenuObj;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private GameObject characterObj;
    [ReadOnly][SerializeField] private Animator playerAnimator;
    [ReadOnly][SerializeField] private GameStateKT currentGameStateKT;
    [ReadOnly][SerializeField] public int currentStage;
    [ReadOnly][SerializeField] private bool isPlayerTurn;
    [ReadOnly][SerializeField] public bool isDonePlayer;
    [ReadOnly][SerializeField] public bool isDoneEnemy;
    [ReadOnly][SerializeField] private float playerTime;
    [ReadOnly][SerializeField] private float enemyTime;

    [Header("JUMP DEBUGGER")]
    [ReadOnly][SerializeField] private bool holdJump;
    [ReadOnly][SerializeField] public bool isJumping;
    [ReadOnly][SerializeField] private float currentSpeedMoveGauge;
    [ReadOnly][SerializeField] private bool moveForward;

    //  ===========================

    Coroutine countdownStart;

    float minutes, seconds;

    //  ===========================

    private void OnEnable()
    {
        GameManager.Instance.SceneController.AddActionLoadinList(ChangeBGMusic());
        GameManager.Instance.SceneController.AddActionLoadinList(Initialize());
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void OnDisable()
    {
        if (countdownStart != null) StopCoroutine(countdownStart);
    }

    private void Update()
    {
        MoveGauge();
        MovePlayer();
        MoveEnemy();
        StartPlayerTimer();
        StartEnemyTimer();
    }

    #region INITIALIZATION

    private IEnumerator ChangeBGMusic()
    {
        GameManager.Instance.AudioSystem.SetBGMusic(bgMusic);

        yield return null;
    }

    private IEnumerator Initialize()
    {
        timerTMP.text = string.Format("{0:00}:{1:00}", 0f, 0f);

        int rand = UnityEngine.Random.Range(0, 2);

        if (rand == 0)
        {
            isPlayerTurn = true;

            playerRB.gameObject.SetActive(true);
            jumpBtnGO.SetActive(true);
            playerVcam.SetActive(true);
        }
        else
        {
            isPlayerTurn = false;

            jumpBtnGO.SetActive(false);

            enemyVcam.SetActive(true);
            enemyRB.gameObject.SetActive(true);
        }

        currentSpeedMoveGauge = speedMoveGauge;
        currentStage = 1;

        characterObj = playerCharacterList.Find(e => e.name == playerData.EquippedCharacter);
        characterObj.SetActive(true);
        playerAnimator = characterObj.GetComponent<CharacterEquipController>().characterAnimator;

        countdownStart = StartCoroutine(StartCountdownAtStart());

        yield return null;
    }

    #endregion

    #region GAMEPLAY

    private IEnumerator StartCountdownAtStart()
    {
        while (!GameManager.Instance.SceneController.DoneLoading) yield return null;

        if (isPlayerTurn)
            playerTurnNowObj.SetActive(true);
        else
            AITurnNowObj.SetActive(true);

        CurrentGameStateKT = GameStateKT.COUNTDOWN;

        float time = 3f;
        countdownTMP.text = time.ToString("n0");

        countdownPanel.SetActive(true);

        while (time > 0f)
        {
            if (time > 1f)
                countdownTMP.text = time.ToString("n0");
            else
                countdownTMP.text = "GO!";

            time -= Time.deltaTime;

            yield return null;
        }

        countdownStart = null;

        gameplayPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(1f);

        if (isPlayerTurn)
            playerTurnNowObj.SetActive(false);
        else
            AITurnNowObj.SetActive(false);

        CurrentGameStateKT = GameStateKT.GAME;

        countdownPanel.SetActive(false);
    }

    private void StartPlayerTimer()
    {
        if (isPlayerTurn)
        {
            minutes = Mathf.FloorToInt(playerTime / 60);
            seconds = Mathf.FloorToInt(playerTime % 60);

            timerTMP.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (CurrentGameStateKT == GameStateKT.GAME)
                playerTime += Time.deltaTime;
        }
    }

    private void StartEnemyTimer()
    {
        if (!isPlayerTurn)
        {
            minutes = Mathf.FloorToInt(enemyTime / 60);
            seconds = Mathf.FloorToInt(enemyTime % 60);

            timerTMP.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (CurrentGameStateKT == GameStateKT.GAME)
                enemyTime += Time.deltaTime;
        }
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

    public void CheckIfDoneRound()
    {
        if (isDoneEnemy && isDonePlayer)
        {
            CurrentGameStateKT = GameStateKT.FINISH;

            if (playerTime == enemyTime || enemyTime == playerTime)
            {
                playerData.Credits += 50;
                finishStatusTMP.text = "THE MATCH IS DRAW!";
            }
            else
            {
                if (playerTime < enemyTime)
                {
                    playerData.Credits += 25;
                    finishStatusTMP.text = "BLUE TEAM WINS";
                    earningStatusTMP.text = "YOU EARNED 25 COIN";
                }
                else
                {
                    playerData.Credits += 50;
                    finishStatusTMP.text = "RED TEAM WINS";
                    earningStatusTMP.text = "YOU EARNED 50 COIN";
                }
            }


            earningStatusTMP.text = "YOU EARNED 0 COIN";

            finishStatusObj.SetActive(true);
            gameplayPanel.SetActive(false);
            pauseMenuObj.SetActive(false);
        }
        else
        {
            if (isPlayerTurn)
            {
                if (!isDoneEnemy)
                {
                    isPlayerTurn = false;

                    countdownStart = StartCoroutine(StartCountdownAtStart());

                    currentStage = 1;

                    #region PLAYER

                    jumpBtnGO.SetActive(false);
                    gaugeSlider.gameObject.SetActive(false);
                    playerVcam.SetActive(false);
                    playerRB.gameObject.SetActive(false);

                    #endregion

                    #region ENEMY

                    enemyRB.gameObject.SetActive(true);
                    enemyVcam.SetActive(true);

                    #endregion
                }
                else
                {
                    isPlayerTurn = true;

                    countdownStart = StartCoroutine(StartCountdownAtStart());

                    currentStage = 1;

                    #region PLAYER

                    jumpBtnGO.SetActive(true);
                    playerVcam.SetActive(true);
                    playerRB.gameObject.SetActive(false);
                    playerRB.gameObject.SetActive(true);

                    #endregion

                    #region ENEMY

                    enemyVcam.SetActive(false);
                    enemyRB.gameObject.SetActive(false);

                    #endregion
                }
            }
            else
            {
                if (!isDonePlayer)
                {
                    isPlayerTurn = true;

                    countdownStart = StartCoroutine(StartCountdownAtStart());

                    currentStage = 1;

                    #region PLAYER

                    jumpBtnGO.SetActive(true);
                    playerVcam.SetActive(true);
                    playerRB.gameObject.SetActive(false);
                    playerRB.gameObject.SetActive(true);

                    #endregion

                    #region ENEMY

                    enemyVcam.SetActive(false);
                    enemyRB.gameObject.SetActive(false);

                    #endregion
                }
                else
                {
                    isPlayerTurn = false;

                    countdownStart = StartCoroutine(StartCountdownAtStart());

                    currentStage = 1;

                    #region PLAYER

                    jumpBtnGO.SetActive(false);
                    gaugeSlider.gameObject.SetActive(false);
                    playerVcam.SetActive(false);
                    playerRB.gameObject.SetActive(false);

                    #endregion

                    #region ENEMY

                    enemyRB.gameObject.SetActive(false);
                    enemyRB.gameObject.SetActive(true);
                    enemyVcam.SetActive(true);

                    #endregion
                }
            }
        }
    }

    #region PLAYER

    private void MovePlayer()
    {
        if (!isPlayerTurn)
        {
            playerAnimator.SetBool("run", false);
            playerAnimator.SetBool("jump", false);
            playerAnimator.SetBool("idle", true);
            return;
        }

        if (CurrentGameStateKT != GameStateKT.GAME)
        {
            playerAnimator.SetBool("run", false);
            playerAnimator.SetBool("jump", false);
            playerAnimator.SetBool("idle", true);
            return;
        }

        playerRB.velocity = new Vector3(0f, playerRB.velocity.y, 1 * moveSpeed);

        if (playerRB.velocity.y != 0)
        {
            playerAnimator.SetBool("run", false);
            playerAnimator.SetBool("jump", true);
            playerAnimator.SetBool("idle", false);
        }
        else
        {
            playerAnimator.SetBool("run", true);
            playerAnimator.SetBool("jump", false);
            playerAnimator.SetBool("idle", false);
        }
    }

    private void JumpPlayer()
    {
        if (CurrentGameStateKT != GameStateKT.GAME) return;

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

        playerRB.velocity = new Vector3(0, jumpStrength, 0f);
        gaugeSlider.gameObject.SetActive(false);
    }

    private void MoveEnemy()
    {
        if (isPlayerTurn)
        {
            enemyAnimator.SetBool("run", false);
            enemyAnimator.SetBool("jump", false);
            enemyAnimator.SetBool("idle", true);
            return;
        }

        if (CurrentGameStateKT != GameStateKT.GAME)
        {
            enemyAnimator.SetBool("run", false);
            enemyAnimator.SetBool("jump", false);
            enemyAnimator.SetBool("idle", true);
            return;
        }

        enemyRB.velocity = new Vector3(0f, enemyRB.velocity.y, 1 * moveSpeed);

        if (enemyRB.velocity.y != 0)
        {
            enemyAnimator.SetBool("run", false);
            enemyAnimator.SetBool("jump", true);
            enemyAnimator.SetBool("idle", false);
        }
        else
        {
            enemyAnimator.SetBool("run", true);
            enemyAnimator.SetBool("jump", false);
            enemyAnimator.SetBool("idle", false);
        }
    }

    #region BUTTON

    public void JumpPressed()
    {
        if (CurrentGameStateKT != GameStateKT.GAME) return;

        if (!isPlayerTurn) return;

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

    #endregion

    #endregion

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
