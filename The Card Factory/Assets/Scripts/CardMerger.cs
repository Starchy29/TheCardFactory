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
        "Kindred",
        "Vanguard",
        "Token",
        "Aura",
        "Saga",
        "Class",
        "Case"
    };

    void Start() {
        
    }

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
                        cardResult = cardResult.card_faces[0]; // for double-sided cards, use the front face
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
        mergedCard.oracle_text = MergeRules(leftCard.oracle_text, rightCard.oracle_text);
        mergedCard.power = MergePowers(leftCard.power, rightCard.power);
        mergedCard.toughness = MergeToughness(leftCard.toughness, rightCard.toughness);

        // account for vehicles
        // account for * PT

        DisplayCard();
    }

    private string MergeNames(string leftName, string rightName) {
        return leftName;
    }

    private string MergeCosts(string leftCost, string rightCost) {
        return leftCost;
    }

    private string MergeTypes(string leftType, string rightType) {
        return leftType;
    }

    private string MergeRules(string leftRules, string rightRules) {
        return leftRules;
    }

    private string MergePowers(string leftPower, string rightPower) {
        return leftPower;
    }

    private string MergeToughness(string leftTough, string rightTough) {
        return leftTough;
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
    #endregion
}
