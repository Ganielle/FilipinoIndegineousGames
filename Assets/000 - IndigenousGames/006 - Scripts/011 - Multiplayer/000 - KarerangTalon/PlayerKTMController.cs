using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static KarerangTalonCore;

public class PlayerKTMController : MonoBehaviour
{
    [SerializeField] private List<GameObject> costumeList;
    [SerializeField] private List<Animator> playerAnimatorList;
    [SerializeField] private Rigidbody playerRB;
    [SerializeField] private float moveSpeed;

    [Header("DEBUGGER")]
    [ReadOnly] public KarerangTalonMultiplayerCore core;
    [ReadOnly][SerializeField] private Animator playerAnimator;
    [ReadOnly][SerializeField] private bool canAddStage;
    [ReadOnly][SerializeField] private bool canCheckDoneRound;
    [ReadOnly][SerializeField] private bool isJumping;
    [ReadOnly][SerializeField] private bool canRun;



    private void OnTriggerEnter(Collider other)
    {
        if (core != null)
        {
            if (other.gameObject.CompareTag("success") && canAddStage)
            {
                canAddStage = false;
                core.currentStage++;
            }


            if (other.gameObject.CompareTag("finishline"))
            {
                core.FinishPlayer();
                core.TellServerToNextPlayer();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (core != null)
        {
            if (collision.gameObject.CompareTag("wall") && canCheckDoneRound)
            {
                canRun = false;
                canCheckDoneRound = false;
                transform.position = core.spawnPoint;
                //core.CheckIfDoneRound();
                core.TellServerToNextPlayer();
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (core != null)
        {
            if (collision.gameObject.CompareTag("ground"))
            {
                core.isJumping = false;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (core != null)
        {
            if (collision.gameObject.CompareTag("ground"))
            {
                canAddStage = true;
                core.isJumping = true;
            }
        }
    }

    private void OnEnable()
    {
        if (core != null)
        {
            canRun = true;
            canCheckDoneRound = true;
            core.OnKTMultiplayerStateChange += StateEvent;
        }
    }

    private void OnDisable()
    {
        if (core != null)
        {
            //canCheckDoneRound = true;
            core.OnKTMultiplayerStateChange -= StateEvent;
        }
    }

    private void StateEvent(object sender, EventArgs e)
    {

    }

    private void Update()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (core == null) return;

        if (!canRun)
        {
            playerRB.velocity = Vector3.zero;
            playerAnimator.SetBool("run", false);
            playerAnimator.SetBool("jump", false);
            playerAnimator.SetBool("idle", true);
            return;
        }

        //if (core.TeamTurn != core.CurrentKTTeam)
        //{
        //    playerRB.velocity = Vector3.zero;
        //    playerAnimator.SetBool("run", false);
        //    playerAnimator.SetBool("jump", false);
        //    playerAnimator.SetBool("idle", true);
        //    return;
        //}

        if (core.CurrentKTState != KarerangTalonMultiplayerCore.KTMultiplayerState.GAME)
        {
            playerRB.velocity = Vector3.zero;
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

    public void JumpPlayer(float jumpStrength)
    {
        playerRB.velocity = new Vector3(0, jumpStrength, 0f);
    }

    public void ActivateCostume(string costumeName)
    {
        GameObject costume = costumeList.Find(e => e.name == costumeName);

        if (costume != null)
        {
            playerAnimator = playerAnimatorList[costumeList.FindIndex(e => e.name == costumeName)];
            costume.SetActive(true);
        }

        costumeList[0].SetActive(true);
        playerAnimator = playerAnimatorList[0];
    }


}
