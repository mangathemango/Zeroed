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

/// <summary>
///* Base class for all guns in the game
/// </summary>
public abstract class BaseGun : MonoBehaviour

{
    [Header("General")]
    [Tooltip("The name of the gun")]
    public string gunName;
    [Tooltip("The type of ammunition used by the gun")]
    public GameObject ammoType;
    [Tooltip("The maximum number of rounds the gun can hold in its magazine")]
    public int ammoCapacity = 10;
    [Tooltip("The number of rounds the gun can hold in its chamber")]
    public int chamberCapacity = 1;
    [Tooltip("The mass of the gun in kilograms")]
    [Range       (0.0f, 10.0f)]
    public float mass = 0.1f;
    [Tooltip("The muzzle velocity of the gun in meters per second")]
    [Range (10.0f, 1000.0f)]
    public float muzzleVelocity = 300.0f;
    [Tooltip("The sound signature of the gun")]
    [Range (1.0f, 10.0f)]
    public float soundSignature = 1.0f;
    [Tooltip("The time it takes to pull the trigger in seconds")]
    [Range (0f, 1f)]
    public float triggerPullTimeSeconds = 0.2f;
    [Tooltip("Apparently, this is technically melee cooldown for now")]
    [Range (0f, 3f)]
    public float switchTimeSeconds = 1.0f;
    [Tooltip("The time it takes to aim down sights in seconds")]
    [Range (0f, 2f)]
    public float adsTimeSeconds = 0.3f;
    [Tooltip("The time it takes to reload the gun in seconds")]
    [Range (0f, 4f)]
    public float reloadTimeSeconds = 1.0f;
    [Tooltip("The time it takes to charge the gun in seconds")]
    [Range (0f, 3f)]
    public float chargingTime = 0.5f;
    [Tooltip("The default fire mode of the gun")]
    public FireMode defaultFireMode = FireMode.Semi;
    
    [Header("Damage")]
    [Tooltip("The maximum damage the gun can deal")]
    [Range (0f, 100f)]
    public float maxDamage = 10.0f;
    [Tooltip("The minimum damage the gun can deal")]
    [Range (0f, 100f)]
    public float minDamage = 1.0f;
    [Tooltip("The range at which the gun deals maximum damage")]
    [Range (0f, 100f)]
    public float maxDamageRange = 10.0f;
    [Tooltip("The range at which the gun deals minimum damage")]
    [Range (0f, 100f)]
    public float minDamageRange = 100.0f;
    [Tooltip("The percentage of armor penetration")]
    [Range (0f, 1f)]
    public float armorPenetrationPercent = 0f;
    [Tooltip("The multiplier for armor damage")]
    [Range (1f, 10f)]
    public float armorDamageMultiplier = 0.1f;
    [Tooltip("The multiplier for headshot damage")]
    [Range (1f, 10f)]
    public float headshotMultiplier = 2.0f;

    [Header("Semi")]
    [Tooltip("Indicates if the gun has semi-automatic fire mode")]
    public bool hasSemiFire = false;
    [Tooltip("Indicates if the gun requires charging between shots in semi-automatic mode")]
    public bool requiresChargingBetweenShots = false;
    
    [Header("Auto")]
    [Tooltip("Indicates if the gun has automatic fire mode")]
    public bool hasAutoFire = false;
    [Tooltip("The rate of fire in shots per minute")]
    [Range (100.0f, 1000.0f)]
    public float shotsPerMinute = 600.0f;

    [Header("Burst")]
    [Tooltip("Indicates if the gun has burst fire mode")]
    public bool hasBurstFire = false;
    [Tooltip("The number of shots fired in a burst")]
    public int burstSize = 0;
    [Tooltip("The rate of fire for burst mode in shots per minute")]
    public float burstRate = 600.0f;

    [Header("Spread")]
    [Tooltip("The spread of the gun when firing from the hip in MOA")]
    public float pointFireSpreadMOA = 1.0f;
    [Tooltip("The spread of the gun when aiming down sights in MOA")]
    public float adsSpreadMOA = 0.1f;
    [Tooltip("The spread of the pellets in MOA for shotguns")]
    public float pelletSpreadMOA = 2.0f;

    [Header("Recoil")]
    [Tooltip("The vertical recoil of the gun")]
    [Range (0.0f, 10.0f)]
    public float recoilY = 1.0f;
    [Tooltip("The horizontal recoil of the gun")]
    [Range (0.0f, 10.0f)]
    public float recoilX = 1.0f;

