using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour {
    [SerializeField]
    private bool AddBulletSpread = false;
    [SerializeField]
    private Vector3 BulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    private Transform BulletSpawnPoint;
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
    [SerializeField]
    private Camera camera;

    private Animator Animator;
    private float LastShootTime;

    //magazine sizes
    private int maxAmmo = 100;
    private int currentAmmo;

    [SerializeField]
    private ColorSchemeSO colorScheme;

    [SerializeField]
    public Image selectImage;


    //sfx
    private AudioSource gunShotAudio;
    private readonly float[] pitchArray = { 0.85f, 0.95f, 1.01f, 1.15f };

    [SerializeField]
    public AudioClip changeWepon;

    private void Awake() {
        // Animator = GetComponent<Animator>();

        gunShotAudio = GetComponent<AudioSource>();
    }

    private void Start() {
        currentAmmo = maxAmmo;
    }

    public void Shoot() {
        if (LastShootTime + ShootDelay < Time.time && FirstPersonController.Instance.GunSwapComplete()) {
            //if no ammo return
            if (currentAmmo <= 0) { return; }

            // Animator.SetBool("New Bool", true);
            PlayGunAudio();
            ShootingSystem.Play();
            Vector3 direction = GetDirection();
            if (Physics.Raycast(camera.transform.position, direction, out RaycastHit hit, float.MaxValue, Mask)) {
                Vector3 gunRotate = hit.point - transform.position;
                BulletSpawnPoint.transform.rotation = Quaternion.LookRotation(gunRotate);

                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true)); //true here leaves hit mark if we hit tht turret don't give a hitmark
                Debug.Log(hit.collider.name);

                LastShootTime = Time.time;
            }
            // this has been updated to fix a commonly reported problem that you cannot fire if you would not hit anything
            else {
                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, BulletSpawnPoint.position + GetDirection() * 100, Vector3.zero, false));

                LastShootTime = Time.time;
            }
            
            //reduce ammo count
            currentAmmo -= 1;
        }
    }

    private Vector3 GetDirection() {
        Vector3 direction = camera.transform.forward;

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
        Trail.transform.position = HitPoint;
        if (MadeImpact) {
            Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal));
        }

        Destroy(Trail.gameObject, Trail.time);
    }


    public string GetCurrentAmmo() {
        return currentAmmo.ToString();
    }

    public int GetCurrentAmmoInt() {
        return currentAmmo;
    }

    public string GetMaxAmmo() { 
        return maxAmmo.ToString(); 
    }

    public void GiveMaxAmmo() {
        currentAmmo = maxAmmo;
    }

    public string GetColor() {
        return colorScheme.color;
    }

    public AudioClip GetWeponChangeAudio() {
        return changeWepon;
    }

    public void PlayGunAudio() {
        gunShotAudio.volume = 0.208f;
        int randomPitch = Random.Range(0, pitchArray.Length);
        gunShotAudio.pitch = pitchArray[randomPitch];
        gunShotAudio.PlayOneShot(gunShotAudio.clip);
    }

    public void PlayGenericSound(AudioClip sound) {
        gunShotAudio.PlayOneShot(sound);
    }

    public void PlayWeponChangeAudio() {
        gunShotAudio.volume = 0.9f;
        gunShotAudio.PlayOneShot(changeWepon);
    }

    public void SetDisplayActive(bool isActive) {
        selectImage.gameObject.SetActive(isActive);
    }
}
