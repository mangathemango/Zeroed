using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using UnityEditor.Callbacks;

using UnityEditor.SceneManagement;
public enum FireMode {
    Semi,
    Auto,
    Burst
}
public enum ManualMode {
    Bolt,
    Pump
}   
public enum ShotType {
    Single,
    Spread,
}
public enum AmmoType {
    _9x19mm,
    __45ACP,
    _5_7x28mm,
    _5_56x45mm,
    _7_62x51mmNATO,
    _12_Gauge,
}

public enum OpticType {
    None,
    x1,
    x2,
    x3,
    x4
}

// oh yeahs sure
public abstract class BaseGun : MonoBehaviour

{
    [Header("General")]
    public string gunName;
    public GameObject ammoType;
    public int ammoCapacity = 10;
    [Range (0.0f, 10.0f)]
    public float mass = 0.1f;
    [Range (10.0f, 1000.0f)]
    public float muzzleVelocity = 300.0f;
    [Range (1.0f, 10.0f)]
    public float soundSignature = 1.0f;
    [Range (0f, 1f)]
    public float triggerPullTimeSeconds = 0.2f;
    [Range (0f, 3f)]
    public float switchTimeSeconds = 1.0f;
    [Range (0f, 2f)]
    public float adsTimeSeconds = 0.3f;
    [Range (0f, 4f)]
    public float reloadTimeSeconds = 1.0f;
    [Range (0f, 3f)]
    public float chargingTime = 0.5f;
    public FireMode defaultFireMode = FireMode.Semi;
    


    [Header("Damage")]
    [Range (0f, 100f)]
    public float maxDamage = 10.0f;
    [Range (0f, 100f)]
    public float minDamage = 1.0f;
    [Range (0f, 100f)]
    public float maxDamageRange = 10.0f;
    [Range (0f, 100f)]
    public float minDamageRange = 100.0f;
    [Range (0f, 1f)]
    public float armorPenetrationPercent = 0f;
    [Range (1f, 10f)]
    public float armorDamageMultiplier = 0.1f;
    [Range (1f, 10f)]
    public float headshotMultiplier = 2.0f;

    [Header("Semi")]
    public bool hasSemiFire = false;
    public bool requiresChargingBetweenShots = false;
    

    [Header("Auto")]
    public bool hasAutoFire = false;
    [Range (100.0f, 1000.0f)]
    public float shotsPerMinute = 600.0f;

    
    [Header("Burst")]
    public bool hasBurstFire = false;
    public int burstSize = 0;
    public float burstRate = 600.0f;

    [Header("Spread")]
    public float pointFireSpreadMOA = 1.0f;
    public float adsSpreadMOA = 0.1f;
    public float pelletSpreadMOA = 2.0f;

    [Header("Recoil")]
    [Range (0.0f, 10.0f)]
    public float recoilY = 1.0f;
    [Range (0.0f, 10.0f)]
    public float recoilX = 1.0f;

    [Header("Attachments")]
    public OpticType Optics = OpticType.None;

    [Header("Melee")]
    public float meleeDamage = 10.0f;
    public float meleeRange = 2f;
    public float meleeKnockback = 1.0f;
    public float meleeStaggerTimeSeconds = 0.5f;

    [Header("References")]
    public Transform firePoint;
    public AudioClip deadTriggerSFX;
    public AudioClip disconnectorSFX;
    public AudioClip fireSFX;
    public AudioClip reloadSFX;
    public AudioClip chargeSFX;
    public AudioClip meleeMissSFX;
    public AudioClip meleeHitSFX;
#if DEBUG
    public PlayerMovement playerMovement;
    public Transform playerPosition;
    public AudioSource audioSource;
    public RotateAround rotateAround;
    public FireMode currentFireMode;
    public bool meleeReady = true;
    public bool autoFireReady = true;
    public bool semiFireReady = true;
    public bool burstFireReady = true;
    public int currentAmmoInMag;
    public int currentAmmoInChamber;
    public bool triggerPressed = false;
    public bool charging = false;
    public bool reloading = false;
    public bool aiming = false;
#else
    [System.NonSerialized] public Transform player;
    [System.NonSerialized] public AudioSource audioSource;
    [System.NonSerialized] public RotateAround rotateAround;
    [System.NonSerialized] public FireMode currentFireMode;
    [System.NonSerialized] public bool meleeReady = true;
    [System.NonSerialized] public bool autoFireReady = true;
    [System.NonSerialized] public bool semiFireReady = true;
    [System.NonSerialized] public bool burstFireReady = true;
    [System.NonSerialized] public int currentAmmoInMag;
    [System.NonSerialized] public int currentAmmoInChamber;
    [System.NonSerialized] public bool triggerPressed = false;
    [System.NonSerialized] public bool charging = false;
    [System.NonSerialized] public bool reloading = false;
    [System.NonSerialized] public bool aiming = false;
#endif

