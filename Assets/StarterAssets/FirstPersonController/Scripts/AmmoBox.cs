using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class AmmoBox : MonoBehaviour, IInteractObject {
    [SerializeField]
    GameObject AmmoFloatingIcon;

    private bool AmmoBoxHidden = false;
    private float respawnTime = 10f;
    private float pickUpTime;

    // Update is called once per frame
    void Update() {
        RespawnAmmo();
    }

    public void Interact() {
        if (AmmoBoxHidden == false) {
            FirstPersonController.Instance.GetGun().GiveMaxAmmo();
            HideAmmo();
            AmmoBoxHidden = true;
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
