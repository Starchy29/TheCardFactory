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

public class Ability {
    public string text;
    public int pointCost;
    public Colors colorRequirement;
    public bool chosen;

    public Ability(string text, int pointCost, Colors colorRequirement) {
        this.pointCost = pointCost;
        this.text = text;
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
    [SerializeField] private GameObject UICover;
    [SerializeField] private List<Toggle> keywordToggles; // order should match ability indices
    [SerializeField] private TMPro.TMP_Dropdown[] abilityDropdowns;

    private const int POINTS_PER_MANA = 3;

    public int PointTotal { get; private set; }
    public Colors ColorIdentity { get; private set; }
    public int ManaValue { get; private set; }
    public int Power { get; private set; }
    public int Toughness { get; private set; }

    private Ability[] keywordOptions;
    private CreatureAbility ability1;
    private CreatureAbility ability2;

    void Start() {
        abilityDropdowns[0].onValueChanged.AddListener((index) => { SetAbilityTrigger(ability1, index); });
        abilityDropdowns[1].onValueChanged.AddListener((index) => { SetAbilityEffect(ability1, index); });
        abilityDropdowns[2].onValueChanged.AddListener((index) => { SetAbilityTrigger(ability2, index); });
        abilityDropdowns[3].onValueChanged.AddListener((index) => { SetAbilityEffect(ability2, index); });
        ability1 = new CreatureAbility();
        ability2 = new CreatureAbility();

        Toughness = 1;
        ColorIdentity = new Colors(false, false, false, false, false);
        DefineAbilities();
        CheckValidAbilities();
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

        if(ability1 != null && ability1.Active && HasRequiredColor(ability1.Effect.colorRequirement)) {
            PointTotal -= ability1.Cost;
        }
        if(ability2 != null && ability2.Active && HasRequiredColor(ability2.Effect.colorRequirement)) {
            PointTotal -= ability2.Cost;
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
                rulesText += ability.text + ", ";
            }
        }
        if(rulesText.Length > 0) {
            rulesText = rulesText.Substring(0, rulesText.Length - 2); // eliminate ending ", "
            rulesText = (rulesText[0] + "").ToUpper() + rulesText.Substring(1, rulesText.Length - 1); // capitalize first letter
        }


        if(ability1 != null && ability1.Active && HasRequiredColor(ability1.Effect.colorRequirement)) {
            rulesText += (rulesText.Length > 0 ? "\n" : "") + ability1.Text;
        }
        if(ability2 != null && ability2.Active && HasRequiredColor(ability2.Effect.colorRequirement)) {
            rulesText += (rulesText.Length > 0 ? "\n" : "") + ability2.Text;
        }

        rulesText = rulesText.Replace("CARDNAME", NameBox.text);
        RulesBox.text = rulesText;
    }

    private bool HasRequiredColor(Colors requirements) {
        return requirements.Total() == 5 ||
            ColorIdentity.white && requirements.white ||
            ColorIdentity.blue && requirements.blue ||
            ColorIdentity.black && requirements.black ||
            ColorIdentity.red && requirements.red ||
            ColorIdentity.green && requirements.green;
    }

    private void CheckValidAbilities() {
        // check keywords
        for(int i = 0; i < keywordOptions.Length; i++) {
            bool valid = HasRequiredColor(keywordOptions[i].colorRequirement);
            keywordToggles[i].interactable = valid;
            if(keywordOptions[i].chosen && !valid) {
                keywordOptions[i].chosen = false;
                keywordToggles[i].isOn = false;
                keywordToggles[i].onValueChanged.Invoke(false);
            }
        }
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
        if(ManaValue <= ColorIdentity.Total()) {
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
        if(Toughness <= 1) {
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
        CheckValidAbilities();
        UpdateCard();
    }

    public void ToggleWhite() { ToggleColor(ref ColorIdentity.white); }
    public void ToggleBlue() { ToggleColor(ref ColorIdentity.blue); }
    public void ToggleBlack() { ToggleColor(ref ColorIdentity.black); }
    public void ToggleRed() { ToggleColor(ref ColorIdentity.red); }
    public void ToggleGreen() { ToggleColor(ref ColorIdentity.green); }

    public void ToggleKeyword(Toggle thrower) {
        Ability ability = keywordOptions[keywordToggles.IndexOf(thrower)];
        ability.chosen = !ability.chosen;
        UpdateCard();
    }

    public void UpdateName(TMPro.TextMeshProUGUI inputText) {
        NameBox.text = inputText.text;
    }

    private void SetAbilityTrigger(CreatureAbility ability, int index) {
        if(index == 0) {
            ability.Active = false;
            UpdateCard();
            return;
        }

        ability.Active = true;
        ability.Trigger = CreatureAbility.Triggers[index - 1];
        UpdateCard();
    }

    private void SetAbilityEffect(CreatureAbility ability, int index) {
        ability.Effect = CreatureAbility.Effects[index];
        CheckValidAbilities();
        UpdateCard();
    }

    public void Print() {
        UICover.SetActive(true);
        ScreenCapture.CaptureScreenshot("custom card.png");
        Debug.Log("captured screenshot");
        Invoke("HideCover", 0.2f);
    }

    private void HideCover() {
        UICover.SetActive(false);
    }
    #endregion

    private void DefineAbilities() {
        keywordOptions = new Ability[16];

        keywordOptions[0] = new Ability("CARDNAME can't block", -2, new Colors(true, true, true, true, true));
        keywordOptions[1] = new Ability("defender", -2, new Colors(true, true, true, true, true));
        keywordOptions[2] = new Ability("flying", 3, new Colors(true, true, true, false, false));
        keywordOptions[3] = new Ability("reach", 1, new Colors(false, false, false, true, true));
        keywordOptions[4] = new Ability("haste", 3, new Colors(false, false, false, true, false));
        keywordOptions[5] = new Ability("trample", 3, new Colors(false, false, false, true, true));
        keywordOptions[6] = new Ability("menace", 2, new Colors(false, false, true, true, false));
        keywordOptions[7] = new Ability("deathtouch", 2, new Colors(false, false, true, false, true));
        keywordOptions[8] = new Ability("lifelink", 3, new Colors(true, false, true, false, false));
        keywordOptions[9] = new Ability("vigilance", 3, new Colors(true, false, false, false, true));
        keywordOptions[10] = new Ability("first strike", 3, new Colors(true, false, false, true, false));
        keywordOptions[11] = new Ability("double strike", 6, new Colors(true, false, false, true, false));
        keywordOptions[12] = new Ability("prowess", 4, new Colors(true, true, false, true, false));
        keywordOptions[13] = new Ability("skulk", 2, new Colors(false, true, true, false, false));
        keywordOptions[14] = new Ability("ward 2", 3, new Colors(true, true, false, false, true));
        keywordOptions[15] = new Ability("indestructible", 6, new Colors(true, false, true, false, true));
    }
}
