using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBox : MonoBehaviour {
    [SerializeField]
    LayerMask playerMask;

    // Update is called once per frame
    void Update() {
        DetectNearByPlayer();
    }

    private bool DetectNearByPlayer() {
        Debug.Log("We are running");
        var colliders = Physics.OverlapSphere(transform.position, 5f, playerMask);
        foreach (var collider in colliders) {
            return true;
        }
        return false;
    }
}
