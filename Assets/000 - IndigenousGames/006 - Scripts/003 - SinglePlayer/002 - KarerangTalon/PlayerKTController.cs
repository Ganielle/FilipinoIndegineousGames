using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKTController : MonoBehaviour
{
    [SerializeField] private KarerangTalonCore core;
    [SerializeField] private Vector3 spawnPos;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private bool canAddStage;
    [ReadOnly][SerializeField] private bool canCheckDoneRound;

    private void OnEnable()
    {
        canCheckDoneRound = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("success") && canAddStage)
        {
            canAddStage = false;
            core.currentStage++;
        }


        if (other.gameObject.CompareTag("finishline"))
        {
            core.isDonePlayer = true;
            core.CheckIfDoneRound();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall") && canCheckDoneRound)
        {
            canCheckDoneRound = false;
            transform.position = spawnPos;
            core.CheckIfDoneRound();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            core.isJumping = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            canAddStage = true;
            core.isJumping = true;
        }
    }
}
