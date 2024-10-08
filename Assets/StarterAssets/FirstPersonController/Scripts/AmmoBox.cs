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
        //if the player is by the ammobox the colliders length will be greater than 0;
        var colliders = Physics.OverlapSphere(transform.position, 5f, playerMask);
        if (colliders.Length > 0) {
            return true;
        }
        return false;
    }

    public void HideAmmo() {
        this.gameObject.SetActive(false);
    }

    public void ShowAmmo() { 
        this.gameObject.SetActive(true);
    }
}
