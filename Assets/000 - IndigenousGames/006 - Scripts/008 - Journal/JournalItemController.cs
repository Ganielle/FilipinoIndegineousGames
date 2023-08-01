using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JournalItemController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;

    [Header("DEBUGGER")]
    [ReadOnly] public TriviaData triviaData;
    [ReadOnly] [SerializeField] private MainMenuCore mainMenuCore;

    public void SetData(MainMenuCore core)
    {
        mainMenuCore = core;
        title.text = triviaData.itemName;
    }

    public void OpenTrivia()
    {
        if (mainMenuCore == null) return;

        mainMenuCore.ShowJournalContent(triviaData.content);
    }
}
