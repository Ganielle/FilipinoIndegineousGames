using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Trivia Data", menuName = "FilipinoGames/Shop/Trivia")]
public class TriviaData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public string content;
    public int price;
}
