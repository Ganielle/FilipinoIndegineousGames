using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class QuestionCore : MonoBehaviour
{

    private event EventHandler AnswerChanged;
    public event EventHandler OnAnswerChanged
    {
        add
        {
            if (AnswerChanged == null || !AnswerChanged.GetInvocationList().Contains(value))
                AnswerChanged += value;
        }
        remove { AnswerChanged -= value; }
    }
    public string SelectedAnswer
    {
        get => selectedAnswer;
        set
        {
            selectedAnswer = value;
            AnswerChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    //  ===============================

    [Header("QUESTIONS")]
    [SerializeField] private GameObject questionParentObj;
    [SerializeField] private TextMeshProUGUI questionTMP;
    [SerializeField] private GameObject nextActionBtn;
    [SerializeField] private List<TextMeshProUGUI> choicesList;
    [SerializeField] private List<QuestionAnswer> questionAnswersList;
    [SerializeField] private List<QuestionData> questionDataList;

    [Header("CHECK")]
    [SerializeField] private GameObject rightObj;
    [SerializeField] private GameObject wrongObj;

    [Header("QUESTION DEBUGGER")]
    [ReadOnly][SerializeField] private QuestionData selectedQuestionData;
    [ReadOnly][SerializeField] private string selectedAnswer;

    //  ===============================

    Action rightAnswerAction;
    Action wrongAnswerAction;

    //  ===============================

    private void Awake()
    {
        OnAnswerChanged += CheckAnswer;
    }

    private void OnDisable()
    {
        OnAnswerChanged -= CheckAnswer;
    }

    private void CheckAnswer(object sender, EventArgs e)
    {
        if (selectedAnswer == "")
            nextActionBtn.SetActive(false);
        else
            nextActionBtn.SetActive(true);
    }

    public IEnumerator SetQuestions(Action rightAnswerAction, Action wrongAnswerAction)
    {
        SelectedAnswer = "";

        this.rightAnswerAction = rightAnswerAction;
        this.wrongAnswerAction = wrongAnswerAction;

        yield return StartCoroutine(Shuffler.Shuffle(questionDataList));

        selectedQuestionData = questionDataList[UnityEngine.Random.Range(0, questionDataList.Count - 1)];

        yield return StartCoroutine(Shuffler.Shuffle(selectedQuestionData.choices));

        questionTMP.text = selectedQuestionData.question;

        for (int a = 0; a < selectedQuestionData.choices.Count; a++)
        {
            choicesList[a].text = selectedQuestionData.choices[a];
            questionAnswersList[a].choice = selectedQuestionData.choices[a];
            questionAnswersList[a].CheckColor();
            yield return null;
        }

        questionParentObj.SetActive(true);
    }

    private IEnumerator CheckNextAnswer()
    {
        questionParentObj.SetActive(false);

        if (SelectedAnswer == selectedQuestionData.answer)
        {
            //  RIGHT
            rightObj.SetActive(true);

            yield return new WaitForSeconds(1f);

            rightObj.SetActive(false);

            rightAnswerAction();
        }
        else
        {
            //  WRONG
            wrongObj.SetActive(true);

            yield return new WaitForSeconds(1f);

            wrongObj.SetActive(false);

            wrongAnswerAction();
        }

        questionTMP.text = "";

        for (int a = 0; a < choicesList.Count; a++)
        {
            choicesList[a].text = "";
            questionAnswersList[a].choice = "";
            questionAnswersList[a].CheckColor();
            yield return null;
        }
    }

    #region BUTTON

    public void NextButton()
    {
        StartCoroutine(CheckNextAnswer());
    }

    #endregion
}