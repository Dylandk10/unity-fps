using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class AmmoBox : MonoBehaviour {
    [SerializeField]
    LayerMask playerMask;

    [SerializeField]
    GameObject AmmoFloatingIcon;

    private bool AmmoBoxHidden = false;
    private float respawnTime = 10f;
    private float pickUpTime;

    // Update is called once per frame
    void Update() {
        PickUpAmmo();
        RespawnAmmo();
    }

    private bool DetectNearByPlayer() {
        //if the player is by the ammobox the colliders length will be greater than 0;
        var colliders = Physics.OverlapSphere(transform.position, 5f, playerMask);
        if (colliders.Length > 0) {
            return true;
        }
        return false;
    }

    private void PickUpAmmo() {
        var _inputs = FirstPersonController.Instance.GetInputs();
        if (_inputs.interact && DetectNearByPlayer() && AmmoBoxHidden == false) {
            FirstPersonController.Instance.GetGun().GiveMaxAmmo();
            HideAmmo();
            AmmoBoxHidden = true;
            _inputs.interact = false;
            pickUpTime = Time.time;
        }
    }

    public void HideAmmo() {
        AmmoFloatingIcon.SetActive(false);
    }

    public void ShowAmmo() { 
        AmmoFloatingIcon.SetActive(true);
    }

    private void RespawnAmmo() {
        if (AmmoBoxHidden == true && pickUpTime + respawnTime < Time.time) {
            ShowAmmo();
            AmmoBoxHidden = false;
        }
    }
}
