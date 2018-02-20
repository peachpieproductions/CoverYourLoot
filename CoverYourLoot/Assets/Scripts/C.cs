using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyWiFi.Core;
using EasyWiFi.ClientControls;

public class C : MonoBehaviour {

    public static C c;
    public Data data;
    public string playerName;
    public Text playerNameText;
    public Text logText;
    public Text playerNumberText;
    public Text yourTurnText;
    public GameObject pairWithDiscardPileButton;
    public CardClient[] cards;
    public int[] cardIds;
    public List<int> cardsSelected;
    public bool discarding;
    public bool isTurn;
    public int currentDiscard;
    public bool canMatchWithDiscard;
    float wait;

    //bc and data
    public StringDataClientController dcNewPair;
    public IntDataClientController dcDiscardCard;

	// Use this for initialization
	void Start () {
        Application.targetFrameRate = 60;
        c = GetComponent<C>();
        playerNameText.text = playerName;
        StartCoroutine(SlowUpdate());
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    public void EnableDiscarding() {
        discarding = !discarding;
        if (discarding) logText.text = "Discard One Card!";
    }

    public void PairWithDiscardPile() {
        CreatePair(true);
        wait = 1.5f;
    }

    public void CreatePair(bool useDiscard = false) {
        var newStr = "";
        if (!useDiscard) for (var i = 0; i < 4; i++) { if (cardsSelected[0] == i || cardsSelected[1] == i) newStr += "1"; else newStr += "0"; }
        else for (var i = 0; i < 4; i++) { if (cardsSelected[0] == i) newStr += "1"; else newStr += "0"; }
        dcNewPair.setValue(newStr);
        cards[cardsSelected[0]].TakeCard();
        if (!useDiscard) cards[cardsSelected[1]].TakeCard();
        cardsSelected.Clear();
    }

    IEnumerator SlowUpdate() {
        while (true) {
            if (wait > 0) { yield return new WaitForSeconds(wait); wait = 0f; }
            if (isTurn) {
                if (canMatchWithDiscard) {
                    bool canMatch = false;
                    if (cardsSelected.Count == 1 && currentDiscard > 0) {
                        if (cardIds[cardsSelected[0]] == currentDiscard || currentDiscard < 3) {
                            canMatch = true;
                        }
                    }
                    if (!canMatch) {
                        canMatchWithDiscard = false;
                        pairWithDiscardPileButton.SetActive(false);
                    }
                }

                if (discarding) {
                    if (cardsSelected.Count > 0) {
                        dcDiscardCard.setValue(cardsSelected[0] + 1);
                        cards[cardsSelected[0]].TakeCard();
                        cardsSelected.Clear();
                        discarding = false;
                        yield return new WaitForSeconds(1f);
                    }
                } else if (cardsSelected.Count == 1) {
                    if (currentDiscard > 0) {
                        if (cardIds[cardsSelected[0]] == currentDiscard || currentDiscard < 3) { //can match with discard
                            if (!canMatchWithDiscard) {
                                canMatchWithDiscard = true;
                                pairWithDiscardPileButton.SetActive(true);
                            }
                        }
                    }
                } else if (cardsSelected.Count == 2) {
                    if (cardIds[cardsSelected[0]] == cardIds[cardsSelected[1]] || cardIds[cardsSelected[0]] < 3 || cardIds[cardsSelected[1]] < 3) { //matched!
                        CreatePair();
                        yield return new WaitForSeconds(1.5f);
                    }
                }
            }

            for (var i = 0; i < 4; i++) {
                if (!cards[i].inHand && cardIds[i] > 0) {
                    cards[i].StartReceive();
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    public void HandUpdated(StringBackchannelType str) {
        if (str.STRING_VALUE != null) {
            //Debug.Log(str.STRING_VALUE);
            string handStr = str.STRING_VALUE;
            for (var i = 0; i < 4; i++) {
                cardIds[i] = handStr[i] - 65;
            }
            currentDiscard = handStr[4] - 65;
            var pnum = int.Parse(handStr.Substring(5, 1));
            var playerTurn = int.Parse(handStr.Substring(6, 1));
            var isTurnLastFrame = isTurn;
            if (playerTurn == pnum) isTurn = true; else isTurn = false;
            if (!isTurnLastFrame && isTurn) { //start turn
                RefreshClient();
            }
            if (isTurn) yourTurnText.text = "Your Turn!";
            else yourTurnText.text = "Waiting...";
            playerNumberText.text = "Player " + (pnum+1);
        }
    }

    public void GotMessageConfirmation(BoolBackchannelType b) {
        //Debug.Log("MessageConfirmation!");
        RefreshClient();
    }

    public void RefreshClient() {
        foreach (CardClient card in cards) {
            if (card.inHand) {
                card.selected = false;
                card.StartMoveToPosition(card.startPos);
            }
        }
        discarding = false;
        cardsSelected.Clear();
        dcDiscardCard.setValue(0);
        dcNewPair.setValue("0000");
    }
}
