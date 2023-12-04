using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CardMerger : MonoBehaviour
{
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
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    CardData cardResult = JsonUtility.FromJson<CardData>(webRequest.downloadHandler.text);
                    if(downloadToLeft) {
                        leftCard = cardResult;
                    } else {
                        rightCard = cardResult;
                    }
                    MergeCards();
                    break;
            }
        }
    }

    private void MergeCards() {
        // check if there are two cards that can combine
        if(leftCard == null || rightCard == null) {
            return;
        }
    }
}
