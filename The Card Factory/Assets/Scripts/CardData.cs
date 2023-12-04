using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// matches the data format of a result from the scryfall api so the json can be parsed into this
[System.Serializable]
public class CardData
{
    public string name;
    public string mana_cost;
    public string type_line;
    public string oracle_text;
    public string power;
    public string toughness;
    public string[] keywords;
}
