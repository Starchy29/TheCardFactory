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
    [SerializeField] private GameObject UICover;

    private CardData leftCard;
    private CardData rightCard;
    private bool downloadToLeft; // false: downloads to right

    void Start() {
        SearchCard("opt");
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
                    Debug.Log("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    CardData cardResult = JsonUtility.FromJson<CardData>(webRequest.downloadHandler.text);
                    if(downloadToLeft) {
                        leftCard = cardResult;
                    } else {
                        rightCard = cardResult;
                    }
                    DisplayCard();
                    break;
            }
        }
    }

    private void MergeCards() {
        // check if there are two cards that can combine
        if(leftCard == null || rightCard == null) {
            return;
        }

        if(leftCard.name == null || rightCard.name == null) {
            // does not work for cards with multiple faces
            return;
        }
    }

    private void DisplayCard() {
        if(leftCard == null) {
            return;
        }

        NameBox.text = leftCard.name;
        CostBox.text = leftCard.mana_cost;
        TypeBox.text = leftCard.type_line;
        RulesBox.text = leftCard.oracle_text;
        PTBox.text = leftCard.power + "/" + leftCard.toughness; 
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
