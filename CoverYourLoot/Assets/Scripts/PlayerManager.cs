using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyWiFi.Core;
using UnityEngine.UI;
using EasyWiFi.ServerBackchannels;



[System.Serializable]
public struct Stack {
    public int type;
    public int gold;
    public int silver;
    public int count;
}

public class PlayerManager : MonoBehaviour {

    public bool AI;
    public bool AIDone;
    public List<Stack> playerStack = new List<Stack>();
    public int p;
    public BoolServerBackchannel bcMessageConfirmation;
    bool discarding;
    Server c;
    public int totalWorth;
    public int cardToAttackWith;
    public bool inChallenge;
    public int challengeType;
    public bool isChallengeTurn;

    // Use this for initialization
    void Start () {
        c = Server.c;
        if (AI) StartCoroutine(AICor());
        StartCoroutine(TotalUpWorth());
    }
	
	// Update is called once per frame
	void Update () {

	}

    IEnumerator AICor() {
        c.playerNames[p].text = "AI";
        yield return new WaitForSeconds(6f);
        while (AI) {
            if (isChallengeTurn) {
                yield return new WaitForSeconds(Random.Range(4f, 7f));
                for (var i = 0; i < 4; i++) {
                    if (Server.playerHand[p,i] == Server.c.challengeType || Server.playerHand[p, i] == 1 || Server.playerHand[p, i] == 2) {
                        cardToAttackWith = i + 1;
                        break;
                    }
                } if (cardToAttackWith == 0) { //no options, forfeit
                    cardToAttackWith = 5;
                }
            }
            if (c.playerTurn == p && !AIDone) {
                if (Server.c.AIWaitTime) yield return new WaitForSeconds(Random.Range(2.5f,4f));
                else yield return new WaitForSeconds(.15f);
                //Try to make a pair
                Vector2 pair = new Vector2(0, 0);
                for (var i = 0; i < 4; i++) {
                    for (var j = 0; j < 4; j++) {
                        if (i != j && Server.playerHand[p, i] == Server.playerHand[p, j]) { //found pair
                            pair.x = i; pair.y = j;
                        }
                    }
                }
                if (pair != Vector2.zero) { //pair was found
                    CreatePairGeneration(pair);
                    AIDone = true;
                } else { //discard card instead
                    StartCoroutine(DiscardCardAnim(1, p));
                    AIDone = true;
                }
            }
            yield return null;
        }
    }

    IEnumerator TotalUpWorth() {
         while (true) {
            totalWorth = 0;
            foreach(Stack s in playerStack) {
                totalWorth += s.count * c.data.worth[s.type];
                totalWorth += s.gold * 50000;
                totalWorth += s.silver * 25000;
            }
            var str = totalWorth.ToString("N0");
            c.playerWorth[p].text = str;
            yield return new WaitForSeconds(1f);
        }
    }

    public void DiscardCard(IntBackchannelType i) {
        if (c.playerTurn == p && !discarding) {
            if (i.INT_VALUE <= 0) return;
            StartCoroutine(DiscardCardAnim(i.INT_VALUE, i.logicalPlayerNumber));
        }
    }
    IEnumerator DiscardCardAnim(int i, int p) {
        discarding = true;
        var animCard = Instantiate(c.prefabs[0], c.canvas.transform);
        animCard.transform.position = c.playerTargetPos[p].position;
        animCard.GetComponent<AnimatedCard>().Set(c.discardPileTran.position, 30, 0, Server.playerHand[p, i - 1]);
        while (animCard != null) {
            yield return null;
        }
        c.AddToDiscard(Server.playerHand[p, i - 1]);
        DrawCard(i - 1,.4f);
        StartSendMessageConfirmation();
        c.NextTurn();
        discarding = false;
    }

    public void StartChallenge(StringBackchannelType chalStr) {
        if (chalStr != null) {
            var s = chalStr.STRING_VALUE;
            if (s == "") { cardToAttackWith = 0;  return; }
            cardToAttackWith = int.Parse(s.Substring(1, 1))+1;
            var victim = int.Parse(s.Substring(0, 1));
            if (!inChallenge) {
                StartCoroutine(Server.c.Challenge(victim, cardToAttackWith, p,Server.c.pm[victim].playerStack[Server.c.pm[victim].playerStack.Count-1].type));
                //cardToAttackWith = 0;
            } 
        }
    }

