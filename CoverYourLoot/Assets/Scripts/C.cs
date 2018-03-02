using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyWiFi.Core;
using EasyWiFi.ClientControls;
using UnityEngine.SceneManagement;

public class C : MonoBehaviour {

    public static C c;
    public Data data;
    public string playerName;
    public Text playerNameText;
    public Text logText;
    public Text playerNumberText;
    public Text yourTurnText;
    public GameObject pairWithDiscardPileButton;
    public GameObject challengeUI;
    public GameObject challengeTurnUI;
    public Transform topStacksTran;
    public CardClient[] cards;
    public int[] cardIds;
    public int[] topStacks;
    public List<int> cardsSelected;
    public bool discarding;
    public bool isTurn;
    public int currentDiscard;
    public bool canMatchWithDiscard;
    public float wait;
    public int pnum;
    public bool gaveUp;

    //challenging (get from server)
    public int wasChallengeTurn; // 0 - is not turn, 1 - is turn
    public int isChallengeTurn; // 0 - is not turn, 1 - is turn
    public int challengeType;
    
    //start a challenge
    public bool isChallenging;
    public int playerToChallenge;
    public int cardToUseInChallenge;

    //bc and data
    public StringDataClientController dcNewPair;
    public StringDataClientController dcChallenge;
    public IntDataClientController dcDiscardCard;
    public IntDataClientController dcCardToChallenge;
    public BoolDataClientController dcGiveUp;

