using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Colors {
    public bool white;
    public bool blue;
    public bool black;
    public bool red;
    public bool green;

    public Colors(bool white, bool blue, bool black, bool red, bool green) {
        this.white = white;
        this.blue = blue;
        this.black = black;
        this.red = red;
        this.green = green;
    }

    public int Total() {
        return (white ? 1 : 0) + (blue ? 1 : 0) + (black ? 1 : 0) + (red ? 1 : 0) + (green ? 1 : 0);
    }
}

public struct Ability {
    public string name;
    public int pointCost;
    public Colors colorRequirement;
    public bool chosen;

    public Ability(string name, int pointCost, Colors colorRequirement) {
        this.pointCost = pointCost;
        this.name = name;
        this.colorRequirement = colorRequirement;
        chosen = false;
    }
}

public class CardMaker : MonoBehaviour {
    [SerializeField] private TMPro.TextMeshProUGUI PowerLevelBox;
    [SerializeField] private TMPro.TextMeshProUGUI NameBox;
    [SerializeField] private TMPro.TextMeshProUGUI CostBox;
    [SerializeField] private TMPro.TextMeshProUGUI RulesBox;
    [SerializeField] private TMPro.TextMeshProUGUI PTBox;

    private const int POINTS_PER_MANA = 3;

    public int PointTotal { get; private set; }
    public Colors ColorIdentity { get; private set; }
    public int ManaValue { get; private set; }
    public int Power { get; private set; }
    public int Toughness { get; private set; }

    private Ability[] keywordOptions;

    void Start() {
        ColorIdentity = new Colors(false, false, false, false, false);
        DefineAbilities();
        UpdateCard();
    }

    private void CalculatePoints() {
        PointTotal = ManaValue * POINTS_PER_MANA;

        // power / toughness
        PointTotal -= Toughness;
        PointTotal -= Power * 2;

        // abilities
        foreach(Ability ability in keywordOptions) {
            if(ability.chosen) {
                PointTotal -= ability.pointCost;
            }
        }
    }

    private void UpdateCard() {
        CalculatePoints();

        // power level
        PowerLevelBox.text = "Power Level: " + Mathf.CeilToInt((float)-PointTotal / POINTS_PER_MANA);

        // mana cost
        int genericCost = ManaValue - ColorIdentity.Total();
        CostBox.text = (genericCost > 0 ? "" + genericCost : "");
        if(ColorIdentity.white) { CostBox.text += "W"; }
        if(ColorIdentity.blue) { CostBox.text += "U"; }
        if(ColorIdentity.black) { CostBox.text += "B"; }
        if(ColorIdentity.red) { CostBox.text += "R"; }
        if(ColorIdentity.green) { CostBox.text += "G"; }
        if(ManaValue == 0 && ColorIdentity.Total() == 0) { CostBox.text = "0"; };

        // power / toughness
        PTBox.text = Power + "/" + Toughness;

        // rules text
        string rulesText = "";
        foreach(Ability ability in keywordOptions) {
            if(ability.chosen) {
                rulesText += ability.name + ", ";
            }
        }
        if(rulesText.Length > 0) {
            rulesText = rulesText.Substring(0, rulesText.Length - 2); // eliminate ending ", "
            rulesText = (rulesText[0] + "").ToUpper() + rulesText.Substring(1, rulesText.Length - 1); // capitalize first letter
        }
        rulesText = rulesText.Replace("CARDNAME", NameBox.text);
        RulesBox.text = rulesText;
    }

    private bool HasRequiredColor(Colors requirements) {
        if(requirements.Total() == 0) {
            return true;
        }

        return ColorIdentity.white && requirements.white ||
            ColorIdentity.blue && requirements.blue ||
            ColorIdentity.black && requirements.black ||
            ColorIdentity.red && requirements.red ||
            ColorIdentity.green && requirements.green;
    }

    #region Button Events
    public void IncreaseManaValue() {
        if(ManaValue >= 9) {
            return;
        }

        ManaValue++;
        UpdateCard();
    }

    public void DecreaseManaValue() {
        if(ManaValue <= 0) {
            return;
        }

        ManaValue--;
        UpdateCard();
    }

    public void IncreasePower() {
        if(Power >= 9) {
            return;
        }

        Power++;
        UpdateCard();
    }

    public void DecreasePower() {
        if(Power <= 0) {
            return;
        }

        Power--;
        UpdateCard();
    }

    public void IncreaseToughness() {
        if(Toughness >= 9) {
            return;
        }

        Toughness++;
        UpdateCard();
    }

    public void DecreaseToughness() {
        if(Toughness <= 0) {
            return;
        }

        Toughness--;
        UpdateCard();
    }

    private void ToggleColor(ref bool flag) {
        flag = !flag;
        if(ManaValue < ColorIdentity.Total()) {
            ManaValue = ColorIdentity.Total();
        }
        UpdateCard();
    }

    public void ToggleWhite() {
        ToggleColor(ref ColorIdentity.white);
    }

    public void ToggleBlue() {
        ToggleColor(ref ColorIdentity.blue);
    }

    public void ToggleBlack() {
        ToggleColor(ref ColorIdentity.black);
    }

    public void ToggleRed() {
        ToggleColor(ref ColorIdentity.red);
    }

    public void ToggleGreen() {
        ToggleColor(ref ColorIdentity.green);
    }
    #endregion

    private void DefineAbilities() {
        keywordOptions = new Ability[16];

        keywordOptions[0] = new Ability("CARDNAME can't block", -2, new Colors(false, false, false, false, false));
        keywordOptions[1] = new Ability("defender", -2, new Colors(false, false, false, false, false));
        keywordOptions[2] = new Ability("flying", 2, new Colors(true, true, true, false, false));
        keywordOptions[3] = new Ability("reach", 1, new Colors(false, false, false, true, true));
        keywordOptions[4] = new Ability("haste", 2, new Colors(false, false, false, true, false));
        keywordOptions[5] = new Ability("trample", 2, new Colors(false, false, false, true, true));
        keywordOptions[6] = new Ability("menace", 1, new Colors(false, false, true, true, false));
        keywordOptions[7] = new Ability("deathtouch", 1, new Colors(false, false, true, false, true));
        keywordOptions[8] = new Ability("first strike", 2, new Colors(true, false, false, true, false));
        keywordOptions[9] = new Ability("lifelink", 2, new Colors(true, false, true, false, false));
        keywordOptions[10] = new Ability("vigilance", 2, new Colors(true, false, false, false, true));
        keywordOptions[11] = new Ability("double strike", 4, new Colors(true, false, false, true, false));
        keywordOptions[12] = new Ability("prowess", 2, new Colors(true, true, false, true, false));
        keywordOptions[13] = new Ability("skulk", 2, new Colors(false, true, true, false, false));
        keywordOptions[14] = new Ability("ward 2", 3, new Colors(true, true, false, false, true));
        keywordOptions[15] = new Ability("indestructible", 4, new Colors(true, false, true, false, true));
    }
}