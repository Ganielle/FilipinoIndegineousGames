using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABPlayerMovement : MonoBehaviour
{
    [SerializeField] private AgawanBaseCOre core;
    [SerializeField] private Rigidbody playerRB;
    [SerializeField] private CharacterController characterController;

    [Header("MOVEMENT")]
    [SerializeField] private float speed;

    [Header("CONTROLLER")]
    [SerializeField] private AgawanBaseGameController controller;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private Vector2 movement;
    [ReadOnly][SerializeField] private Vector3 move;

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
    }

    private void Update()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
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
