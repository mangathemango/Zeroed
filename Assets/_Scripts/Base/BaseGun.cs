using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using UnityEditor.Callbacks;
using GunStuff;

namespace GunStuff {
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
    [Range (0.0f, 10.0f)]
    public float recoil = 1.0f;
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
    
    [System.NonSerialized] public Transform Player;
    private TextMeshProUGUI ammoCountUI;
    private TextMeshProUGUI fireModeUI;
    private TextMeshProUGUI gunNameUI;
    private SVGImage chamberUI;
    private SVGImage chargeUI;
    private RawImage ReloadingUI;
    private AudioSource audioSource;
    private LookAtCursor lookAtCursor;
    private RotateAround rotateAround;

    [Header("Internal")]
    private bool meleeReady = true;
    private FireMode currentFireMode;
    private bool autoFireReady = true;
    private bool semiFireReady = true;
    private bool burstFireReady = true;
    private int burstShotsFired = 0;
    private int currentAmmoInMag;
    private int currentAmmoInChamber;
    private bool charging = false;
    private bool reloading = false;
    private bool triggerPressed = false;

    protected virtual void Start()
    {
        currentFireMode = defaultFireMode;
        currentAmmoInMag = ammoCapacity - 1;
        currentAmmoInChamber = 1;
        getReferences();
        setupFireModes();
    }


