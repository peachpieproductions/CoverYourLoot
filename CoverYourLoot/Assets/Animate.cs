using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animate : MonoBehaviour {

    internal Vector3 startPos;
    internal bool goToTarget;
    internal Vector3 targetPos;
    public Vector3 offset;
    public Vector3 worldOffset;

    private void Awake() {
        startPos = transform.localPosition;
    }

    private void Update() {
        if (!goToTarget) {
            if (worldOffset != Vector3.zero) {
                transform.position = worldOffset;
                worldOffset = Vector3.zero;
            }
            if (offset != Vector3.zero) {
                transform.localPosition += offset;
                offset = Vector3.zero;
            }
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, .1f * Time.deltaTime * 60);
        } else {
            transform.position = Vector3.Lerp(transform.position, targetPos, .1f * Time.deltaTime * 60);
        }

    }


}
