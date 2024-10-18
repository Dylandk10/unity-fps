using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
public class CanvasController : MonoBehaviour {
    // Start is called before the first frame update
    private Text ammoDisplay;
    private const int LOW_AMMO_COUNT = 30;

    [SerializeField]
    public Text gunDisplay;

    [SerializeField]
    GameObject HitSomethingMarker;

    private void Awake() {
        ammoDisplay = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        DisplayGunAmmo();
        SetAmmoDisplayColor();
        ammoDisplay.text = FirstPersonController.Instance.GetGun().GetColor() + " : " + FirstPersonController.Instance.GetGun().GetCurrentAmmo();

    }

    private void SetAmmoDisplayColor() {
        if (FirstPersonController.Instance.GetGun().GetCurrentAmmoInt() <= LOW_AMMO_COUNT) {
            ammoDisplay.color = Color.red;
        }
        else {
            ammoDisplay.color = Color.white;
        }
    }

    private void DisplayGunAmmo() {
        GameObject[] guns = FirstPersonController.Instance.GetGuns();
        Gun blueGun = guns[0].GetComponent<Gun>();
        Gun redGun = guns[1].GetComponent<Gun>();
        Gun greenGun = guns[2].GetComponent<Gun>();
        gunDisplay.text =     "Blue: " + blueGun.GetCurrentAmmo() + "\n\n"
                            + "Red: " + redGun.GetCurrentAmmo() + "\n\n"
                            + "Green: " + greenGun.GetCurrentAmmo() + "\n\n";
    }
}
