using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CardMerger : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI NameBox;
    [SerializeField] private TMPro.TextMeshProUGUI CostBox;
    [SerializeField] private TMPro.TextMeshProUGUI RulesBox;
    [SerializeField] private TMPro.TextMeshProUGUI PTBox;
    [SerializeField] private TMPro.TextMeshProUGUI TypeBox;
    [SerializeField] private TMPro.TextMeshProUGUI LeftStatus;
    [SerializeField] private TMPro.TextMeshProUGUI RightStatus;
    [SerializeField] private GameObject PTOutline;
    [SerializeField] private GameObject UICover;

    private CardData leftCard;
    private CardData rightCard;
    private CardData mergedCard;
    private bool downloadToLeft; // false: downloads to right

    private static List<string> invalidCardTypes = new List<string> { 
        "Battle",
        "Conspiracy",
        "Emblem",
        "Hero",
        "Instant",
        "Land",
        "Phenomenon",
        "Plane",
        "Planeswalker",
        "Scheme",
        "Sorcery",
        "Vanguard",
        "Token",
        "Aura",
        "Saga",
        "Class",
        "Case"
    };

    private static List<string> keywords = new List<string> {
        "Flash",
        "Defender",
        "Flying",
        "Reach",
        "Haste",
        "Trample",
        "Menace",
        "Deathtouch",
        "Lifelink",
        "Vigilance",
        "First strike",
        "Double strike",
        "Prowess",
        "Skulk",
        "Convoke",
        "Hexproof",
        "Indestructible",
        "Enlist"
    };

    private static List<string> typeOrder = new List<string> {
        "Legendary",
        "Snow",
        "Tribal",
        "Kindred",
        "Artifact",
        "Enchantment",
        "Creature"
    };

    public void SearchCard(string cardName) {
        StartCoroutine(GetRequest("https://api.scryfall.com/cards/named?fuzzy=" + cardName));
    }

    private IEnumerator GetRequest(string url) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url)) {
            yield return webRequest.SendWebRequest();

            switch(webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    if(downloadToLeft) {
                        LeftStatus.text = "Failed to find card.";
                    } else {
                        RightStatus.text = "Failed to find card.";
                    }
                    break;
                case UnityWebRequest.Result.Success:
                    CardData cardResult = JsonUtility.FromJson<CardData>(webRequest.downloadHandler.text);
                    if(cardResult.card_faces != null) {
                        string[] storedKeywords = cardResult.keywords;
                        cardResult = cardResult.card_faces[0]; // for double-sided cards, use the front face
                        cardResult.keywords = storedKeywords;
                    }

                    bool validCard = true;
                    string[] types = cardResult.type_line.Split(' ');
                    foreach(string type in types) {
                        if(invalidCardTypes.Contains(type)) {
                            if(downloadToLeft) {
                                LeftStatus.text = cardResult.name + " has an invalid card type";
                            } else {
                                RightStatus.text = cardResult.name + " has an invalid card type";
                            }
                            validCard = false;
                            break;
                        }
                    }

                    if(validCard) {
                        if(downloadToLeft) {
                            LeftStatus.text = cardResult.name + " is ready to merge.";
                            leftCard = cardResult;
                        } else {
                            RightStatus.text = cardResult.name + " is ready to merge.";
                            rightCard = cardResult;
                        }
                    }
                    break;
            }
        }
    }

    public void MergeCards() {
        if(leftCard == null || rightCard == null) {
            return;
        }

        mergedCard = new CardData();
        mergedCard.name = MergeNames(leftCard.name, rightCard.name);
        mergedCard.mana_cost = MergeCosts(leftCard.mana_cost, rightCard.mana_cost);
        mergedCard.type_line = MergeTypes(leftCard.type_line, rightCard.type_line);
        mergedCard.power = MergePowers(leftCard.power, rightCard.power);
        mergedCard.toughness = MergeToughness(leftCard.toughness, rightCard.toughness);

        // create oracle text
        string leftRules = leftCard.oracle_text;
        string rightRules = rightCard.oracle_text;

        HashSet<string> realKeywords = RemoveKeywords(ref leftRules, leftCard.keywords);
        realKeywords.UnionWith(RemoveKeywords(ref rightRules, rightCard.keywords));

        mergedCard.oracle_text = leftRules + "\n" + rightRules;
        mergedCard.oracle_text = mergedCard.oracle_text.Replace(leftCard.name.Length > rightCard.name.Length ? leftCard.name : rightCard.name, "~").Replace(leftCard.name.Length > rightCard.name.Length ? rightCard.name : leftCard.name, "~").Replace("~", mergedCard.name);

        if(realKeywords.Count > 0) {
            string keywordChunk = "";
            foreach(string keyword in realKeywords) {
                keywordChunk += keyword.ToLower() + ", ";
            }
            keywordChunk = char.ToUpper(keywordChunk[0]) + keywordChunk.Substring(1) ;

            mergedCard.oracle_text = keywordChunk + "\n" + mergedCard.oracle_text;
        }

        if(mergedCard.type_line.Contains("Creature") && mergedCard.type_line.Contains("Equipment")) {
            mergedCard.oracle_text = mergedCard.oracle_text.Replace("\nEquip {", "\nReconfigure {").Replace("\nEquip—", "\nReconfigure—");
        }

        DisplayCard();
    }

    private HashSet<string> RemoveKeywords(ref string ruleText, string[] cardKeywords) {
        if(cardKeywords == null || cardKeywords.Length == 0) {
            return new HashSet<string>();
        }

        HashSet<string> removedKeywords = new HashSet<string>();
        foreach(string possibleKeyword in cardKeywords) {
            if(keywords.Contains(possibleKeyword)) {
                removedKeywords.Add(possibleKeyword);
                int indexUpper = ruleText.IndexOf(possibleKeyword);
                int indexLower = ruleText.IndexOf(possibleKeyword.ToLower());
                int index = indexUpper == -1 ? indexLower : indexUpper;
                if(indexLower >= 0 && indexUpper >= 0) {
                    // happens for terms like "flash" that might find "flashback"
                    index = Mathf.Min(indexLower, indexUpper);
                }

                ruleText = ruleText.Remove(index, possibleKeyword.Length);
            }
        }

        for(int i = 0; i < ruleText.Length; i++) {
            if(ruleText[i] != ',' && ruleText[i] != ' ' && ruleText[i] != '\n' && ruleText[i] != ';') {
                ruleText = ruleText.Substring(i);
                break;
            }
        }

        return removedKeywords;
    }

    private string MergeNames(string leftName, string rightName) {
        if(leftName == rightName) {
            return leftName + " x2";
        }

        string[] leftWords = leftName.Split(' ');
        string[] rightWords = rightName.Split(' ');

        if(leftWords.Length == 1 && rightWords.Length == 1) {
            // split the one word
            return leftWords[0].Substring(0, leftWords[0].Length / 2) + rightWords[0].Substring(rightWords[0].Length / 2);
        }

        string[] longerName = leftWords.Length > rightWords.Length ? leftWords : rightWords;
        string[] shorterName = longerName == leftWords ? rightWords : leftWords;

        string finalName = "";
        for(int i = 0; i < longerName.Length; i++) {
            if(i < shorterName.Length && i < longerName.Length / 2) {
                finalName += shorterName[i];
            } else {
                finalName += longerName[i];
            }
            if(i < longerName.Length - 1) {
                finalName += " ";
            }
        }

        return finalName;
    }

    private string MergeCosts(string leftCost, string rightCost) {
        if(leftCost.Length > 2) {
            leftCost = leftCost.Substring(1, leftCost.Length - 2);
        }
        if(rightCost.Length > 2) {
            rightCost = rightCost.Substring(1, rightCost.Length - 2);
        }
        string[] leftPips = leftCost.Split("}{");
        string[] rightPips = rightCost.Split("}{");

        Dictionary<string, int> pipCounts = new Dictionary<string, int>();

        foreach(string[] pipList in new string[2][] { leftPips, rightPips }) {
            foreach(string pip in pipList) {
                int genericCost;
                bool isGeneric = int.TryParse(pip, out genericCost);
                if(isGeneric) {
                    if(!pipCounts.ContainsKey("0")) {
                        pipCounts["0"] = 0;
                    }
                    pipCounts["0"] += genericCost;
                    continue;
                }

                if(!pipCounts.ContainsKey(pip)) {
                    pipCounts[pip] = 0;
                }
                pipCounts[pip]++;
            }
        }

        string[] keys = new string[pipCounts.Count];
        pipCounts.Keys.CopyTo(keys, 0);
        List<string> sortKeys = new List<string>(keys);
        sortKeys.Sort((string cur, string next) => { return (cur == "0" ? -1 : 0) - (next == "0" ? -1 : 0); });

        string finalCost = "";
        foreach(string pip in sortKeys) {
            if(pip == "0") {
                finalCost += pipCounts[pip];
                continue;
            }

            bool addBrackets = pip.Length > 1;
            for(int i = 0; i < pipCounts[pip]; i++) {
                finalCost += (addBrackets ? "{" : "") + pip + (addBrackets ? "}" : "");
            }
        }

        return finalCost;
    }

    private string MergeTypes(string leftType, string rightType) {
        string[] leftTypes = leftType.Split(' ');
        string[] rightTypes = rightType.Split(' ');

        List<string> types = new List<string>(leftTypes);
        foreach(string type in rightTypes) {
            if(!types.Contains(type)) {
                types.Add(type);
            }
        }

        types.Sort((string cur, string next) => { return TypeSortValue(cur) - TypeSortValue(next); });

        string typeLine = "";
        foreach(string type in types) {
            typeLine += type + " ";
        }
        
        return typeLine;
    }

    private int TypeSortValue(string type) {
        int index = typeOrder.IndexOf(type);
        return index == -1 ? int.MaxValue : index;
    }

    private string MergePowers(string leftPower, string rightPower) {
        if(leftPower == null && rightPower == null) {
            return "";
        }

        if(rightPower != null && rightPower.Contains('*')) {
            return rightPower;
        }

        if(leftPower != null && leftPower.Contains('*')) {
            return leftPower;
        }

        int totalPower = 0;
        if(leftPower != null) {
            totalPower += int.Parse(leftPower);
        }
        if(rightPower != null) {
            totalPower += int.Parse(rightPower);
        }

        return "" + totalPower;
    }

    private string MergeToughness(string leftTough, string rightTough) {
        if(leftTough == null && rightTough == null) {
            return "";
        }

        if(rightTough != null && rightTough.Contains('*')) {
            return rightTough;
        }

        if(leftTough != null && leftTough.Contains('*')) {
            return leftTough;
        }

        int totalTough = 0;
        if(leftTough != null) {
            totalTough += int.Parse(leftTough);
        }
        if(rightTough != null) {
            totalTough += int.Parse(rightTough);
        }

        return "" + totalTough;
    }

    private void DisplayCard() {
        if(mergedCard == null) {
            return;
        }
        bool isCreature = mergedCard.type_line.Contains("Creature");

        NameBox.text = mergedCard.name;
        CostBox.text = mergedCard.mana_cost;
        TypeBox.text = mergedCard.type_line;
        RulesBox.text = mergedCard.oracle_text;
        PTBox.text = isCreature ? mergedCard.power + "/" + mergedCard.toughness : "";
        PTOutline.SetActive(isCreature);
    }

    #region UI functions
    public void SearchLeft(string searchTerm) {
        downloadToLeft = true;
        SearchCard(searchTerm);
    }

    public void SearchRight(string searchTerm) {
        downloadToLeft = false;
        SearchCard(searchTerm);
    }

    public void Print() {
        if(mergedCard == null) {
            return;
        }

        UICover.SetActive(true);
        ScreenCapture.CaptureScreenshot("..\\" + mergedCard.name + ".png");
        Debug.Log("captured screenshot");
        Invoke("HideCover", 0.2f);
    }

    private void HideCover() {
        UICover.SetActive(false);
    }

    public void Quit() {
        Application.Quit();
    }
    #endregion
}