    // Update is called once per frame
    protected virtual void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            audioSource.PlayOneShot(disconnectorSFX, soundSignature);
            if (currentAmmoInChamber <= 0) {
                audioSource.PlayOneShot(deadTriggerSFX, soundSignature / 3);
            }
            StartCoroutine(PressTrigger());

        }

        if (triggerPressed) {
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            StartCoroutine(Reload());
        }
        if (Input.GetMouseButtonDown(2)) {
            StartCoroutine(Charge());
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            Melee();
        }
        if (Input.GetKeyDown(KeyCode.V)) {
            switchFireMode();
        }
        updateUI();
    }

    IEnumerator Reload() {
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




    /// <summary>
    /// Put one bullet from mag into chamber
    /// </summary>
    IEnumerator Charge() {
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

    private void Melee() {
        if (!meleeReady) {
            Debug.Log("Melee not ready");
            return;
        }
        RaycastHit hit;
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 targetDirection = new Vector3();
        if (Physics.Raycast(screenRay, out hit)) {
            targetDirection = hit.point - Player.transform.position;
        }
        if (Physics.Raycast(Player.transform.position, targetDirection, out hit, meleeRange)) {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.GetComponent<BaseEnemy>() != null) {
                hit.collider.gameObject.GetComponent<BaseEnemy>().TakeDamage(meleeDamage, firePoint.forward * meleeKnockback, meleeStaggerTimeSeconds);
            }
            meleeReady = false;
            StartCoroutine(resetMelee());
            audioSource.PlayOneShot(meleeHitSFX, soundSignature / 2);  
            Debug.Log("Melee hit: " + hit.collider.gameObject.name);
        } else {
            Debug.Log("Melee Attack thrown, missed");
            audioSource.PlayOneShot(meleeMissSFX, soundSignature / 2);  
        }

    }

    private IEnumerator PressTrigger() {
        yield return new WaitForSeconds(triggerPullTimeSeconds);
        triggerPressed = true;
        yield return new WaitUntil(() => !Input.GetMouseButton(0));
        triggerPressed = false;
    }

    IEnumerator ResetAutoFireReady() {
        yield return new WaitForSeconds(60 / shotsPerMinute);
        autoFireReady = true;
    }
    IEnumerator ResetSemiFireReady() {
        yield return new WaitUntil(() => !triggerPressed);
        semiFireReady = true;
    }
    IEnumerator ResetBurstFireReady() {
        burstShotsFired += 1;
        if (burstShotsFired >= burstSize) {
            yield break;
        }
        yield return new WaitForSeconds(60 / burstRate);
        burstFireReady = true;
        yield return new WaitUntil(() => !triggerPressed);
        burstShotsFired = 0;
        burstFireReady = true;
    }
    public void Shoot() {
        if (currentAmmoInChamber <= 0 ||
            (currentFireMode == FireMode.Semi && !semiFireReady) ||
            (currentFireMode == FireMode.Auto && !autoFireReady) ||
            (currentFireMode == FireMode.Burst && !burstFireReady)) {
            return;
        }
        Fire();
        if (currentFireMode == FireMode.Semi) {
            semiFireReady = false;
            StartCoroutine(ResetSemiFireReady());
        }
        if (currentFireMode == FireMode.Auto) {
            autoFireReady = false;
            StartCoroutine(ResetAutoFireReady());
        }
        if (currentFireMode == FireMode.Burst) {
            burstFireReady = false;
            StartCoroutine(ResetBurstFireReady());
        }
    }

    public void Fire() {
        currentAmmoInChamber -= 1;
        if (currentAmmoInMag >= 1 && !reloading) {
            if (!(currentFireMode == FireMode.Semi && requiresChargingBetweenShots)) {
                currentAmmoInMag -= 1;
                currentAmmoInChamber += 1;
            }
        }

        Vector3 expectedHitPoint = new Vector3();
        Vector3 targetPoint = new Vector3();
        float targetDistance = 1f;

        // Cast the first ray from the camera to the mouse position to get target point
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            targetPoint = hit.point;
            targetDistance = hit.distance;
        }

        // Cast the second ray from the firepoint to the target point to get the expected hit point
        ray = new Ray(firePoint.position, targetPoint - firePoint.position);
        if (Physics.Raycast(ray, out hit)) {
            expectedHitPoint = hit.point;
        }
        
        
        Vector3 shootingDirection = (expectedHitPoint - firePoint.position).normalized;

        // Convert MOA to radians
        float spreadRadians = pointFireSpreadMOA * Mathf.Deg2Rad / 60f;
        // Calculate the spread based on the distance to the target
        float spreadAtDistance = Mathf.Tan(spreadRadians) * targetDistance;
        // Apply random spread
        shootingDirection.x += Random.Range(-spreadAtDistance, spreadAtDistance);
        shootingDirection.y += Random.Range(-spreadAtDistance, spreadAtDistance);
        
        // Instantiate the bullet
        GameObject bullet = Instantiate(ammoType, firePoint.transform.position, transform.rotation);
        bullet.GetComponent<Rigidbody>().velocity = shootingDirection * muzzleVelocity;
        bullet.GetComponent<BaseBullet>().source = gameObject;

        if (fireSFX != null) {
            audioSource.PlayOneShot(fireSFX, soundSignature);
        }
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
    private void switchFireMode() {
        int currentIndex = System.Array.IndexOf(fireModeList, currentFireMode);
        int nextIndex = (currentIndex + 1) % fireModeList.Length;
        currentFireMode = fireModeList[nextIndex];
    }
    // Update the UI elements
    private void updateUI () {
        updateAmmoText();
        updateChamberUI();
        updateFireMode();
    }
    private void updateAmmoText() {
        int currentAmmo = currentAmmoInMag + currentAmmoInChamber;
        ammoCountUI.text = currentAmmo.ToString();
        while (ammoCountUI.text.Length < 2) {
            ammoCountUI.text = "0" + ammoCountUI.text;
        }
    }

    private float currentReloadRotation = 0;
    private void updateChamberUI() {
        if (currentAmmoInChamber <= 0) {
            chamberUI.color = new Color(1f, 1f, 1f, 0.5f);
            chargeUI.color = new Color(1f, 1f, 1f, 0.5f);
        }
        else {
            chamberUI.color = new Color(1f, 1f, 1f, 1f);
            chargeUI.color = new Color(1f, 1f, 1f, 1f);
        }
        if (reloading) {
            chargeUI.color = new Color(1f, 1f, 1f, 0f);
            ReloadingUI.color = new Color(1f, 1f, 1f, 1f);
            currentReloadRotation += 1; 
            ReloadingUI.transform.rotation = Quaternion.Euler(0, 0, currentReloadRotation);
        } else {
            ReloadingUI.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    private void updateFireMode() {
        fireModeUI.text = currentFireMode.ToString().ToUpper();
    }
    public void getReferences () {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (Player == null) {
            Player = GameObject.Find("Player").transform;
        }
        if (Player == null) {
            Debug.LogError("Player not found!");
        }

        lookAtCursor = gameObject.AddComponent<LookAtCursor>();
        rotateAround = gameObject.AddComponent<RotateAround>();
        rotateAround.target = Player;
        rotateAround.offsetPosition = new Vector3(0, 0, -1);

        string[] uiElementNames = { "Ammo Count UI", "Chamber UI", "Charge UI", "Fire Mode UI", "Gun Name UI", "Reloading UI" };
        foreach (string uiElementName in uiElementNames) {
            GameObject uiObject = GameObject.Find(uiElementName);
            if (uiObject != null) {
                switch (uiElementName) {
                    case "Ammo Count UI":
                        ammoCountUI = uiObject.GetComponent<TextMeshProUGUI>();
                        Debug.Log("Ammo Count GameObject found!");
                        break;
                    case "Chamber UI":
                        chamberUI = uiObject.GetComponent<SVGImage>();
                        Debug.Log("Chamber UI GameObject found!");
                        break;
                    case "Charge UI":
                        chargeUI = uiObject.GetComponent<SVGImage>();
                        Debug.Log("Charge UI GameObject found!");
                        break;
                    case "Fire Mode UI":
                        fireModeUI = uiObject.GetComponent<TextMeshProUGUI>();
                        Debug.Log("Fire Mode UI GameObject found!");
                        break;
                    case "Gun Name UI":
                        gunNameUI = uiObject.GetComponent<TextMeshProUGUI>();
                        Debug.Log("Gun Name UI GameObject found!");
                        break;
                    case "Reloading UI":
                        ReloadingUI = uiObject.GetComponent<RawImage>();
                        Debug.Log("Reloading UI GameObject found!");
                        break;
                }
            } else {
                Debug.LogError($"{uiElementName} GameObject not found!");
            }
        }
    }
}
