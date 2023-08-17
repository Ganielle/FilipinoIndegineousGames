using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AgawanBaseCOre;

public class AIAgawanBaseController : MonoBehaviour
{
    [SerializeField] private AgawanBaseCOre core;
    [SerializeField] private AgawanBaseCOre.Team team;
    [SerializeField] private AgawanBaseKeeperBoundry boundry;
    [SerializeField] private Animator playerAnim;
    [SerializeField] private GameObject indicator;

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
    [ReadOnly][SerializeField] private string teamName;
    [ReadOnly][SerializeField] private bool isPlayerTeammate;
    [ReadOnly] public bool isTagged;
    [ReadOnly][SerializeField] private bool isRescue;

    //  =====================================

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("AI") && collision.gameObject.name == gatekeeperName)
        {
            //aiNavMesh.isStopped = true;
            //aiNavMesh.enabled = false;

            //transform.position = team == AgawanBaseCOre.Team.BLUE ? blueSpawnPoint.position : redSpawnPoint.position;

            //waypointIndex = Random.Range(0, waypoints.Count);
            //selectedWaypoint = waypoints[waypointIndex];

            //flagDecision = 100;

            //aiNavMesh.enabled = true;

            boundry.RemovePlayerInBoundary(gameObject.transform);

            if (teamName == "Blue") core.AddBlueTaggedCharacters(gameObject);
            else core.AddRedTaggedCharacters(gameObject);

            isTagged = true;

            aiNavMesh.isStopped = false;
        }

        if (collision.gameObject.CompareTag("AI") && collision.gameObject.name == teamName && isTagged)
        {
            waypointIndex = UnityEngine.Random.Range(0, waypoints.Count);
            selectedWaypoint = waypoints[waypointIndex];

            flagDecision = 100;

            isTagged = false;

            aiNavMesh.enabled = true;

            if (teamName == "Blue")
                core.taggedBlueList.Remove(gameObject);
            else
                core.taggedRedList.Remove(gameObject);

            boundry.AddPlayerInBoundary(transform);
        }

        if (isPlayerTeammate && collision.gameObject.CompareTag("Player") && isTagged)
        {
            waypointIndex = UnityEngine.Random.Range(0, waypoints.Count);
            selectedWaypoint = waypoints[waypointIndex];

            flagDecision = 100;

            isTagged = false;

            aiNavMesh.enabled = true;

            if (teamName == "Blue")
                core.taggedBlueList.Remove(gameObject);
            else
                core.taggedRedList.Remove(gameObject);

            boundry.AddPlayerInBoundary(transform);
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

            waypointIndex = UnityEngine.Random.Range(0, waypoints.Count);
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
        teamName = team == Team.BLUE ? "Blue" : "Red";
        flagDecision = 100;
    }


    private void Update()
    {
        isPlayerTeammate = team == core.CurrentTeam ? true : false;

        CheckTeamIndicator();
        RunTowardsWaypoint();
    }

    private void CheckTeamIndicator()
    {
        if (!isPlayerTeammate)
        {
            indicator.SetActive(false);
            return;
        }

        indicator.SetActive(true);
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

        if (isTagged)
        {
            playerAnim.SetBool("idle", true);
            playerAnim.SetBool("run", false);
            aiNavMesh.isStopped = true;
            return;
        }

        aiNavMesh.isStopped = false;

        playerAnim.SetBool("run", true);
        playerAnim.SetBool("idle", false);

        if (aiNavMesh.remainingDistance <= aiNavMesh.stoppingDistance)
        {
            flagDecision = UnityEngine.Random.Range(0, 100);

            waypointIndex = UnityEngine.Random.Range(0, waypoints.Count);

            if (team == Team.BLUE)
            {
                if (core.taggedBlueList.Count > 0)
                {
                    float random = UnityEngine.Random.Range(0, 101);

                    if (random > 65)
                    {
                        isRescue = true;
                        selectedWaypoint = core.taggedBlueList[0].transform;
                    }
                    else
                    {
                        if (flagDecision <= 25)
                            selectedWaypoint = team == AgawanBaseCOre.Team.BLUE ? redFlagTF : blueFlagTF;
                        else
                            selectedWaypoint = waypoints[waypointIndex];
                    }
                }
                else
                {
                    if (flagDecision <= 25)
                        selectedWaypoint = team == AgawanBaseCOre.Team.BLUE ? redFlagTF : blueFlagTF;
                    else
                        selectedWaypoint = waypoints[waypointIndex];
                }
            }
            else
            {
                if (core.taggedRedList.Count > 0)
                {
                    float random = UnityEngine.Random.Range(0, 101);

                    if (random > 65)
                    {
                        isRescue = true;
                        selectedWaypoint = core.taggedRedList[0].transform;
                    }
                    else
                    {
                        if (flagDecision <= 25)
                            selectedWaypoint = team == AgawanBaseCOre.Team.BLUE ? redFlagTF : blueFlagTF;
                        else
                            selectedWaypoint = waypoints[waypointIndex];
                    }
                }
                else
                {
                    if (flagDecision <= 25)
                        selectedWaypoint = team == AgawanBaseCOre.Team.BLUE ? redFlagTF : blueFlagTF;
                    else
                        selectedWaypoint = waypoints[waypointIndex];
                }
            }

            aiNavMesh.SetDestination(selectedWaypoint.position);
        }

    }
}
