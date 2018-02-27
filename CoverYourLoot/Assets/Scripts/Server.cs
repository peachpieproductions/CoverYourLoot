using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyWiFi.ServerBackchannels;
using EasyWiFi.ServerControls;
using EasyWiFi.Core;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Server : MonoBehaviour {

    
    public static Server c;
    public bool AIWaitTime;
    public Data data;
    public Transform deckTran;
    public Transform discardPileTran;
    public GameObject[] prefabs;
    public GameObject canvas;
    public GameObject challengePanel;
    public static int[,] playerHand = new int[4, 4];
    public GameObject[] playerManagers = new GameObject[4];
    public GameObject[] playerPanels = new GameObject[4];
    public Transform[] playerTargetPos = new Transform[4];
    public Text[] playerNames = new Text[4];
    public Text[] playerWorth = new Text[4];
    public StringServerBackchannel[] bcHandStr = new StringServerBackchannel[4];
    public List<int> deck;
    public List<int> discardPile;
    public PlayerManager[] pm = new PlayerManager[4];
    public int playerTurn;
    public int challengeType;

	// Use this for initialization
	void Awake () {
        c = GetComponent<Server>();
        canvas = GameObject.Find("Canvas");
	}

    private void Start() {
        Initialize();
        StartCoroutine(DealCards());
        StartCoroutine(UpdateHands());
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Backspace)) { //Restart game
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (Input.GetKeyDown(KeyCode.N)) { NextTurn(); } 
	}

    public void Initialize() {

        //build deck
        BuildDeck();

        //Set up Player Managers 
        for (var i = 0; i < 4; i++) {
            bcHandStr[i] = pm[i].GetComponent<StringServerBackchannel>();
        }
    }

    public void BuildDeck() {        
        var i = 0;
        for (i = 0; i < 4; i++) { deck.Add(1); } //Huge Sack x 4 - 50,000
        for (i = 0; i < 8; i++) { deck.Add(2); } //Big Gold Sack x 8 - 25,000
        for (i = 0; i < 8; i++) { deck.Add(3); } //Diamond x 8 - 20,000

        //for (i = 0; i < 90; i++) { deck.Add(3); } //DELETE _ FOR TESTING - ONLY DIAMONDS

        for (var j = 4; j < 13; j++) {
            for (i = 0; i < 10; i++) { deck.Add(j); } //Other Cards x 10
        }

        //shuffle
        for (i = 0; i < deck.Count; i++) {
            int temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public void NextTurn() {
        playerTurn++;
        if (playerTurn > 3) playerTurn = 0;
        pm[playerTurn].StartTurn();
    }

    public IEnumerator Challenge(int victim, int cardSlot, int p, int type) {
Debug.Log("Start Challenge _____");
        foreach (Image img in challengePanel.transform.GetChild(0).GetComponentsInChildren<Image>()) { if (img.transform.GetSiblingIndex() > 0) img.enabled = false; }
        foreach (Image img in challengePanel.transform.GetChild(1).GetComponentsInChildren<Image>()) { if (img.transform.GetSiblingIndex() > 0) img.enabled = false; }
        challengeType = type;
        challengePanel.SetActive(true);
        pm[p].cardToAttackWith = cardSlot;
        int[] P = new int[2];
        P[0] = p;
        P[1] = victim;
Debug.Log("victim: " + victim);
Debug.Log("attacker: " + p);
        pm[p].inChallenge = true;
        pm[victim].inChallenge = true;
        int round = 0;
        int turn = 0;
        //int cardSlotSelected = cardSlot;
        while (true) {
            if (pm[P[turn]].cardToAttackWith != 0) {
Debug.Log("P" + P[turn] + " used slot " + pm[P[turn]].cardToAttackWith);
                if (pm[P[turn]].cardToAttackWith == 5) { //forfeit
                    if (P[turn] == victim) { //victim forfeited
                        //animate cards going to attacker
Debug.Log("victim forfeited");
                        var victimStack = pm[victim].playerStack[pm[victim].playerStack.Count - 1];
                        pm[p].playerStack.Add(victimStack);
                        pm[victim].playerStack.RemoveAt(pm[victim].playerStack.Count - 1);
                        pm[p].UpdateStackVisual();
                        pm[victim].UpdateStackVisual();
                    } else { //attacker forfeited
Debug.Log("attacker forfeited");
                        //animate cards going to victim
                    }
                    pm[p].cardToAttackWith = 0;
                    pm[victim].cardToAttackWith = 0;
                    pm[p].inChallenge = false;
                    pm[victim].inChallenge = false;
                    challengePanel.SetActive(false);
                    challengeType = 0;
                    NextTurn();
                    yield break;
                }
                //turn on card, set graphic
                challengePanel.transform.GetChild(turn).GetChild(round).GetComponent<Image>().enabled = true;
                challengePanel.transform.GetChild(turn).GetChild(round).GetComponent<Image>().sprite = data.cardSprites[playerHand[P[turn], pm[P[turn]].cardToAttackWith-1]];
                playerHand[P[turn], pm[P[turn]].cardToAttackWith - 1] = 0;
                pm[P[turn]].DrawCard(pm[P[turn]].cardToAttackWith - 1);

                if (round == 0) {
                    challengePanel.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = data.cardSprites[challengeType];
                }

                //add to stack
                var stack = pm[victim].playerStack[pm[victim].playerStack.Count - 1];
                if (challengeType == 1) stack.gold++;
                else if (challengeType == 2) stack.silver++;
                else stack.count++;
                pm[victim].playerStack[pm[victim].playerStack.Count - 1] = stack;

                pm[P[turn]].cardToAttackWith = 0;
                pm[P[turn]].isChallengeTurn = false;
                turn++; if (turn > 1) turn = 0;
                if (turn == 1) round++;
                pm[P[turn]].isChallengeTurn = true;
            }
            yield return null;
        }
    }

    IEnumerator UpdateHands() {
        while (true) {
            for (var i = 0; i < 4; i++) {
                string str = "";
                for (var j = 0; j < 4; j++) { //build hand string
                    str += data.alphabet[playerHand[i, j]];
                }
                if (discardPile.Count > 0) str += data.alphabet[discardPile[discardPile.Count-1]]; //discardPile
                else str += "A"; //empty discard pile
                str += i; //player number
                str += playerTurn; //current turn
                for (var j = 0; j < 4; j++) { //Send Over Top Stacks
                    if (pm[j].playerStack.Count > 1) str += data.alphabet[pm[j].playerStack[pm[j].playerStack.Count-1].type];
                    else str += "A";
                }
                if (pm[i].inChallenge) str += challengeType; //challengeType
                else str += 0; //not in challenge
                if (pm[i].inChallenge && pm[i].isChallengeTurn) str += 1; //if in challenge, and is players turn to send card
                else str += 0;


                //send string to client
                bcHandStr[i].setValue(str);
            }
            yield return new WaitForSeconds(.15f);
        }
    }

    public void CreateAnimatedCard(Vector3 spawnPos, Vector3 targPos, float spd, float spin, int skin = 0) {
        var inst = Instantiate(prefabs[0], canvas.transform);
        inst.transform.position = spawnPos;
        inst.GetComponent<AnimatedCard>().Set(targPos, spd, spin,skin);
    }

    IEnumerator DealCards() {
        yield return new WaitForSeconds(2f);
        for (var i = 0; i < 4; i++) {
            for (var j = 0; j < 4; j++) {
                var inst = Instantiate(prefabs[0], canvas.transform);
                inst.transform.position = deckTran.position;
                AudioManager.am.PlaySound(0);
                playerHand[j, i] = deck[0];
                deck.RemoveAt(0);
                switch (j) {
                    case 0: inst.GetComponent<AnimatedCard>().Set(inst.transform.position + Vector3.down * 1000, 15, 8); break;
                    case 1: inst.GetComponent<AnimatedCard>().Set(inst.transform.position + Vector3.left * 1000, 15, 8); break;
                    case 2: inst.GetComponent<AnimatedCard>().Set(inst.transform.position + Vector3.up * 1000, 15, 8); break;
                    case 3: inst.GetComponent<AnimatedCard>().Set(inst.transform.position + Vector3.right * 1000, 15, 8); break;
                }
                yield return new WaitForSeconds(.2f);
                if (i == 3 && j == 3) { //last card, start discard pile
                    AddToDiscard(deck[0]);
                    deck.RemoveAt(0);
                    AudioManager.am.PlaySound(0);
                }
            }
        }
    }

    public void AddToDiscard(int ID) {
        discardPileTran.GetComponent<Image>().enabled = true;
        discardPileTran.GetComponent<Image>().sprite = data.cardSprites[ID];
        discardPile.Add(ID);
    }

}
