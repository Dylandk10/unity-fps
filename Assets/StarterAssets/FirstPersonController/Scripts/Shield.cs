using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {
    [SerializeField]
    ColorSchemeSO colorScheme;

    public string GetColor() {
        return colorScheme.color;
    }
}
