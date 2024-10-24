using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class Turret : MonoBehaviour {

    [SerializeField]
    GameObject turretHead;
    [SerializeField]
    GameObject turretRotator;
    [SerializeField]
    GameObject turretBody;

    private readonly int headRotationSpeed = 2;


    // Update is called once per frame
    void Update() {
        CheckIfPlayerIsNearBy();
    }

    private void CheckIfPlayerIsNearBy() {
        if ((transform.position - FirstPersonController.Instance.GetPosition()).magnitude < 15.0f) {
            RotateTurretHead();
        }
    }

    private void RotateTurretHead() {
        Vector3 dir = FirstPersonController.Instance.GetPosition() - turretHead.transform.position;
        Quaternion rot = Quaternion.LookRotation(dir);
        // slerp to the desired rotation over time
        turretHead.transform.rotation = Quaternion.Slerp(turretHead.transform.rotation, rot, headRotationSpeed * Time.deltaTime);
    }
}
