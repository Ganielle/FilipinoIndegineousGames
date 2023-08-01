using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopCharController : MonoBehaviour
{
    [SerializeField] private RawImage itemRawImg;
    [SerializeField] private GameObject buyBtnObj;
    [SerializeField] private GameObject equipBtnObj;
    [SerializeField] private Button equipBtn;

    [Header("DEBUGGER")]
    [ReadOnly] public ShopCharData shopCharData;
    [ReadOnly] public PlayeData playerData;
    [ReadOnly] public bool isUnlocked;
    [ReadOnly] public bool isEquipped;

    //  =============================

    Action afterPurchase;

    //  =============================

    public void SetData(Action afterPurchase)
    {
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