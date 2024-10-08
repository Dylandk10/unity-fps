using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
public class CanvasController : MonoBehaviour {
    // Start is called before the first frame update
    public Text ammoDisplay;
    private const int LOW_AMMO_COUNT = 30;

    private void Awake() {
        ammoDisplay = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        SetAmmoDisplayColor();
        ammoDisplay.text = "Ammo: " + FirstPersonController.Instance.GetGun().GetCurrentAmmo();

    }

    private void SetAmmoDisplayColor() {
        if (FirstPersonController.Instance.GetGun().GetCurrentAmmoInt() <= LOW_AMMO_COUNT) {
            ammoDisplay.color = Color.red;
        }
        else {
            ammoDisplay.color = Color.white;
        }
    }
}
