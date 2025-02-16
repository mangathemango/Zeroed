using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//! Some of these enums will be replaced with ScriptableObjects in the future
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
/// * Base gun class for all guns in the game
/// </summary>
public abstract class BaseGun : MonoBehaviour

{
    // ! A lot of these nomenclature is kinda inconsistent right now, but I'll fix it later
    [Header("General")]
    public string gunName;
    public GameObject ammoType;
    public int ammoCapacity = 10;
    public int chamberCapacity = 1;
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

    [HideInInspector] public PlayerMovement playerMovement;
    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public FireMode currentFireMode;
    [HideInInspector] public bool meleeReady = true;
    [HideInInspector] public bool autoFireReady = true;
    [HideInInspector] public bool semiFireReady = true;
    [HideInInspector] public bool burstFireReady = true;
    [HideInInspector] public int currentAmmoInMag;
    [HideInInspector] public int currentAmmoInChamber;
    [HideInInspector] public bool triggerPressed = false;
    [HideInInspector] public bool charging = false;
    [HideInInspector] public bool reloading = false;
    [HideInInspector] public bool aiming = false;


    [Header("Internal")]
    private bool aimCoroutineRunning = false;
    private bool pressTriggerCoroutineRunning = false;
    private int burstShotsFired = 0;

    /// <summary>
    ///* The start function for baseGun is used to setup references (such as playerPosition and playerMovement)<br/>
    ///* And to setup the default fire mode and ammo in the mag and chamber<br/>
    /// </summary>
    protected virtual void Start()
    {
        // Setup References
        GameObject player = GameObject.Find("Player");
        playerTransform = player.transform;
        playerMovement = player.GetComponent<PlayerMovement>();

        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Setup gun stuff
        currentFireMode = defaultFireMode;
        currentAmmoInMag = ammoCapacity - 1;
        currentAmmoInChamber = 1;
        SetupFireModes();
    }


    /// <summary>
    ///* The update function for baseGun is used to handle 2 events: trigger being pressed and aiming<br/>
    /// </summary>
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


