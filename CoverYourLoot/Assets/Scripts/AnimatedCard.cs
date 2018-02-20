using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedCard : MonoBehaviour {

    public Vector3 targetPos;
    public float speed;
    public float spin = 0f;

	// Use this for initialization
	void Start () {
        
	}

    public void Set(Vector3 targPos, float spd, float spinf,int skin = 0) {
        targetPos = targPos;
        speed = spd;
        spin = spinf;
        if (skin > 0) GetComponent<Image>().sprite = Server.c.data.cardSprites[skin];
    }
	
	// Update is called once per frame
	void Update () {
        transform.position += (targetPos - transform.position).normalized * speed * 60 * Time.deltaTime;
        transform.eulerAngles += new Vector3(0, 0, spin * Time.deltaTime * 60);
        if (Vector3.Distance(transform.position, targetPos) < 32) {
            Destroy(gameObject);
        }
	}
}
