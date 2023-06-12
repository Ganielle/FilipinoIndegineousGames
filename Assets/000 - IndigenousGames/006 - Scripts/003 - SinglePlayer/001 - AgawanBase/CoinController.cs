using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    [SerializeField] private AgawanBaseCOre core;

    [Header("COIN")]
    [SerializeField] private float totalTimeBeforeRespawn;
    [SerializeField] private Collider coinCollider;
    [SerializeField] private MeshRenderer coinRenderer;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private float currentRestartTime;

    //  ===============================

    Coroutine respawnCoin;

    //  ===============================

    private void OnDisable()
    {
        if (respawnCoin != null) StopCoroutine(respawnCoin);
    }

    public void AddCoin()
    {
        core.CurrentCollectedCoin++;
        coinCollider.enabled = false;
        coinRenderer.enabled = false;

        core.ChangeCurrentCoin();

        respawnCoin = StartCoroutine(StartRespawnCoin());
    }

    IEnumerator StartRespawnCoin()
    {
        currentRestartTime = totalTimeBeforeRespawn;

        while (currentRestartTime > 0)
        {
            currentRestartTime -= Time.deltaTime;
            yield return null;
        }

        coinCollider.enabled = true;
        coinRenderer.enabled = true;

        respawnCoin = null;
    }
}
