using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour {

    private Slider healthSlider;

    public void Start() {
        healthSlider = GetComponent<Slider>();
    }

    public void SetMaxHealth(int maxHealth) {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }

    public void SetHealth(int newHealth) {
        healthSlider.value = newHealth;
    }
}