    /// <summary>
    ///* Rotates the gun to look at the cursor<br/>
    ///? This function is paired with RotateAroundPlayer() to make the gun look at the cursor<br/>
    ///! (Note: Currently the gun can't look up or down on the cursor due to some visual bug)<br/>
    /// </summary>
    void LookAtCursor() {
        Vector3 targetPosition = Crosshair.Instance.CrosshairToRaycastHit().point;
        // Ignore the y axis of the Cursor
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition);
    }


    /// <summary>
    ///* Rotates around the player based on the gun's own rotation<br/>
    ///? This function is paired with LookAtCursor() to make the gun look at the cursor<br/>
    /// </summary>
    void RotateAroundPlayer () {
        transform.RotateAround(playerTransform.position, Vector3.up, transform.rotation.y);
        // Moves the gun a bit forward so it doesn't clip with the player
        transform.position = playerTransform.position + (transform.rotation * Vector3.forward);
    }


    /// <summary>
    ///* Reloads the gun <br/><br/>
    /// 
    ///? The reloading behavior is different depending on whether the<br/>
    ///? gun uses magazines or not (if ammoCapacity == 0, the gun doesn't use magazines)<br/><br/>
    ///
    ///? - If the gun uses magazines, this function will fill up the magazine if it's not already full<br/>
    ///? - If the gun doesn't use magazines, this function will fill up the chamber instead, if it's not already full<br/><br/>
    /// 
    ///! Correct me if I'm wrong I'm not a gun person but the idea is there
    /// </summary>
    public virtual IEnumerator Reload() {
        bool magazineIsFull = currentAmmoInMag >= ammoCapacity;
        bool chamberIsFull = currentAmmoInChamber >= chamberCapacity;
        bool gunUsesMagazines = ammoCapacity > 0;

        if (reloading) {
            yield break;
        }
        if (gunUsesMagazines && magazineIsFull) {
            yield break;
        }
        if (!gunUsesMagazines && chamberIsFull) {
            yield break;
        }

        audioSource.PlayOneShot(reloadSFX, soundSignature / 3);

        currentAmmoInMag = 0;
        reloading = true;
        yield return new WaitForSeconds(reloadTimeSeconds);
        if (gunUsesMagazines) {
            currentAmmoInMag = ammoCapacity;
        } else {
            currentAmmoInChamber = chamberCapacity;
        }
        reloading = false;
    }

    /// <summary>
    ///* Handles when the player aims down the sight<br/><br/>
    ///
    ///? This function basically moves the camera further away from the player in order to inspect the environment.<br/>
    ///? How much the camera is moved away from the player is calculated by: <br/>
    ///? The cursor's distance from the center of the screen * the scope multiplier<br/>
    ///? Details on how exactly the camera shifts from the player is in the CameraManager script<br/>
    ///
    /// TODO: The switch case for scopeMultiplier will be omitted after the Optics are replaced with ScriptableObjects
    /// </summary>
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
            Vector3 aimOffset = Crosshair.Instance.GetCrosshairDistanceFromCenter() * scopeMultiplier;
            cameraManager.playerOffset = aimOffset * 10;
            yield return null;
        }
        cameraManager.playerOffset = Vector3.zero;
        aimCoroutineRunning = false;
    }


    /// <summary>
    ///* Puts one bullet from mag into chamber, if the chamber is not full already<br/>
    /// TODO: Maybe handle the case where the gun doesn't use magazines as well
    /// </summary>
    public IEnumerator Charge() {
        if (currentAmmoInMag < 1 || charging || currentAmmoInChamber >= chamberCapacity) {
            yield break;
        }
        audioSource.PlayOneShot(chargeSFX, soundSignature / 2);
        charging = true;

        yield return new WaitForSeconds(chargingTime);
        currentAmmoInMag -= 1;
        currentAmmoInChamber += 1;
        charging = false;
    }


    /// <summary>
    /// * Melee attack the enemy<br/>
    /// ? The direction of the attack is done by casting a ray from the player to the shotOrigin's position<br/>
    /// ? Enemy hit by a melee attack will take damage, knockback, and stagger<br/>
    /// </summary>
    public void Melee() {
        if (!meleeReady) {
            Debug.Log("Melee not ready");
            return;
        }
        bool meleeHit;

        Vector3 targetDirection = Crosshair.Instance.CrosshairToRaycastHit().point - playerTransform.position;
        RaycastHit hit;
        if (Physics.Raycast(playerTransform.transform.position, targetDirection, out hit, meleeRange)) {
            meleeHit = true;
        } else {
            meleeHit = false;
        }

        if (meleeHit) {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.GetComponent<BaseEnemy>() != null) {
                hit.collider.gameObject.GetComponent<BaseEnemy>().TakeDamage(meleeDamage, firePoint.forward * meleeKnockback, meleeStaggerTimeSeconds);
            }
            meleeReady = false;
            StartCoroutine(ResetMelee());
            audioSource.PlayOneShot(meleeHitSFX, soundSignature / 2);  
        } else {
            audioSource.PlayOneShot(meleeMissSFX, soundSignature / 2);  
        }
    }


    /// <summary>
    /// * Resets meleeReady based on switchTime<br/>
    /// ! (I though switch time is supposed to be for switching between weapons, but I ain't the mechanics designer so don't blame me)<br/>
    /// </summary>
    IEnumerator ResetMelee() {
        yield return new WaitForSeconds(switchTimeSeconds);
        meleeReady = true;
    }


    /// <summary>
    /// * Presses the trigger of the gun<br/>
    /// TODO: Generalize this function to work not only with left mouse button press<br/>
    /// </summary>
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

    /// <summary>
    /// * Aims down the sight<br/>
    /// TODO: Generalize this function to work not only with right mouse button press<br/>
    /// </summary>
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

    /// <summary>
    /// * Resets auto fire ready<br/>
    /// ? This is based on the shotsPerMinute variable<br/>
    /// </summary>
    private IEnumerator ResetAutoFireReady() {
        yield return new WaitForSeconds(60 / shotsPerMinute);
        autoFireReady = true;
    }


    /// <summary>
    /// * Resets semi fire ready<br/>
    /// ? This is based on the trigger being released<br/>
    /// </summary>
    private IEnumerator ResetSemiFireReady() {
        yield return new WaitUntil(() => !triggerPressed);
        semiFireReady = true;
    }

    /// <summary>
    /// * Resets burst fire ready<br/><br/>
    /// 
    /// ! This one is a bit more complicated, and I'm not sure if it's optimized<br/>
    /// ? When trigger is pressed during burst mode, the gun will fire [burstSize] times with the interval of [60 / burstRate].<br/>
    /// ? After a shot is done, the gun will wait for [60 / burstRate] seconds, set burstFireReady to true, and then automatically<br/>
    /// ? presses the trigger until [burstSize] shots are fired. When that is done, burstFireReady is reset to true when the<br/>
    /// ? player releases the trigger. <br/>
    /// ? Also, if the player reloads during the burst, the burst automatically reset to true <br/>
    /// </summary>
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

    /// <summary>
    /// * Handles when the trigger is pressed. This function is called every frame where [triggerPressed] is true<br/><br/>
    /// 
    /// ? Since this function calls every frame, it's important to set fireReady to false right after the shot is fired.<br/>
    /// ? Otherwise, the gun will keep firing every single frame until the trigger is released.<br/>
    /// </summary>
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

    /// <summary>
    /// * Fires the gun.<br/><br/>
    /// 
    /// ? This function handles every action required to fire a gun: Reduce ammo, calculate spread, instantiate bullet, <br/>
    /// ? calculate damage, recoil etc.<br/>
    /// TODO: Implement damage calculation based on range. <br/>
    /// </summary>
    protected virtual void Fire() {
        currentAmmoInChamber -= 1;
        if (!(currentFireMode == FireMode.Semi && requiresChargingBetweenShots)) {
            // Automatically charge the gun if gun doesn't require charging between shots
            AutoCharge();
        }
        
        // Instantiate the bullet
        GameObject bullet = Instantiate(ammoType, firePoint.transform.position, transform.rotation);
        bullet.GetComponent<Rigidbody>().linearVelocity = GetShootingDirection() * muzzleVelocity;
        bullet.GetComponent<BaseBullet>().damage = maxDamage;

        // Play the fire sound
        if (fireSFX != null) {
            audioSource.PlayOneShot(fireSFX, soundSignature);
        }
        // Move the crosshair to random direction
        Crosshair.Instance.Recoil(Random.Range(-recoilX, recoilX) * 10, recoilY * 10);
    }

    /// <summary>
    /// * Automatically charges the gun<br/><br/>
    /// 
    /// ? Different from Charge(), this function is called after the player shoots the gun, not when the player presses any button<br/>
    /// </summary>
    protected void AutoCharge() {
        if (reloading || charging) {
            return;
        }
        if (currentAmmoInMag <= 0) {
            return;
        }
        currentAmmoInMag -= 1;
        currentAmmoInChamber += 1;
    }

    /// <summary>
    /// * Gets the shooting direction based on the shot placement position and spread of the gun<br/><br/>
    /// ? To view how this is done, check the comments inside this function<br/>
    /// </summary>
    /// <returns>The bullet's expected firing direction</returns>
    protected Vector3 GetShootingDirection() {
        
        // Cast the first ray from shot placement to get target point
        Vector3 targetPoint = Crosshair.Instance.CrosshairToRaycastHit().point;
        float targetDistance = 100f;

        // Cast the second ray from the firepoint to the target point to get the expected hit point
        Ray ray = new Ray(firePoint.position, targetPoint - firePoint.position);
        Vector3 expectedHitPoint = new Vector3();
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            expectedHitPoint = hit.point;
            targetDistance = hit.distance;
        }

        // Convert MOA to radians
        float spreadRadians;
        if (aiming) {
            spreadRadians = adsSpreadMOA * Mathf.Deg2Rad / 60f;
        } else {
            spreadRadians = pointFireSpreadMOA * Mathf.Deg2Rad / 60f;
        }
        // Calculate the spread based on the distance to the target
        float spreadAtDistance = Mathf.Tan(spreadRadians) * targetDistance;

        
        Vector3 shootingDirection = expectedHitPoint - firePoint.position;
        // Apply random spread
        shootingDirection.x += Random.Range(-spreadAtDistance, spreadAtDistance);
        shootingDirection.y += Random.Range(-spreadAtDistance, spreadAtDistance);
        return shootingDirection.normalized;
    }

    private FireMode[] fireModeList;
    /// <summary>
    /// * Sets up the fire modes for the gun by putting the selected fire modes in an array<br/><br/>
    /// ? This is to cycle between the fire modes when switchFireMode() is called<br/>
    /// </summary>
    private void SetupFireModes() {
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

    /// <summary>
    /// * Switches the fire mode of the gun<br/><br/>
    /// ? This is done by selecting the next fire mode in the fireModeList array<br/>
    /// </summary>
    public void SwitchFireMode() {
        int currentIndex = System.Array.IndexOf(fireModeList, currentFireMode);
        int nextIndex = (currentIndex + 1) % fireModeList.Length;
        currentFireMode = fireModeList[nextIndex];
    } 
}
