using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgawanBaseKeeperBoundry : MonoBehaviour
{
    [SerializeField] private AgawanBaseCOre core;
    [SerializeField] private AgawanBaseCOre.Team currentTeam;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] public bool isPlayerEnemy;
    [ReadOnly][SerializeField] public string enemyNames;
    [ReadOnly][SerializeField] public List<Transform> enemiesList;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject.CompareTag("AI") && other.gameObject.name == enemyNames) || 
            (other.gameObject.CompareTag("Player") && isPlayerEnemy))
        {
            enemiesList.Add(other.gameObject.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && isPlayerEnemy)
            enemiesList.Remove(other.gameObject.transform);

        if (other.gameObject.CompareTag("AI") && other.gameObject.name == enemyNames)
            enemiesList.Remove(other.gameObject.transform);
    }

    private void Awake()
    {
        enemyNames = currentTeam == AgawanBaseCOre.Team.BLUE ? "Red" : "Blue";
        StartCoroutine(SelectPlayerEnemy());
    }

    IEnumerator SelectPlayerEnemy()
    {
        while (core.CurrentTeam == AgawanBaseCOre.Team.NONE) yield return null;

        isPlayerEnemy = core.CurrentTeam == currentTeam ? false : true;
    }

    public void RemovePlayerInBoundary(Transform objTF)
    {
        if (enemiesList.Contains(objTF))
            enemiesList.Remove(objTF);
    }

    public void AddPlayerInBoundary(Transform objTF)
    {
        if (enemiesList.Contains(objTF)) return;

        enemiesList.Add(objTF);
    }
}
