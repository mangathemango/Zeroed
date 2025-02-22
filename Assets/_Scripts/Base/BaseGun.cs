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
    [HideInInspector] public Transform environment;
    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public FireMode currentFireMode;
    [HideInInspector] public bool meleeReady = true;
    [HideInInspector] public bool autoFireReady = true;
    [HideInInspector] public bool semiFireReady = true;
    [HideInInspector] public bool burstFireReady = true;
    [HideInInspector] public int currentAmmoInMag;
    [HideInInspector] public int currentAmmoInChamber;
    [HideInInspector] private bool initialized = false;

    //* Coroutines
    //* Trigger Press
    private Coroutine triggerPressTransitioningCoroutine;
    private Coroutine triggerPressCoroutine;

    /// <summary>
    ///* Returns true whenever there's a trigger press coroutine running
    /// </summary>
    public bool triggerPressed {
        get {return triggerPressCoroutine != null;}
    }


    //* Aim Down Sight
    private Coroutine aimDownSightTransitioningCoroutine;
    private Coroutine aimDownSightCoroutine;

    /// <summary>
    /// * Returns true whenever there's an aim down sight coroutine running
    /// </summary>
    public bool aiming {
        get {return aimDownSightCoroutine != null;}
    }

    //* Reload
    private Coroutine reloadCoroutine;

    /// <summary>
    /// * Returns true whenever there's a reload coroutine running
    /// </summary>
    public bool reloading {
        get {return reloadCoroutine != null;}
    }
    
    //* Charge
    private Coroutine chargeCoroutine;

    /// <summary>
    /// * Returns true whenever there's a charge coroutine running
    /// </summary>
    public bool charging {
        get {return chargeCoroutine != null;}
    }


    
    /// <summary>
    ///* Reloads the gun <br/><br/>
    /// 
    ///? The reloading behavior is different depending on whether the gun uses magazines or not<br/>
    ///? (if ammoCapacity == 0, the gun doesn't use magazines)<br/><br/>
    ///
    ///? - If the gun uses magazines, this function will fill up the magazine if it's not already full<br/>
    ///? - If the gun doesn't use magazines, this function will fill up the chamber instead, if it's not already full<br/><br/>
    /// </summary>
    public void Reload() {
        reloadCoroutine ??= StartCoroutine(ReloadCoroutine());

        IEnumerator ReloadCoroutine() {
            bool magazineIsFull = currentAmmoInMag >= ammoCapacity;
            bool chamberIsFull = currentAmmoInChamber >= chamberCapacity;
            bool gunUsesMagazines = ammoCapacity > 0;

            if (gunUsesMagazines && magazineIsFull) {
                reloadCoroutine = null;
                yield break;
            }
            if (!gunUsesMagazines && chamberIsFull) {
                reloadCoroutine = null;
                yield break;
            }

            audioSource.PlayOneShot(reloadSFX, soundSignature / 3);

            currentAmmoInMag = 0;
            yield return new WaitForSeconds(reloadTimeSeconds);
            if (gunUsesMagazines) {
                currentAmmoInMag = ammoCapacity;
            } else {
                currentAmmoInChamber = chamberCapacity;
            }
            reloadCoroutine = null;
        }           
    }

    /// <summary>
    ///* Puts one bullet from mag into chamber, if the chamber is not full already<br/>
    /// </summary>
    public void Charge() {
        chargeCoroutine ??= StartCoroutine(ChargeCoroutine());

        IEnumerator ChargeCoroutine() {
            if (currentAmmoInMag <= 0 || currentAmmoInChamber >= chamberCapacity) {
                chargeCoroutine = null;
                yield break;
            }

            audioSource.PlayOneShot(chargeSFX, soundSignature / 2);

            yield return new WaitForSeconds(chargingTime);
            currentAmmoInMag -= 1;
            currentAmmoInChamber += 1;
            chargeCoroutine = null;
        }
    }


    /// <summary>
    /// * Presses the trigger of the gun<br/>
    /// ? This function has 2 coroutines: TriggerPressTransitioning() and TriggerPressCoroutine()<br/>
    /// ? TriggerPressTransitioning() is used to delay the TriggerPressCoroutine() function by [triggerPullTimeSeconds]<br/>
    /// ? TriggerPressCoroutine() is used to handle the trigger being pressed<br/>
    /// ? TriggerPressCoroutine() procs HandleTriggerPressed() every frame while the mouse is pressed<br/>
    /// 
    /// TODO: Generalize this function to work not only with left mouse button press<br/>
    /// </summary>
    public void PressTrigger() {
        triggerPressTransitioningCoroutine ??= StartCoroutine(TriggerPressTransitioning());

        IEnumerator TriggerPressTransitioning() {
            audioSource.PlayOneShot(disconnectorSFX, soundSignature);
            if (currentAmmoInChamber <= 0) {
                audioSource.PlayOneShot(deadTriggerSFX, soundSignature / 3);
            }

            yield return new WaitForSeconds(triggerPullTimeSeconds);
            triggerPressCoroutine = StartCoroutine(TriggerPressCoroutine());
            yield return new WaitUntil(() => !Input.GetMouseButton(0));
            
            triggerPressTransitioningCoroutine = null;
        }

        IEnumerator TriggerPressCoroutine() {   
            while (Input.GetMouseButton(0)) {
                HandleTriggerPressed();
                yield return null;
            }
            triggerPressCoroutine = null;
        }
        

        // This funtion is called every frame where the trigger is pressed
        void HandleTriggerPressed() {
            if (currentFireMode == FireMode.Semi && semiFireReady) {
                Fire();
                semiFireReady = false;
                StartCoroutine(ResetSemiFireReady());

                IEnumerator ResetSemiFireReady() {
                    yield return null;
                    while (triggerPressed) {
                        yield return null;
                    }
                    semiFireReady = true;   
                }
            }
            if (currentFireMode == FireMode.Auto && autoFireReady) {
                Fire();
                autoFireReady = false;
                StartCoroutine(ResetAutoFireReady());

                IEnumerator ResetAutoFireReady() {
                    yield return new WaitForSeconds(60 / shotsPerMinute);
                    if (reloading) {
                        yield return new WaitUntil(() => !triggerPressed);
                    }
                    autoFireReady = true;
                }
            }
            if (currentFireMode == FireMode.Burst && burstFireReady) {
                StartCoroutine(BurstFire());
                burstFireReady = false;

                IEnumerator BurstFire() {
                    for (int i = 0; i < burstSize && currentAmmoInChamber > 0; i++) {
                        Fire();
                        yield return new WaitForSeconds(60 / burstRate);
                    }
                    StartCoroutine(ResetBurstFireReady());
                }

                IEnumerator ResetBurstFireReady() {
                    yield return new WaitUntil(() => !triggerPressed);
                    burstFireReady = true;
                }
            }
        }
    }

    /// <summary>
    /// * Aims down the sight<br/>
    /// 
    /// ? Note: This function has 2 coroutines: AimDownSightTransitioning() and AimDownSightCoroutine()<br/>
    /// ? AimDownSightTransitioning() is used to delay the AimDownSightCoroutine() function by [adsTimeSeconds]<br/>
    /// ? AimDownSightCoroutine() is used to move the camera away from the player to simulate aiming down the sight<br/>
    /// TODO: Generalize this function to work not only with right mouse button press<br/>
    /// </summary>
    public void AimDownSight() {
        aimDownSightTransitioningCoroutine ??= StartCoroutine(AimDownSightTransitioning());
        
        IEnumerator AimDownSightTransitioning() {
            yield return new WaitForSeconds(adsTimeSeconds);

            aimDownSightCoroutine ??= StartCoroutine(AimDownSightCoroutine());
            
            yield return new WaitUntil(() => !Input.GetMouseButton(1));

            StopCoroutine(aimDownSightCoroutine);
            aimDownSightCoroutine = null;
            CameraManager.Instance.playerOffset = Vector3.zero;
            aimDownSightTransitioningCoroutine = null;
        }

        IEnumerator AimDownSightCoroutine() {
            float scopeMultiplier = GetScopeMultiplier();
            while (true) {
                Vector3 aimOffset = Crosshair.Instance.GetCrosshairDistanceFromCenter() * scopeMultiplier;
                // Shifts the camera away from the player
                CameraManager.Instance.playerOffset = aimOffset * 10;
                yield return null;
            }
        }

        // TODO: The switch case for scopeMultiplier will be omitted after the Optics are replaced with ScriptableObjects
        float GetScopeMultiplier() {
            switch (Optics) {
                case OpticType.x1:
                    return 1.0f;
                case OpticType.x2:
                    return 2.0f;
                case OpticType.x3:
                    return 3.0f;
                case OpticType.x4:
                    return 4.0f;
                default:
                    return 1.0f;
            }
        }
    }

    /// <summary>
    ///* The start function for baseGun is used to setup references (such as playerPosition and playerMovement)<br/>
    ///* And to setup the default fire mode and ammo in the mag and chamber<br/>
    /// </summary>
    protected virtual void Start()
    {
        Initialize();
    }

    public void Initialize () {
        if (initialized) {
            return;
        }
        // Setup References
        if (!playerTransform || !playerMovement || !environment) {
            GameObject player = GameObject.Find("Player");
            playerTransform = player.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
            environment = GameObject.Find("Environment").transform;
        }


        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Setup gun stuff
        currentFireMode = defaultFireMode;
        currentAmmoInMag = ammoCapacity - 1;
        currentAmmoInChamber = 1;
        SetupFireModes();

        initialized = true;
    }


    /// <summary>
    ///* The update function for baseGun is used to handle 2 events: trigger being pressed and aiming<br/>
    /// </summary>
    protected virtual void Update()
    {   
        LookAtCursor();
        RotateAroundPlayer();
    }

    public void Disable() {
        gameObject.SetActive(false);
        StopAllGunCoroutines();
    }

    public void Enable() {
        gameObject.SetActive(true);
    }

    private void StopAllGunCoroutines() {
        StopAllCoroutines();
        triggerPressCoroutine = null;
        triggerPressTransitioningCoroutine = null;
        aimDownSightCoroutine = null;
        aimDownSightTransitioningCoroutine = null;
        reloadCoroutine = null;
        chargeCoroutine = null;
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

        IEnumerator ResetMelee() {
            yield return new WaitForSeconds(switchTimeSeconds);
            meleeReady = true;
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
        if (currentAmmoInChamber <= 0) {
            return;
        }
        currentAmmoInChamber -= 1;
        if (!(currentFireMode == FireMode.Semi && requiresChargingBetweenShots)) {
            // Automatically charge the gun if gun doesn't require charging between shots
            AutoCharge();
        }
        
        // Instantiate the bullet
        GameObject bullet = Instantiate(ammoType, firePoint.transform.position, transform.rotation, environment);
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
            spreadRadians = adsSpreadMOA * Mathf.Deg2Rad;
        } else {
            spreadRadians = pointFireSpreadMOA * Mathf.Deg2Rad;
        }
        // Calculate the spread based on the distance to the target
        float spreadAtDistance = Mathf.Tan(spreadRadians) * targetDistance;

        
        Vector3 shootingDirection = expectedHitPoint - firePoint.position;
        // Apply random spread
        shootingDirection.x += Random.Range(-spreadAtDistance, spreadAtDistance);
        shootingDirection.y += Random.Range(-spreadAtDistance, spreadAtDistance);
        shootingDirection.z += Random.Range(-spreadAtDistance, spreadAtDistance);

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
