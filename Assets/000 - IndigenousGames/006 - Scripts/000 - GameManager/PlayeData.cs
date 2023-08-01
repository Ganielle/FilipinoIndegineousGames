using MyBox;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Data", menuName = "FilipinoGames/GameManager/Player")]
public class PlayeData : ScriptableObject
{
    public List<string> UnlockedCharacters
    {
        get => unlockedCharacters;
    }
    public string EquippedCharacter
    {
        get => equipCharacter;
    }

    public int Credits
    {
        get => credits;
        set
        {
            credits = value;
            PlayerPrefs.SetInt("credits", credits);
        }
    }

    //  ==============================

    [ReadOnly][SerializeField] private int credits;
    [ReadOnly][SerializeField] private string equipCharacter;
    [ReadOnly][SerializeField] private List<string> unlockedCharacters;

    public void SetPlayerData()
    {
        if (PlayerPrefs.HasKey("credits"))
            credits = PlayerPrefs.GetInt("credits");
        else
        {
            credits = 0;
            PlayerPrefs.SetInt("credits", credits);
        }

        if (PlayerPrefs.HasKey("unlockChars"))
        {
            unlockedCharacters = JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString("unlockChars"));
        }
        else
        {
            unlockedCharacters.Add("001");
            PlayerPrefs.SetString("unlockChars", JsonConvert.SerializeObject(unlockedCharacters).ToString());
        }

        if (PlayerPrefs.HasKey("equipChar"))
        {
            equipCharacter = PlayerPrefs.GetString("equipChar");
        }
        else
        {
            equipCharacter = "001";
            PlayerPrefs.SetString("equipChar", equipCharacter);
        }
    }

    public void UnlockCharacter(string id)
    {
        unlockedCharacters.Add(id);
        PlayerPrefs.SetString("unlockChars", JsonConvert.SerializeObject(unlockedCharacters).ToString());
    }

    public void EquipCharacter(string id)
    {
        equipCharacter = id;
        PlayerPrefs.SetString("equipChar", equipCharacter);
    }
}