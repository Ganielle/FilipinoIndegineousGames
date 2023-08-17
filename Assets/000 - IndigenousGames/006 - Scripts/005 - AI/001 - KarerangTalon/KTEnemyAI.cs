using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static KarerangTalonCore;

public class KTEnemyAI : MonoBehaviour
{
    [SerializeField] private KarerangTalonCore core;
    [SerializeField] private Vector3 spawnPos;
    [SerializeField] private Rigidbody enemyRB;
    [SerializeField] private float stageOneJumpStrength;
    [SerializeField] private float stageTwoJumpStrength;
    [SerializeField] private float stageThreeJumpStrength;

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

        if (other.gameObject.CompareTag("enemyjump")) Jump();

        if (other.gameObject.CompareTag("finishline"))
        {
            core.isDoneEnemy = true;
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

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            canAddStage = true;
        }
    }

    private void Jump()
    {
        if (core.CurrentGameStateKT != GameStateKT.GAME) return;

        float jumpStrength;
        int rand = UnityEngine.Random.Range(0, 100);

        if (rand < 50)
        {
            if (core.currentStage == 1)
                jumpStrength = 2;
            else if (core.currentStage == 2)
                jumpStrength = 3;
            else
                jumpStrength = 4;
        }
        else
        {
            if (core.currentStage == 1)
                jumpStrength = stageOneJumpStrength + 2;
            else if (core.currentStage == 2)
                jumpStrength = stageTwoJumpStrength + 2;
            else
                jumpStrength = stageThreeJumpStrength + 2;
        }

        enemyRB.velocity = new Vector3(0, jumpStrength, enemyRB.velocity.z);
    }
}
