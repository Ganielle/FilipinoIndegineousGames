using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionAnswer : MonoBehaviour
{
    [SerializeField] private QuestionCore questionCore;

    [Header("SETTINGS")]
    [SerializeField] private TextMeshProUGUI choiceTMP;
    [SerializeField] private Image choiceIMG;
    [SerializeField] private Color unselected;
    [SerializeField] private Color selected;

    [Header("DEBUGGER")]
    [ReadOnly] public string choice; 

    private void OnEnable()
    {
        questionCore.OnAnswerChanged += CheckAnswer;
    }

    private void OnDisable()
    {
        questionCore.OnAnswerChanged -= CheckAnswer;
    }

    private void CheckAnswer(object sender, EventArgs e)
    {
        CheckColor();
    }

    public void CheckColor()
    {
        if (questionCore.SelectedAnswer == choice)
            choiceIMG.color = selected;
        else
            choiceIMG.color = unselected;
    }

    public void ChoiceButton()
    {
        if (questionCore.SelectedAnswer == choice) return;

        questionCore.SelectedAnswer = choice;
    }
}
