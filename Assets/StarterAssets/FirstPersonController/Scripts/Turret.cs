using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;

public class Turret : MonoBehaviour {

    [SerializeField]
    GameObject turretHead;
    [SerializeField]
    GameObject turretBarel;
    [SerializeField]
    GameObject turretRotator;
    [SerializeField]
    GameObject turretBody;

    private readonly int headRotationSpeed = 5;

    [SerializeField]
    private bool AddBulletSpread = false;
    [SerializeField]
    private Vector3 BulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    private Transform BulletSpawnPointOne;
    [SerializeField]
    private Transform BulletSpawnPointTwo;
    [SerializeField]
    private ParticleSystem ImpactParticleSystem;
    [SerializeField]
    private TrailRenderer BulletTrail;
    [SerializeField]
    private float ShootDelay = 0.5f;
    [SerializeField]
    private LayerMask Mask;
    [SerializeField]
    private float BulletSpeed = 100f;
    private float LastShootTime;
    private int difficulty = 100;
    private int diffcultyDelta = 30;

    //health and other shield
    private int health = 100;
    private readonly string[] shieldColors = {"Red", "Blue", "Green"};
    private int activeShieldIndex;

    //shield matterials
    [SerializeField]
    Material[] shieldMatterials;


    void Start() {
        activeShieldIndex = 0;
    }


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
        var holdingPos = FirstPersonController.Instance.GetPosition();
        holdingPos.y += 1.3f;
        Vector3 dir = holdingPos - turretHead.transform.position;
        Quaternion rot = Quaternion.LookRotation(dir);
        // slerp to the desired rotation over time
        turretHead.transform.rotation = Quaternion.Slerp(turretHead.transform.rotation, rot, headRotationSpeed * Time.deltaTime);
        CheckToShoot();
    }




    public void CheckToShoot() {
        if (LastShootTime + ShootDelay < Time.time) {

            ShootingSystem.Play();
            Vector3 direction = GetDirection();
            if (Physics.Raycast(turretHead.transform.position, direction, out RaycastHit hit, float.MaxValue, Mask)) {
                if (hit.transform.gameObject.CompareTag("Player")) {
                    FirstPersonController.Instance.TakeDamage(5, "Red");
                    ShootHit(hit);
                }
            }
            // this has been updated to fix a commonly reported problem that you cannot fire if you would not hit anything
            else {
                ShootMiss();
            }
        }
    }

    private void ShootHit(RaycastHit hit) {
        Vector3 gunRotate = hit.point - transform.position;
        BulletSpawnPointOne.transform.rotation = Quaternion.LookRotation(gunRotate);
        BulletSpawnPointTwo.transform.rotation = Quaternion.LookRotation(gunRotate);

        TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPointOne.position, Quaternion.identity);
        TrailRenderer trail2 = Instantiate(BulletTrail, BulletSpawnPointTwo.position, Quaternion.identity);

        StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));
        StartCoroutine(SpawnTrail(trail2, hit.point, hit.normal, true));//true here leaves hit mark if we hit tht turret don't give a hitmark

        LastShootTime = Time.time;
    }

    private void ShootMiss() {
        TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPointOne.position, Quaternion.identity);
        TrailRenderer trail2 = Instantiate(BulletTrail, BulletSpawnPointTwo.position, Quaternion.identity);

        StartCoroutine(SpawnTrail(trail, BulletSpawnPointOne.position + GetDirection() * 100, Vector3.zero, false));
        StartCoroutine(SpawnTrail(trail2, BulletSpawnPointTwo.position + GetDirection() * 100, Vector3.zero, false));

        LastShootTime = Time.time;
    }

    private Vector3 GetDirection() {
        Vector3 direction = turretHead.transform.forward;

        if (AddBulletSpread) {
            direction += new Vector3(
                Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
            );

            direction.Normalize();
        }

        return direction;
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, bool MadeImpact) {
        // This has been updated from the video implementation to fix a commonly raised issue about the bullet trails
        // moving slowly when hitting something close, and not
        Vector3 startPosition = Trail.transform.position;
        float distance = Vector3.Distance(Trail.transform.position, HitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0) {
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));

            remainingDistance -= BulletSpeed * Time.deltaTime;

            yield return null;
        }
        //Animator.SetBool("New Bool", false);
        //Trail.transform.position = HitPoint;
        //if (MadeImpact) {
        //    Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal));
        //}

        Destroy(Trail.gameObject, Trail.time);
    }

    public void TakeDamage(int damage, string color) {
        if (color == shieldColors[activeShieldIndex]) {
            Debug.Log("Turret Blocked!");
            return;
        }
        health -= damage;
        Debug.Log("Turret Hit!");
        ShouldSwitchcolor();
    }

    private int GetRandomNumber() {
        return Random.Range(0, difficulty);
    }

    public void ShouldSwitchcolor() {
        if (GetRandomNumber() < diffcultyDelta) {
            SwitchColors();
        }
    }

    private void SwitchColors() {
        subtask_ChangeActiveIndex();
        turretHead.GetComponent<Renderer>().material = shieldMatterials[activeShieldIndex];
        turretBarel.GetComponent<Renderer>().material = shieldMatterials[activeShieldIndex];
        turretRotator.GetComponent<Renderer>().material = shieldMatterials[activeShieldIndex];
        turretBody.GetComponent<Renderer>().material = shieldMatterials[activeShieldIndex];
    }

    private void subtask_ChangeActiveIndex() {
        if ((activeShieldIndex + 1) >= shieldColors.Length) { activeShieldIndex = 0; }
        else { activeShieldIndex += 1; }
    }

}
