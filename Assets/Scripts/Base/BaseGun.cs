using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine.UI;

public class BaseGun : MonoBehaviour

{
    [Header("General")]
    public string gunName;
    public GameObject ammoType;
    public AudioClip fireSound;
    public int ammoCapacity = 10;
    public float mass = 0.1f;
    public float recoil = 1.0f;
    public bool requiresChargingBetweenShots = false;
    public float verticalForce = 0.0f;
    public float muzzleVelocity = 300.0f;
    public float soundSignature = 75.0f;
    public float triggerPullTime = 0.2f;
    public float fireDelay = 0.0f;
    public float switchTime = 1.0f;
    public float adsTime = 0.3f;
    public float reloadTime = 1.0f;
    public float chargingTime = 0.5f;
    public string defaultFireMode = "semi";
    


    [Header("Damage")]
    public float maxDamage = 10.0f;
    public float maxDamageRange = 10.0f;
    public float minDamage = 1.0f;
    public float minDamageRange = 100.0f;
    public float armorDamage = 0.5f;
    public float headshotMultiplier = 2.0f;


    [Header("Semi")]
    public bool hasSemiFire = false;
    

    [Header("Auto")]
    public bool hasAutoFire = false;
    public float rateOfFire = 600.0f;

    
    [Header("Burst")]
    public bool hasBurstFire = false;
    public int burstSize = 0;
    public float burstRate = 600.0f;

    [Header("Spread")]
    public float pointFireSpread = 1.0f;
    public float adsSpread = 0.1f;
    public float pelletSpread = 2.0f;

    [Header("Melee")]
    public float meleeDamage = 10.0f;
    public float meleeRange = 2f;
    public float meleeKnockback = 1.0f;
    public float meleeStaggerTime = 0.5f;

    [Header("References")]
    public Transform firePoint;
    public Transform Player;
    private TextMeshProUGUI ammoCountUI;
    private TextMeshProUGUI fireModeUI;
    private TextMeshProUGUI gunNameUI;
    private SVGImage chamberUI;
    private SVGImage chargeUI;
    private AudioSource audioSource;
    private LookAtCursor lookAtCursor;
    private RotateAround rotateAround;

    [Header("Internal")]
    private bool meleeReady = true;
    private string currentFireMode;
    private bool autoFireReady = true;
    private bool semiFireReady = true;
    private int currentAmmoInMag;
    private int currentAmmoInChamber;
    private bool charging = false;
    private bool reloading = false;
    private bool triggerPressed = false;

    public void SetupGun()
    {
        currentFireMode = defaultFireMode;
        currentAmmoInMag = ammoCapacity - 1;
        currentAmmoInChamber = 1;
        getReferences();
        gunNameUI.text = gunName;
    }


    // Update is called once per frame
    public void UpdateGun()
    {
        if (Input.GetMouseButton(0)) {
            StartCoroutine(PressTrigger());
        } else {
            triggerPressed = false;
        }

        if (triggerPressed) {
            if (fireDelay > 0) {
                Invoke("Shoot", fireDelay);
            } else {
                Shoot();
            }
        } else {
            semiFireReady = true;
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

    void ResetAutoFireReady() {
        autoFireReady = true;
    }

    IEnumerator Reload() {
        if (reloading) {
            yield break;
        }
        reloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmoInMag = ammoCapacity;
        reloading = false;
    }

    IEnumerator Charge() {
        if (currentAmmoInMag < 1 || charging) {
            yield break;
        }
        charging = true;
        Debug.Log("Charging");
        yield return new WaitForSeconds(chargingTime);
        currentAmmoInMag -= 1;
        currentAmmoInChamber += 1;
        Debug.Log("Charging Done");
        charging = false;
    }

    IEnumerator resetMelee() {
        yield return new WaitForSeconds(chargingTime);
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
                hit.collider.gameObject.GetComponent<BaseEnemy>().TakeDamage(meleeDamage, firePoint.forward * meleeKnockback, meleeStaggerTime);
            }
            meleeReady = false;
            StartCoroutine(resetMelee());
            Debug.Log("Melee hit: " + hit.collider.gameObject.name);
        } else {
            Debug.Log("Melee Attack thrown, missed");
        }

    }

