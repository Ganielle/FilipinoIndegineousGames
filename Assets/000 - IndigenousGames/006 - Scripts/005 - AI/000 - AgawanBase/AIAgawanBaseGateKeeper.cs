using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAgawanBaseGateKeeper : MonoBehaviour
{
    [SerializeField] private AgawanBaseCOre core;
    [SerializeField] private AgawanBaseKeeperBoundry boundry;
    [SerializeField] private Animator playerAnim;

    [Header("ARTIFICIAL INTELIGENCE")]
    [SerializeField] private NavMeshAgent aiNavMesh;
    [SerializeField] private Transform gatekeeperPosition;

    private void Update()
    {
        MoveToDestination();
    }

    private void MoveToDestination()
    {
        if (core.CurrentMatchState != AgawanBaseCOre.MatchState.PLAY)
        {
            playerAnim.SetBool("idle", true);
            playerAnim.SetBool("run", false);

            aiNavMesh.isStopped = true;
            return;
        }

        aiNavMesh.isStopped = false;


        if (boundry.enemiesList.Count <= 0)
        {
            aiNavMesh.SetDestination(gatekeeperPosition.position);

            if (aiNavMesh.pathStatus == NavMeshPathStatus.PathComplete)
            {
                playerAnim.SetBool("run", false);
                playerAnim.SetBool("idle", true);
            }
            else
            {
                playerAnim.SetBool("run", true);
                playerAnim.SetBool("idle", false);
            }
        }
        else
        {
            aiNavMesh.SetDestination(boundry.enemiesList[0].position);

            playerAnim.SetBool("run", true);
            playerAnim.SetBool("idle", false);
        }
    }
}
