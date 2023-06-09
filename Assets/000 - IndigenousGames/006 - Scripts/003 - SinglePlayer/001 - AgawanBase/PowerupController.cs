using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerupController : MonoBehaviour
{
    [SerializeField] private AgawanBaseCOre core;
    [SerializeField] private GameObject powerupParentUIObj;

    [Header("POWERUP")]
    [SerializeField] private Collider powerupCollider;
    [SerializeField] private MeshRenderer powerupRenderer;

    [Header("QUESTIONS")]
    [SerializeField] private GameObject questionsParentUIObj;

    [Header("SPIN THE WHEEL")]
    [SerializeField] private GameObject wheelParentUIObj;
    [SerializeField] private GameObject spinWheelBtnObj;
    [SerializeField] private Button spinWheelBtn;
    [SerializeField] private Transform wheelTF;
    [SerializeField] private int numberOfItems;
    [SerializeField] private float rotateTimeMin;
    [SerializeField] private float rotateTimeMax;
    [SerializeField] private float numberCircleRotate;
    [SerializeField] private float minOffset;
    [SerializeField] private float maxOffset;
    [SerializeField] private AnimationCurve curve;

    [Header("DEBUGGER")]
    [ReadOnly][SerializeField] private float circle = 360.0f;
    [ReadOnly][SerializeField] private float selectedRotateTime;
    [ReadOnly][SerializeField] private float startAngle;
    [ReadOnly][SerializeField] private float selectedAngle;
    [ReadOnly][SerializeField] private float angleOfOneItem;
    [ReadOnly][SerializeField] private float currentAngle;
    [ReadOnly][SerializeField] private float currentTime;
    [ReadOnly][SerializeField] private float currentOffset;

    //  ===================================

    Coroutine spinTheWheel;
    Coroutine respawnTimer;

    //  ===================================

    private void Awake()
    {
        core.OnMatchStateChange += MatchStateChange;
    }

    private void OnDisable()
    {
        core.OnMatchStateChange -= MatchStateChange;
    }

    private void MatchStateChange(object sender, EventArgs e)
    {
        TurnOffPowerupUI();
    }

    private void TurnOffPowerupUI()
    {
        if (core.CurrentMatchState == AgawanBaseCOre.MatchState.PLAY)
            return;

        if (spinTheWheel != null)
            StopCoroutine(spinTheWheel);

        powerupParentUIObj.SetActive(false);
    }

    public void StartPowerup()
    {
        powerupCollider.enabled = false;
        powerupRenderer.enabled = false;

        //  INSERT QUESTIONS MUNA DITO SA NGAYON IS SPIN THE WHEEL NA MUNA

        spinWheelBtn.onClick.AddListener(() => RotateWheelButton());

        wheelParentUIObj.SetActive(true);
        spinWheelBtnObj.SetActive(true);
        powerupParentUIObj.SetActive(true);
    }

    public IEnumerator RotateWheel(int selectedIndex)
    {
        angleOfOneItem = circle / numberOfItems;
        startAngle = wheelTF.eulerAngles.z;
        currentTime = 0;
        selectedRotateTime = UnityEngine.Random.Range(rotateTimeMin, rotateTimeMax);
        currentOffset = UnityEngine.Random.Range(minOffset, maxOffset);

        selectedAngle = (numberCircleRotate * circle) + angleOfOneItem * selectedIndex - startAngle;

        while (currentTime < selectedRotateTime)
        {
            yield return new WaitForEndOfFrame();
            currentTime += Time.deltaTime;

            currentAngle = selectedAngle * curve.Evaluate(currentTime / selectedRotateTime);
            wheelTF.eulerAngles = new Vector3(0, 0, currentAngle + startAngle - currentOffset);
        }

        yield return new WaitForSecondsRealtime(1f);

        powerupParentUIObj.SetActive(false);
        wheelParentUIObj.SetActive(false);

        respawnTimer = StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        float currentRespawnTime = 20f;

        while (currentRespawnTime > 0)
        {
            currentRespawnTime -= Time.deltaTime;
            yield return null;
        }

        powerupCollider.enabled = true;
        powerupRenderer.enabled = true;
    }

    #region BUTTON

    public void RotateWheelButton()
    {
        spinWheelBtnObj.SetActive(false);

        int rand = UnityEngine.Random.Range(0, 3);

        Debug.Log(rand);

        spinTheWheel = StartCoroutine(RotateWheel(rand));
    }

    #endregion
}
