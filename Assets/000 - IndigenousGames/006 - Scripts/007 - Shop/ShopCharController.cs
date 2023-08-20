using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCharController : MonoBehaviour
{
    [SerializeField] private RawImage itemRawImg;
    [SerializeField] private GameObject buyBtnObj;
    [SerializeField] private GameObject equipBtnObj;
    [SerializeField] private Button equipBtn;
    [SerializeField] private TextMeshProUGUI titleTMP;

    [Header("DEBUGGER")]
    [ReadOnly] public MainMenuCore core;
    [ReadOnly] public ShopCharData shopCharData;
    [ReadOnly] public PlayeData playerData;
    [ReadOnly] public bool isUnlocked;
    [ReadOnly] public bool isEquipped;

    //  =============================

    Action afterPurchase;

    //  =============================

    private void OnEnable()
    {
        playerData.OnEquipCharacterChanged += CheckEquip;
    }

    private void OnDisable()
    {
        playerData.OnEquipCharacterChanged -= CheckEquip;
    }

    private void CheckEquip(object sender, EventArgs e)
    {
        CheckIfEquip();
    }

    public void SetData(Action afterPurchase, string characterTitle)
    {
        titleTMP.text = characterTitle;

        if (afterPurchase != null) this.afterPurchase = afterPurchase;

        CheckIfUnlocked();
        CheckIfEquip();

        itemRawImg.texture = this.shopCharData.characterRT;
    }

    private void CheckIfUnlocked()
    {
        if (playerData.UnlockedCharacters.Contains(shopCharData.characterID))
            isUnlocked = true;
        else
            isUnlocked = false;

        if (isUnlocked)
        {
            buyBtnObj.SetActive(false);
            equipBtnObj.SetActive(true);
        }
        else
        {
            buyBtnObj.SetActive(true);
            equipBtnObj.SetActive(false);
        }
    }

    private void CheckIfEquip()
    {
        if (shopCharData.characterID == playerData.EquippedCharacter)
            isEquipped = true;
        else
            isEquipped = false;

        if (isEquipped)
            equipBtn.interactable = false;
        else
            equipBtn.interactable = true;
    }

    public void BuyCharacter()
    {

        GameManager.Instance.ErrorControl.ShowConfirmation("Are you sure you want to buy this character? Price: " +
            shopCharData.price + " credits", () =>
            {
                if (playerData.Credits < shopCharData.price)
                {
                    GameManager.Instance.ErrorControl.ShowError("Insufficient Funds! Please earn more credits to purchase this item", null);
                    return;
                }
                playerData.Credits -= shopCharData.price;
                playerData.UnlockCharacter(shopCharData.characterID);
                CheckIfUnlocked();
                core.RefreshShopCredits();

                if (afterPurchase != null)
                    afterPurchase();
            }, null);
    }

    public void EquipCharacter()
    {
        GameManager.Instance.ErrorControl.ShowConfirmation("Are you sure you want to equip this character?", () =>
        {
            playerData.EquipCharacter(shopCharData.characterID);
            CheckIfEquip();
        }, null);
    }
}
