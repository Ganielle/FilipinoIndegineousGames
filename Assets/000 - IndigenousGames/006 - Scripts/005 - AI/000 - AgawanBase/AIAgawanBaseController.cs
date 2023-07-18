using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AgawanBaseCOre;

public class AIAgawanBaseController : MonoBehaviour
{
    [SerializeField] private AgawanBaseCOre core;
    [SerializeField] private AgawanBaseCOre.Team team;
    [SerializeField] private Animator playerAnim;

    [Header("ARTIFICIAL INTELIGENCE")]
    [SerializeField] private NavMeshAgent aiNavMesh;
    [SerializeField] private Transform blueFlagTF;
    [SerializeField] private Transform redFlagTF;
    [SerializeField] private Transform blueSpawnPoint;
    [SerializeField] private Transform redSpawnPoint;
    [SerializeField] private List<Transform> waypoints;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private string gatekeeperName;
    [ReadOnly][SerializeField] private int waypointIndex;
    [ReadOnly][SerializeField] private int flagDecision;
    [ReadOnly][SerializeField] private Transform selectedWaypoint;

    //  =====================================

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("AI") && collision.gameObject.name == gatekeeperName)
        {
            aiNavMesh.isStopped = true;
            aiNavMesh.enabled = false;

            transform.position = team == AgawanBaseCOre.Team.BLUE ? blueSpawnPoint.position : redSpawnPoint.position;

            waypointIndex = Random.Range(0, waypoints.Count);
            selectedWaypoint = waypoints[waypointIndex];

            flagDecision = 100;

            aiNavMesh.enabled = true;

            aiNavMesh.isStopped = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Flag"))
        {

            aiNavMesh.isStopped = true;
            aiNavMesh.enabled = false;

            if (team == AgawanBaseCOre.Team.BLUE)
                core.AddBlueTeamScore();
            else
                core.AddRedTeamScore();

            transform.position = team == AgawanBaseCOre.Team.BLUE ? blueSpawnPoint.position : redSpawnPoint.position;

            waypointIndex = Random.Range(0, waypoints.Count);
            selectedWaypoint = waypoints[waypointIndex];


            flagDecision = 100;

            aiNavMesh.enabled = true;

            aiNavMesh.isStopped = false;
        }
    }

    //  =====================================

    private void Awake()
    {
        gatekeeperName = team == Team.BLUE ? "RedGatekeeper" : "BlueGatekeeper";
        flagDecision = 100;
    }


    private void Update()
    {
        RunTowardsWaypoint();
    }

    private void RunTowardsWaypoint()
    {
        if (core.CurrentMatchState != AgawanBaseCOre.MatchState.PLAY)
        {
            playerAnim.SetBool("idle", true);
            playerAnim.SetBool("run", false);
            aiNavMesh.isStopped = true;
            return;
        }

        aiNavMesh.isStopped = false;

        playerAnim.SetBool("run", true);
        playerAnim.SetBool("idle", false);

        if (aiNavMesh.remainingDistance <= aiNavMesh.stoppingDistance && flagDecision > 40)
        {
            flagDecision = Random.Range(0, 100);

            waypointIndex = Random.Range(0, waypoints.Count);


            if (flagDecision <= 40)
                selectedWaypoint = team == AgawanBaseCOre.Team.BLUE ? redFlagTF : blueFlagTF;
            else
                selectedWaypoint = waypoints[waypointIndex];

            aiNavMesh.SetDestination(selectedWaypoint.position);
        }

    }
}