    [Header("Internal")]
    private bool aimCoroutineRunning = false;
    private bool pressTriggerCoroutineRunning = false;
    private int burstShotsFired = 0;

    protected virtual void Start()
    {
        // Setup References
        GameObject player = GameObject.Find("Player");
        playerPosition = player.transform;
        playerMovement = player.GetComponent<PlayerMovement>();

        rotateAround = gameObject.AddComponent<RotateAround>();
        rotateAround.target = playerPosition;
        rotateAround.offsetPosition = new Vector3(0, 0, -1);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Setup gun stuff
        currentFireMode = defaultFireMode;
        currentAmmoInMag = ammoCapacity - 1;
        currentAmmoInChamber = 1;
        setupFireModes();
    }


    // Update is called once per frame
    protected virtual void Update()
    {   
        LookAtCursor();
        if (triggerPressed) {
            HandleTriggerPressed();
        }
        if (aiming && !aimCoroutineRunning) {
            StartCoroutine(Aim());
        }
    }

    void LookAtCursor() {
        Ray ray = Camera.main.ScreenPointToRay(Crosshair.Instance.placement.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {   
            Vector3 targetPosition = hit.point;
            targetPosition.y = transform.position.y;
            transform.LookAt(targetPosition);
        }
    }

    public IEnumerator Reload() {
        if (reloading) {
            yield break;
        }
        audioSource.PlayOneShot(reloadSFX, soundSignature / 3);

        currentAmmoInMag = 0;
        reloading = true;
        yield return new WaitForSeconds(reloadTimeSeconds);
        currentAmmoInMag = ammoCapacity;
        reloading = false;
    }


    IEnumerator Aim() {
        if (aimCoroutineRunning) {
            yield break;
        }
        aimCoroutineRunning = true;
        float scopeMultiplier;
        switch (Optics) {
            case OpticType.x1:
                scopeMultiplier = 1.0f;
                break;
            case OpticType.x2:
                scopeMultiplier = 2.0f;
                break;
            case OpticType.x3:
                scopeMultiplier = 3.0f;
                break;
            case OpticType.x4:
                scopeMultiplier = 4.0f;
                break;
            default:
                scopeMultiplier = 1.0f;
                break;
        }
        CameraManager cameraManager = CameraManager.Instance.GetComponent<CameraManager>();
        while (aiming) {
            Vector3 aimOffset = Crosshair.Instance.GetOriginDistanceFromCenter() * scopeMultiplier;
            cameraManager.playerOffset = aimOffset * 10;
            yield return null;
        }
        cameraManager.playerOffset = Vector3.zero;
        aimCoroutineRunning = false;
    }

    /// <summary>
    /// Put one bullet from mag into chamber
    /// </summary>
    public IEnumerator Charge() {
        if (currentAmmoInMag < 1 || charging || currentAmmoInChamber >= 1) {
            yield break;
        }
        audioSource.PlayOneShot(chargeSFX, soundSignature / 2);
        charging = true;

        yield return new WaitForSeconds(chargingTime);
        currentAmmoInMag -= 1;
        currentAmmoInChamber += 1;
        charging = false;
    }

    IEnumerator resetMelee() {
        yield return new WaitForSeconds(switchTimeSeconds);
        meleeReady = true;
    }

    public void Melee() {
        if (!meleeReady) {
            Debug.Log("Melee not ready");
            return;
        }
        Vector3 targetDirection = Crosshair.Instance.ShotOriginToRaycastHit().point - playerPosition.position;
        bool meleeHit = false;
        RaycastHit hit;
        if (Physics.Raycast(playerPosition.transform.position, targetDirection, out hit, meleeRange)) {
            meleeHit = true;
        }

        if (meleeHit) {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.GetComponent<BaseEnemy>() != null) {
                hit.collider.gameObject.GetComponent<BaseEnemy>().TakeDamage(meleeDamage, firePoint.forward * meleeKnockback, meleeStaggerTimeSeconds);
            }
            meleeReady = false;
            StartCoroutine(resetMelee());
            audioSource.PlayOneShot(meleeHitSFX, soundSignature / 2);  
        } else {
            audioSource.PlayOneShot(meleeMissSFX, soundSignature / 2);  
        }

    }

    public IEnumerator PressTrigger() {
        if (pressTriggerCoroutineRunning) {
            yield break;
        }
        pressTriggerCoroutineRunning = true;
        audioSource.PlayOneShot(disconnectorSFX, soundSignature);
        if (currentAmmoInChamber <= 0) {
            audioSource.PlayOneShot(deadTriggerSFX, soundSignature / 3);
        }

        yield return new WaitForSeconds(triggerPullTimeSeconds);
        if (Input.GetMouseButton(0)) {
            triggerPressed = true;
        }        
        yield return new WaitUntil(() => !Input.GetMouseButton(0));
        triggerPressed = false;
        pressTriggerCoroutineRunning = false;
    }

