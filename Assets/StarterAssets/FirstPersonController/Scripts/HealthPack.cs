using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class HealthPack : MonoBehaviour, IInteractObject {
    public void Interact() {
        FirstPersonController.Instance.GiveMaxHealth();
        Debug.Log("Interacting with healthpack");
    }
}
