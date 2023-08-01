using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Data", menuName = "FilipinoGames/Shop/Character")]
public class ShopCharData : ScriptableObject
{
    public RenderTexture characterRT;
    public string characterID;
    public int price;
}