    public IEnumerator AimDownSight() {
        if (adsTimeSeconds > 0) {
            yield return new WaitForSeconds(adsTimeSeconds);
        }
        if (Input.GetMouseButton(1)) {
            aiming = true;
        }
        yield return new WaitUntil(() => !Input.GetMouseButton(1));
        aiming = false;
    }
    private IEnumerator ResetAutoFireReady() {
        yield return new WaitForSeconds(60 / shotsPerMinute);
        autoFireReady = true;
    }
    private IEnumerator ResetSemiFireReady() {
        yield return new WaitUntil(() => !triggerPressed);
        semiFireReady = true;
    }
    private IEnumerator ResetBurstFireReady() {
        burstShotsFired += 1;
        if (burstShotsFired >= burstSize) {
            yield break;
        }
        yield return new WaitForSeconds(60 / burstRate);
        burstFireReady = true;
        HandleTriggerPressed();
        yield return new WaitUntil(() => (!triggerPressed && burstShotsFired >= burstSize) || reloading);
        yield return new WaitForSeconds(60 / burstRate);
        burstShotsFired = 0;
        burstFireReady = true;
    }
    public void HandleTriggerPressed() {
        if (currentAmmoInChamber <= 0) {
            return;
        }
        if (currentFireMode == FireMode.Semi && semiFireReady) {
            Fire();
            semiFireReady = false;
            StartCoroutine(ResetSemiFireReady());
        }
        if (currentFireMode == FireMode.Auto && autoFireReady) {
            Fire();
            autoFireReady = false;
            StartCoroutine(ResetAutoFireReady());
        }
        if (currentFireMode == FireMode.Burst && burstFireReady) {
            Fire();
            burstFireReady = false;
            StartCoroutine(ResetBurstFireReady());
        }
    }

    protected virtual void Fire() {
        currentAmmoInChamber -= 1;
        if (currentAmmoInMag >= 1 && !reloading) {
            if (!(currentFireMode == FireMode.Semi && requiresChargingBetweenShots)) {
                currentAmmoInMag -= 1;
                currentAmmoInChamber += 1;
            }
        }

        Vector3 expectedHitPoint = new Vector3();
        
        // Cast the first ray from shot placement to get target point
        Vector3 targetPoint = Crosshair.Instance.ShotPlacementToRaycastHit().point;
        float targetDistance = 100f;

        // Cast the second ray from the firepoint to the target point to get the expected hit point
        Ray ray = new Ray(firePoint.position, targetPoint - firePoint.position);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            expectedHitPoint = hit.point;
            targetDistance = hit.distance;
        }

        // Convert MOA to radians
        float spreadRadians = pointFireSpreadMOA * Mathf.Deg2Rad / 60f;
        if (aiming) {
            spreadRadians = adsSpreadMOA * Mathf.Deg2Rad / 60f;
        }
        // Calculate the spread based on the distance to the target
        float spreadAtDistance = Mathf.Tan(spreadRadians) * targetDistance;

        
        Vector3 shootingDirection = (expectedHitPoint - firePoint.position).normalized;
        // Apply random spread
        shootingDirection.x += Random.Range(-spreadAtDistance, spreadAtDistance);
        shootingDirection.y += Random.Range(-spreadAtDistance, spreadAtDistance);
        
        // Instantiate the bullet
        GameObject bullet = Instantiate(ammoType, firePoint.transform.position, transform.rotation);
        bullet.GetComponent<Rigidbody>().linearVelocity = shootingDirection * muzzleVelocity;
        bullet.GetComponent<BaseBullet>().source = gameObject;

                // Play the fire sound

        if (fireSFX != null) {
            audioSource.PlayOneShot(fireSFX, soundSignature);
        }
        // Move the crosshair to random direction
        Crosshair.Instance.Recoil(Random.Range(-recoilX, recoilX) * 10, recoilY * 10);
    }

    private FireMode[] fireModeList;
    private void setupFireModes() {
        List<FireMode> fireModes = new List<FireMode>();
        if (hasSemiFire) {
            fireModes.Add(FireMode.Semi);
        }
        if (hasAutoFire) {
            fireModes.Add(FireMode.Auto);
        }
        if (hasBurstFire) {
            fireModes.Add(FireMode.Burst);
        }
        if (fireModes.Count == 0) {
            Debug.LogError("No fire modes set for gun!");
        }
        fireModeList = fireModes.ToArray();
    }
    public void switchFireMode() {
        int currentIndex = System.Array.IndexOf(fireModeList, currentFireMode);
        int nextIndex = (currentIndex + 1) % fireModeList.Length;
        currentFireMode = fireModeList[nextIndex];
    } 
}
