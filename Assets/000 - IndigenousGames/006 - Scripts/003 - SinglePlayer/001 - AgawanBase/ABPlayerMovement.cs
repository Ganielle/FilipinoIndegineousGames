using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABPlayerMovement : MonoBehaviour
{
    [SerializeField] private AgawanBaseCOre core;
    [SerializeField] private Rigidbody playerRB;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private AgawanBaseKeeperBoundry redKeeperBoundry;
    [SerializeField] private AgawanBaseKeeperBoundry blueKeeperBoundry;

    [Header("MOVEMENT")]
    [SerializeField] private float speed;

    [Header("CONTROLLER")]
    [SerializeField] private AgawanBaseGameController controller;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private string gatekeeperEnemyName;
    [ReadOnly][SerializeField] private Vector2 movement;
    [ReadOnly][SerializeField] private Vector3 move;
    [ReadOnly][SerializeField] private float currentSpeed;
    [ReadOnly][SerializeField] private bool isTagged;
    [ReadOnly][SerializeField] private string teamName;

    //  =====================================

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Flag"))
        {
            if (core.CurrentTeam == AgawanBaseCOre.Team.BLUE)
                core.AddBlueTeamScore();
            else
                core.AddRedTeamScore();

            core.BrinPlayerToSpawnPoint();
        }

        if (other.gameObject.CompareTag("Coin"))
        {
            other.GetComponent<CoinController>().AddCoin();
        }

        if (other.gameObject.CompareTag("Powerup"))
        {
            other.GetComponent<PowerupController>().StartPowerup();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("AI") && collision.gameObject.name == gatekeeperEnemyName)
        {
            if (gatekeeperEnemyName == "RedGatekeeper")
                redKeeperBoundry.enemiesList.Remove(transform);
            else
                blueKeeperBoundry.enemiesList.Remove(transform);

            //core.BrinPlayerToSpawnPoint();

            isTagged = true;

            if (core.CurrentTeam == AgawanBaseCOre.Team.BLUE)
                core.AddBlueTaggedCharacters(gameObject);
            else
                core.AddRedTaggedCharacters(gameObject);
        }

        if (collision.gameObject.CompareTag("AI") && collision.gameObject.name == teamName)
        {

            isTagged = false;

            Debug.Log("hello");

            if (gatekeeperEnemyName == "RedGatekeeper")
            {
                core.taggedBlueList.Remove(gameObject);
            }
            else
            {
                core.taggedBlueList.Remove(gameObject);
            }

            //core.BrinPlayerToSpawnPoint();

            if (gatekeeperEnemyName == "RedGatekeeper")
            {
                redKeeperBoundry.AddPlayerInBoundary(transform);
            }
            else
            {
                blueKeeperBoundry.AddPlayerInBoundary(transform);
            }
        }
    }

    //  =====================================

    private void Awake()
    {
        StartCoroutine(SelectEnemyGatekeeper());
        currentSpeed = speed;
    }

    private void Update()
    {
        MovePlayer();

        if (core.CurrentTeam == AgawanBaseCOre.Team.BLUE) teamName = "Blue";
        else teamName = "Red";
    }

    IEnumerator SelectEnemyGatekeeper()
    {
        while (core.CurrentTeam == AgawanBaseCOre.Team.NONE) yield return null;

        gatekeeperEnemyName = core.CurrentTeam == AgawanBaseCOre.Team.BLUE ? "RedGatekeeper" : "BlueGatekeeper";
    }

    public IEnumerator SpeedBoost()
    {
        currentSpeed = speed + 10;

        float currentTime = 5f;

        while(currentSpeed > 0)
        {
            currentTime -= Time.deltaTime;
            yield return null;
        }

        currentSpeed = speed;
    }

    private void MovePlayer()
    {
        if (isTagged)
        {
            characterController.Move(Vector3.zero);
            return;
        }

        movement = controller.GetPlayerMovement();
        move = new Vector3(movement.x, 0f, movement.y);
        move = GameManager.Instance.MainCamera.transform.forward * move.z +
            GameManager.Instance.MainCamera.transform.right * move.x;
        move.y = 0f;
        //playerRB.velocity = new Vector3(move.x * speed * Time.fixedDeltaTime, 0f, move.z * speed * Time.fixedDeltaTime);
        characterController.Move(move * Time.fixedDeltaTime * speed);

        //if (move != Vector3.zero)
        //{
        //    playerRB.transform.forward = move;
        //}
    }
}