    // Use this for initialization
    void Start () {
        Application.targetFrameRate = 60;
        c = GetComponent<C>();
        playerNameText.text = playerName;
        StartCoroutine(SlowUpdate());
        playerToChallenge = -1;
        cardToUseInChallenge = -1;
        dcCardToChallenge.setValue(-1);
        dcGiveUp.setValue(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (wait > 0) return;
        if (Input.GetKeyDown(KeyCode.Backspace)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (challengeTurnUI.activeSelf) {
            if (isChallengeTurn == 0) {
                challengeTurnUI.SetActive(false);
            }
        }
        if (isChallenging) {
            if (playerToChallenge != -1 && cardToUseInChallenge != -1) {
                var str = "";
                str += playerToChallenge;
                str += cardToUseInChallenge;
                dcChallenge.setValue(str);
                challengeType = cardIds[cardToUseInChallenge];
                cards[cardToUseInChallenge].TakeCard();
                playerToChallenge = -1;
                cardToUseInChallenge = -1;
                isChallenging = false;
                wait = 1f;
            }
        }
        if (isChallengeTurn == 1) {
            if (cardToUseInChallenge != -1) {
                dcCardToChallenge.setValue(cardToUseInChallenge);
                cards[cardToUseInChallenge].TakeCard();
                playerToChallenge = -1;
                cardToUseInChallenge = -1;
            }
        }
    }

    void OnGUI() {
        int i = 0;
        GUI.Label(new Rect(10, 10 + i * 15, 500, 20), "Pnum: " + pnum); i++;
        GUI.Label(new Rect(10, 10 + i * 15, 500, 20), "isChallengeTurn: " + isChallengeTurn); i++;
        GUI.Label(new Rect(10, 10 + i * 15, 500, 20), "challengeType: " + challengeType); i++;
        GUI.Label(new Rect(10, 10 + i * 15, 500, 20), "isChallenging: " + isChallenging); i++;
        GUI.Label(new Rect(10, 10 + i * 15, 500, 20), "playerToChallenge: " + playerToChallenge); i++;
        GUI.Label(new Rect(10, 10 + i * 15, 500, 20), "cardToUseInChallenge: " + cardToUseInChallenge); i++;
        GUI.Label(new Rect(10, 10 + i * 15, 500, 20), "wait: " + wait); i++;
        GUI.Label(new Rect(10, 10 + i * 15, 500, 20), "discarding: " + discarding); i++;
        GUI.Label(new Rect(10, 10 + i * 15, 500, 20), "cardid1: " + cardIds[0]); i++;
        GUI.Label(new Rect(10, 10 + i * 15, 500, 20), "cardid2: " + cardIds[1]); i++;
        GUI.Label(new Rect(10, 10 + i * 15, 500, 20), "cardid3: " + cardIds[2]); i++;
        GUI.Label(new Rect(10, 10 + i * 15, 500, 20), "cardid4: " + cardIds[3]); i++;
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
                        if (cardIds[cardsSelected[0]] == currentDiscard || currentDiscard < 3 || cardIds[cardsSelected[0]] < 3) { //can match with discard
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

            if (challengeType == 0) {
                for (var i = 0; i < 4; i++) {
                    if (!cards[i].inHand && cardIds[i] > 0) {
                        cards[i].StartReceive();
                    }
                }
            }
            yield return new WaitForSeconds(.15f);
        }
    }

    public void HandUpdated(StringBackchannelType str) {
        if (wait > 0) return;
        if (str.STRING_VALUE != null) {
            //Debug.Log(str.STRING_VALUE);
            string handStr = str.STRING_VALUE;
            for (var i = 0; i < 4; i++) {
                cardIds[i] = handStr[i] - 65;
            }
            currentDiscard = handStr[4] - 65;
            pnum = int.Parse(handStr.Substring(5, 1));
            var playerTurn = int.Parse(handStr.Substring(6, 1));
            for (var i = 0; i < 4; i++) { //top stacks
                topStacks[i] = handStr[7 + i] - 65;
            }
            challengeType = handStr[11] - 65;
            wasChallengeTurn = isChallengeTurn;
            isChallengeTurn = int.Parse(handStr.Substring(12, 1));
            if (isChallengeTurn == 1 && wasChallengeTurn == 0) {
                dcCardToChallenge.setValue(-1);
                dcGiveUp.setValue(false);
            } wasChallengeTurn = isChallengeTurn;

            if (isChallengeTurn == 1 && wait == 0) {
                StartChallengeTurn();
            } 
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

    public void StartChallenge() {
        challengeUI.transform.Find("ChallengeText").GetComponent<Text>().text = "Who Do You Want to Challenge?";
        playerToChallenge = -1;
        cardToUseInChallenge = -1;
        for (var i = 0; i < 4; i++) { //set up Challenge screen - compare slots
            topStacksTran.GetChild(i).GetChild(2).gameObject.SetActive(true);
            for (var j = 0; j < 4; j++) { //my hand = j
                topStacksTran.GetChild(i).GetChild(0).GetComponent<Image>().sprite = data.cardSprites[topStacks[i]];
                if (cardIds[j] > 0 && topStacks[i] > 0 && i != pnum) {
                    if (topStacks[i] == cardIds[j] || cardIds[j] < 3) { //unblock card - can challenge
                        topStacksTran.GetChild(i).GetChild(2).gameObject.SetActive(false);
                    }
                }
            }
        }
        challengeUI.SetActive(true);
        isChallenging = true;
    }

    public void PickCardForChallenge() {
        challengeUI.transform.Find("ChallengeText").GetComponent<Text>().text = "Which Card Do You Want to Challenge With?";
        for (var i = 0; i < 4; i++) { //set up Challenge screen - compare slots
            topStacksTran.GetChild(i).GetChild(2).gameObject.SetActive(true);
            topStacksTran.GetChild(i).GetChild(0).GetComponent<Image>().sprite = data.cardSprites[cardIds[i]];
            if (cardIds[i] > 0) {
                if (cardIds[i] == topStacks[playerToChallenge] || cardIds[i] < 3) {
                    topStacksTran.GetChild(i).GetChild(2).gameObject.SetActive(false);
                }
            }
        }
    }

    public void CancelChallenge() {
        challengeUI.SetActive(false);
        isChallenging = false;
    }

    public void GiveUpChallenge() {
        challengeTurnUI.SetActive(false);
        dcGiveUp.setValue(true);
        wait = 1f;
    }

    public void StartChallengeTurn() {
        if (!challengeTurnUI.activeSelf) {
            cardToUseInChallenge = -1;
            for (var i = 0; i < 4; i++) { //set up Challenge screen - compare slots
                challengeTurnUI.transform.GetChild(0).GetChild(i).GetChild(1).gameObject.SetActive(true);
                if (cards[i].inHand) {
                    challengeTurnUI.transform.GetChild(0).GetChild(i).GetChild(0).GetComponent<Image>().enabled = true;
                    challengeTurnUI.transform.GetChild(0).GetChild(i).GetChild(0).GetComponent<Image>().sprite = data.cardSprites[cardIds[i]];
                    if (cardIds[i] == challengeType || cardIds[i] == 1 || cardIds[i] == 2) { challengeTurnUI.transform.GetChild(0).GetChild(i).GetChild(1).gameObject.SetActive(false); }
                } else {
                    challengeTurnUI.transform.GetChild(0).GetChild(i).GetChild(0).GetComponent<Image>().enabled = false;
                }
            }
            challengeTurnUI.SetActive(true);
            dcChallenge.setValue(""); //reset inputs
            wait = 1f;
        }
    }

    public void GotMessageConfirmation(BoolBackchannelType b) {
        RefreshClient();
    }

    public void RefreshClient() {
        foreach (CardClient card in cards) {
            if (card.inHand) {
                card.selected = false;
                card.StartMoveToPosition(card.startPos);
            }
        }
        challengeUI.SetActive(false);
        discarding = false;
        cardsSelected.Clear();
        dcDiscardCard.setValue(0);
        dcCardToChallenge.setValue(-1);
        dcGiveUp.setValue(false);
        dcNewPair.setValue("0000");
    }
}
