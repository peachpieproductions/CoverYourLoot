using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardClient : MonoBehaviour {

    public int slot;
    public int id;
    public bool inHand;
    internal Vector3 startPos;
    public bool faceUp;
    Image image;
    public bool selected;
    public bool isChallengeCard;

    private void Awake() {
        startPos = transform.localPosition;
        slot = transform.parent.GetSiblingIndex();
    }

    // Use this for initialization
    void Start () {
        //C.c.cardIds[slot] = Random.Range(1,13);
        image = GetComponent<Image>();
        if(!isChallengeCard) image.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    public void StartReceive() {
        StopAllCoroutines();
        StartCoroutine(Receive());
    }

    public void Clicked() {
        if (C.c.isChallengeTurn == 1) {
            if (C.c.cardToUseInChallenge == -1) {
                C.c.playerToChallenge = slot;
                C.c.challengeTurnUI.SetActive(false);
            }
        }
        else if (isChallengeCard) {
            if (C.c.playerToChallenge == -1) {
                C.c.playerToChallenge = slot;
                C.c.PickCardForChallenge();
            } else if (C.c.cardToUseInChallenge == -1) {
                C.c.cardToUseInChallenge = slot;
                C.c.challengeUI.SetActive(false);
                //send off card HERE
            }
        }
        else if (C.c.cardsSelected.Count < 2 || selected) {
            if (!selected) { //check for matching card
                C.c.cardsSelected.Add(slot);
                
            }
            selected = !selected;
            if (selected) {
                StartMoveToPosition(startPos + Vector3.up * 50);
            } else {
                C.c.cardsSelected.Remove(slot);
                StartMoveToPosition(startPos);
            }
        }
    }

    public void TakeCard() {
        C.c.cardIds[slot] = 0;
        StartMoveToPosition(C.c.cards[C.c.cardsSelected[0]].startPos + Vector3.up * 500);
        inHand = false;
        selected = false;
    }


    Coroutine movingCor;
    public void StartMoveToPosition(Vector3 pos) {
        if (movingCor != null) StopCoroutine(movingCor);
        movingCor = StartCoroutine(MoveToPosition(pos));
    }
    IEnumerator MoveToPosition(Vector3 pos, float spd = 1) {
        while (transform.localPosition != pos) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, pos, .1f * spd);
            yield return null;
        }
    }

    IEnumerator Receive() {
        faceUp = false;
        selected = false;
        image.sprite = C.c.data.cardSprites[0];
        inHand = true;
        yield return null;
        image.enabled = true;
        transform.localPosition = startPos + Vector3.up * 500;
        transform.localScale = new Vector3(-1, 1, 1);
        AudioManager.am.PlaySound(0);
        while ((transform.localPosition - startPos).magnitude > .1f) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, .075f);
            yield return null;
        }
        id = C.c.cardIds[slot];
        AudioManager.am.PlaySound(1);
        while (transform.localScale.x < 1) {
            if (transform.localScale.x >= 0 && !faceUp) { faceUp = true; image.sprite = C.c.data.cardSprites[id]; }
            transform.localScale += new Vector3(.1f, 0, 0);
            yield return null;
        }
    }

}