    [Header("Attachments")]
    [Tooltip("The type of optic attached to the gun")]
    public OpticType Optics = OpticType.None;

    [Header("Melee")]
    [Tooltip("The damage dealt by melee attacks")]
    public float meleeDamage = 10.0f;
    [Tooltip("The range of melee attacks")]
    public float meleeRange = 2f;
    [Tooltip("The knockback force of melee attacks")]
    public float meleeKnockback = 1.0f;
    [Tooltip("The stagger time caused by melee attacks in seconds")]
    public float meleeStaggerTimeSeconds = 0.5f;

    [Header("References")]
    [Tooltip("The point from which the gun fires")]
    public Transform firePoint;
    [Tooltip("The sound effect played when the trigger is pulled but the gun is empty")]
    public AudioClip deadTriggerSFX;
    [Tooltip("The sound effect played when the disconnector is engaged")]
    public AudioClip disconnectorSFX;
    [Tooltip("The sound effect played when the gun is fired")]
    public AudioClip fireSFX;
    [Tooltip("The sound effect played when the gun is reloaded")]
    public AudioClip reloadSFX;
    [Tooltip("The sound effect played when the gun is charged")]
    public AudioClip chargeSFX;
    [Tooltip("The sound effect played when a melee attack misses")]
    public AudioClip meleeMissSFX;
    [Tooltip("The sound effect played when a melee attack hits")]
    public AudioClip meleeHitSFX;
#if DEBUG
    [Tooltip("The player movement script")]
    public PlayerMovement playerMovement;
    [Tooltip("The position of the player")]
    public Transform playerPosition;
    [Tooltip("The audio source for playing sound effects")]
    public AudioSource audioSource;
    [Tooltip("The current fire mode of the gun")]
    public FireMode currentFireMode;
    [Tooltip("Indicates if the melee attack is ready")]
    public bool meleeReady = true;
    [Tooltip("Indicates if the gun is ready to fire in automatic mode")]
    public bool autoFireReady = true;
    [Tooltip("Indicates if the gun is ready to fire in semi-automatic mode")]
    public bool semiFireReady = true;
    [Tooltip("Indicates if the gun is ready to fire in burst mode")]
    public bool burstFireReady = true;
    [Tooltip("The current number of rounds in the magazine")]
    public int currentAmmoInMag;
    [Tooltip("The current number of rounds in the chamber")]
    public int currentAmmoInChamber;
    [Tooltip("Indicates if the trigger is pressed")]
    public bool triggerPressed = false;
    [Tooltip("Indicates if the gun is being charged")]
    public bool charging = false;
    [Tooltip("Indicates if the gun is being reloaded")]
    public bool reloading = false;
    [Tooltip("Indicates if the player is aiming down sights")]
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
    [Tooltip("Indicates if the aim coroutine is running")]
    private bool aimCoroutineRunning = false;
    [Tooltip("Indicates if the press trigger coroutine is running")]
    private bool pressTriggerCoroutineRunning = false;
    [Tooltip("The number of shots fired in the current burst")]
    private int burstShotsFired = 0;

    protected virtual void Start()
    {
        // Setup References
        GameObject player = GameObject.Find("Player");
        playerPosition = player.transform;
        playerMovement = player.GetComponent<PlayerMovement>();

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
        RotateAroundPlayer();
        if (triggerPressed) {
            HandleTriggerPressed();
        }
        if (aiming && !aimCoroutineRunning) {
            StartCoroutine(Aim());
        }
    }

    void LookAtCursor() {
        Vector3 targetPosition = Crosshair.Instance.ShotPlacementToRaycastHit().point;
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition);
    }

    void RotateAroundPlayer () {
        transform.RotateAround(playerPosition.position, Vector3.up, transform.rotation.y);
        transform.position = playerPosition.position + (transform.rotation * Vector3.forward);
    }

    public virtual IEnumerator Reload() {
        if (reloading || (currentAmmoInMag >= ammoCapacity && ammoCapacity > 0)) {
            yield break;
        }
        audioSource.PlayOneShot(reloadSFX, soundSignature / 3);

        currentAmmoInMag = 0;
        reloading = true;
        yield return new WaitForSeconds(reloadTimeSeconds);
        if (ammoCapacity > 0) {
            currentAmmoInMag = ammoCapacity;
        } else {
            currentAmmoInChamber = chamberCapacity;
        }
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
        bullet.GetComponent<BaseBullet>().damage = maxDamage;

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