    public void CreatePair(StringBackchannelType s) {
        if (c.playerTurn == p) {
            if (s.STRING_VALUE != null) {
                if (s.STRING_VALUE == "0000") return;
                //Debug.Log(s.STRING_VALUE);
                if (s.STRING_VALUE.Length > 4) return;
                Vector2 slots = new Vector2(-1,-1);
                for (var i = 0; i < 4; i++) {
                    if (s.STRING_VALUE[i] == char.Parse("1")) {
                        if (slots.x == -1) slots.x = i;
                        else slots.y = i;
                    }
                }
                CreatePairGeneration(slots);
            }
        }
    }

    public void CreatePairGeneration(Vector2 slots) {
        var newStackImg = c.playerPanels[p].transform.GetChild(0).GetChild(0);
        newStackImg.transform.SetSiblingIndex(3);
        switch (p) {
            case 0: newStackImg.GetComponent<Animate>().offset = Vector3.down * 400; break;
            case 1: newStackImg.GetComponent<Animate>().offset = Vector3.left * 400; break;
            case 2: newStackImg.GetComponent<Animate>().offset = Vector3.up * 400; break;
            case 3: newStackImg.GetComponent<Animate>().offset = Vector3.right * 400; break;
        }
        var img = newStackImg.GetComponent<Image>();
        img.enabled = true;
        bool typeSet = false;
        Stack newStack = new Stack();
        int count = 0;
        for (var i = 0; i < 4; i++) {
            if (slots.x == i || slots.y == i) {
                count++;
                var ID = Server.playerHand[p, i];
                //if (!sprSet || ID > 2) { img.sprite = c.data.cardSprites[ID]; sprSet = true; }
                if (ID == 1) newStack.gold++;
                else if (ID == 2) newStack.silver++;
                else newStack.count++;
                if (!typeSet || ID > 2) { newStack.type = ID; typeSet = true; }
                DrawCard(i, 1f + count * .15f);
            }
            if (i == 3 && count == 1) { //use discard pile
                var IDdisc = c.discardPile[c.discardPile.Count - 1];
                c.CreateAnimatedCard(c.discardPileTran.position, newStackImg.transform.position, 15, 0, IDdisc);
                //if (!sprSet || IDdisc > 2) { img.sprite = c.data.cardSprites[IDdisc]; sprSet = true; }
                if (IDdisc == 1) newStack.gold++;
                else if (IDdisc == 2) newStack.silver++;
                else newStack.count++;
                if (!typeSet || IDdisc > 2) { newStack.type = IDdisc; typeSet = true; }
                c.discardPile.RemoveAt(c.discardPile.Count - 1);
                if (c.discardPile.Count == 0) c.discardPileTran.GetComponent<Image>().enabled = false;
                else c.discardPileTran.GetComponent<Image>().sprite = c.data.cardSprites[c.discardPile[c.discardPile.Count - 1]];
            }
            img.sprite = c.data.cardSprites[newStack.type];
        }
        playerStack.Add(newStack);
        StartSendMessageConfirmation();
        c.NextTurn();
    }

    public void UpdateStackVisual() {
        for (var i = 0; i < 4; i++) {
            var stackVisual = c.playerPanels[p].transform.GetChild(0).GetChild(3-i).GetComponent<Image>();
            if (playerStack.Count > i) {
                var stack = playerStack[playerStack.Count-1-i];
                stackVisual.enabled = true;
                stackVisual.sprite = c.data.cardSprites[stack.type];
            }
        }
    }

    public void DrawCard(int slot, float delay = 0) {
        StartCoroutine(DrawCardCor(slot, delay));
    }
    IEnumerator DrawCardCor(int slot, float delay) {
        yield return new WaitForSeconds(delay);
        Server.playerHand[p, slot] = c.deck[0];
        c.deck.RemoveAt(0);
        var inst = Instantiate(c.prefabs[0], c.canvas.transform);
        inst.transform.position = c.deckTran.position;
        inst.GetComponent<AnimatedCard>().Set(c.playerTargetPos[p].position, 25, 15);
    }

    public void StartTurn() {
        if (AI) AIDone = false;
        StartCoroutine(SendMessageConfirmation());
    }

    public void StartSendMessageConfirmation() {
        StartCoroutine(SendMessageConfirmation());
    }
    IEnumerator SendMessageConfirmation() {
        //send bool telling client we got message and end their turn
        bcMessageConfirmation.setValue(true);
        yield return new WaitForSeconds(.25f);
        //turn off sending bool
        bcMessageConfirmation.setValue(false);
    }

}