    private IEnumerator PressTrigger() {
        yield return new WaitForSeconds(triggerPullTime);
        triggerPressed = true;
        if (Input.GetMouseButtonUp(0)) {
            triggerPressed = false;
            yield break;
        }
    }

    private void Shoot() {
        if (currentAmmoInChamber <= 0 || reloading || charging) {
            return;
        }
        if (currentFireMode == "semi") {
            if (!semiFireReady) {
                return;
            }
            semiFireReady = false;
        }
        if (currentFireMode == "auto") {
            if (!autoFireReady) {
                return;
            }
            autoFireReady = false;
            Invoke("ResetAutoFireReady", 60 / rateOfFire);
        }

        if (fireDelay > 0) {
            
        }
        currentAmmoInChamber -= 1;
        if (currentAmmoInMag >= 1 && !requiresChargingBetweenShots) {
            currentAmmoInMag -= 1;
            currentAmmoInChamber += 1;
        }

        float targetDistance = 0f;

        Vector3 expectedHitPoint;
        Vector3 targetPoint;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            targetPoint = hit.point;
        }
        else {
            targetPoint = ray.GetPoint(1000);
        }

        ray = new Ray(firePoint.position, targetPoint - firePoint.position);
        if (Physics.Raycast(ray, out hit)) {
            targetDistance = hit.distance;
            expectedHitPoint = hit.point;
        }
        else {
            targetDistance = 1000;
            expectedHitPoint = ray.GetPoint(1000);
        }
        
        
        Vector3 shootingDirection = (expectedHitPoint - firePoint.position).normalized;

        // Convert MOA to radians
        float spreadRadians = pointFireSpread * Mathf.Deg2Rad / 60f;
        // Calculate the spread based on the distance to the target
        float spreadAtDistance = Mathf.Tan(spreadRadians) * targetDistance;
        // Apply random spread
        shootingDirection.x += Random.Range(-spreadAtDistance, spreadAtDistance);
        shootingDirection.y += Random.Range(-spreadAtDistance, spreadAtDistance);
        shootingDirection.y += verticalForce;
        
        GameObject bullet = Instantiate(ammoType, firePoint.transform.position, transform.rotation);
        bullet.GetComponent<Rigidbody>().velocity = shootingDirection * muzzleVelocity;
        bullet.GetComponent<BaseBullet>().source = gameObject;

        audioSource.PlayOneShot(fireSound);
    }

    private void switchFireMode() {
        string[] fireModes = { "semi", "auto", "burst" };
        int currentIndex = System.Array.IndexOf(fireModes, currentFireMode);
        while (true) {
            currentIndex++;
            if (currentIndex >= fireModes.Length) {
                currentIndex = 0;
            }
            if (fireModes[currentIndex] == "semi" && hasSemiFire) {
                currentFireMode = "semi";
                break;
            }
            if (fireModes[currentIndex] == "auto" && hasAutoFire) {
                currentFireMode = "auto";
                break;
            }
            if (fireModes[currentIndex] == "burst" && hasBurstFire) {
                currentFireMode = "burst";
                break;
            }
        }
    }
    // Update the UI elements
    private void updateUI () {
        updateAmmoText();
        updateChamberUI();
        updateFireMode();
    }
    private void updateAmmoText() {
        if (reloading) {
            ammoCountUI.text = "--";
            return;
        }
        int currentAmmo = currentAmmoInMag + currentAmmoInChamber;
        ammoCountUI.text = currentAmmo.ToString();
        while (ammoCountUI.text.Length < 2) {
            ammoCountUI.text = "0" + ammoCountUI.text;
        }
    }

    private void updateChamberUI() {
        if (currentAmmoInChamber <= 0) {
            chamberUI.color = new Color(1f, 1f, 1f, 0.5f);
            chargeUI.color = new Color(1f, 1f, 1f, 0.5f);
        }
        else {
            chamberUI.color = new Color(1f, 1f, 1f, 1f);
            chargeUI.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    private void updateFireMode() {
        fireModeUI.text = currentFireMode.ToUpper();
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

        string[] uiElementNames = { "Ammo Count UI", "Chamber UI", "Charge UI", "Fire Mode UI", "Gun Name UI" };
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
                }
            } else {
                Debug.LogError($"{uiElementName} GameObject not found!");
            }
        }
    }
}
