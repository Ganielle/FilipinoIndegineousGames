using MyBox;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public List<string> UnlockedTrivias
    {
        get => unlockedTrivias;
    }

    private event EventHandler EquipCharacterChanged;
    public event EventHandler OnEquipCharacterChanged
    {
        add
        {
            if (EquipCharacterChanged == null || !EquipCharacterChanged.GetInvocationList().Contains(value))
                EquipCharacterChanged += value;
        }
        remove { EquipCharacterChanged -= value; }
    }

    //  ==============================

    [ReadOnly][SerializeField] private int credits;
    [ReadOnly][SerializeField] private string equipCharacter;
    [ReadOnly][SerializeField] private List<string> unlockedCharacters;
    [ReadOnly][SerializeField] private List<string> unlockedTrivias;

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

        if (PlayerPrefs.HasKey("unlockTrivias"))
        {
            unlockedTrivias = JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString("unlockTrivias"));
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
        EquipCharacterChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UnlockTrivia(string id)
    {
        unlockedTrivias.Add(id);
        PlayerPrefs.SetString("unlockTrivias", JsonConvert.SerializeObject(unlockedTrivias).ToString());
    }
}