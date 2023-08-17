using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionData", menuName = "FilipinoGames/Questions/Data")]
public class QuestionData : ScriptableObject
{
    public string question;
    public List<string> choices;
    public string answer;
}
