using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TriviaController : MonoBehaviour
{
    [SerializeField] private PlayeData playerData;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Button buyBtn;

    [Header("DEBUGGER")]
    [ReadOnly] public TriviaData triviaData;

    public void CheckIfUnlocked()
    {
        title.text = triviaData.itemName;

        if (playerData.UnlockedTrivias.Contains(triviaData.itemID))
            buyBtn.interactable = false;
        else
            buyBtn.interactable = true;
    }

    public void BuyTrivia()
    {
        GameManager.Instance.ErrorControl.ShowConfirmation("Are you sure you want to buy this " + triviaData.itemName + " for " + triviaData.price + " credits?", () =>
        {
            if (playerData.Credits < triviaData.price)
            {
                GameManager.Instance.ErrorControl.ShowError("Insufficient funds! Please play more to earn more credits.", null);
                return;
            }

            playerData.UnlockTrivia(triviaData.itemID);
            CheckIfUnlocked();
        }, null);
    }
}
