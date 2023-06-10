using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAgawanBaseGateKeeper : MonoBehaviour
{
    [SerializeField] private AgawanBaseCOre core;
    [SerializeField] private AgawanBaseKeeperBoundry boundry;

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
            aiNavMesh.isStopped = true;
            return;
        }

        aiNavMesh.isStopped = false;

        if (boundry.enemiesList.Count <= 0)
            aiNavMesh.SetDestination(gatekeeperPosition.position);
        else
            aiNavMesh.SetDestination(boundry.enemiesList[0].position);
    }
}
