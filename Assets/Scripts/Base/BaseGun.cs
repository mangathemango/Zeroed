using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VectorGraphics;

public class BaseGun : MonoBehaviour

{
    [Header("General")]
    public GameObject ammoType;
    public AudioClip fireSound;
    public int ammoCapacity = 10;
    public float mass = 0.1f;
    public float recoil = 1.0f;
    public bool requiresCharging = false;
    public float muzzleVelocity = 300.0f;
    public float soundSignature = 75.0f;
    public float triggerPullTime = 0.2f;
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

    private TextMeshProUGUI ammoText;
    private bool meleeReady = true;
    private string currentFireMode;
    private AudioSource audioSource;
    private bool readyToFire = true;
    private int currentAmmo;
    private LookAtCursor lookAtCursor;
    private RotateAround rotateAround;
    private bool chamberEmpty = false;
    private bool chargeEmpty = false;
    private GameObject chamberUI;
    private GameObject chargeUI;

    public void SetupGun()
    {
        audioSource = GetComponent<AudioSource>();
        currentFireMode = defaultFireMode;
        currentAmmo = ammoCapacity;

        if (Player == null) {
            Player = GameObject.Find("Player").transform;
        }

        lookAtCursor = gameObject.AddComponent<LookAtCursor>();
        rotateAround = gameObject.AddComponent<RotateAround>();
        rotateAround.target = Player;
        rotateAround.offsetPosition = new Vector3(0, 0, -1);

        
        GameObject ammoCountObject = GameObject.Find("Ammo Count");
        if (ammoCountObject != null)
        {
            ammoText = ammoCountObject.GetComponent<TextMeshProUGUI>();
            Debug.Log("Ammo Count GameObject found!");
        }
        else
        {
            Debug.LogError("Ammo Count GameObject not found!");
        }
        updateAmmoText();
        chamberUI = GameObject.Find("Chamber UI");
        
    }

    // Update is called once per frame
    public void UpdateGun()
    {
        if (Input.GetMouseButtonDown(0) && currentFireMode == "semi") {
            Shoot();
        }
        if (Input.GetMouseButton(0) && currentFireMode == "auto" && readyToFire) {
            Shoot();
            readyToFire = false;
            StartCoroutine(ReadyToFire());
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
    }

    IEnumerator ReadyToFire() {
        yield return new WaitForSeconds(60f / rateOfFire);
        readyToFire = true;
    }

    IEnumerator Reload() {
        ammoText.text = "--";
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = ammoCapacity;
        if (!chamberEmpty) {
            currentAmmo = ammoCapacity + 1;
        }
        updateAmmoText();
    }

    IEnumerator Charge() {
        if (currentAmmo <= 0) {
            yield break;
        }
        Debug.Log("Charging");
        yield return new WaitForSeconds(chargingTime);
        chamberEmpty = false;
        chamberUI.SetActive(true);
        updateAmmoText();
        Debug.Log("Charging Done");
        updateChamberAndCharge();
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
    private void Shoot() {
        if (currentAmmo <= 0) {
            return;
        }
        if (chamberEmpty) {
            return;
        }
        currentAmmo--;

        if (currentAmmo <= 0) {
            chamberEmpty = true;
        }
        updateChamberAndCharge();
        updateAmmoText();

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
        
        GameObject bullet = Instantiate(ammoType, firePoint.transform.position, transform.rotation);
        bullet.GetComponent<Rigidbody>().velocity = shootingDirection * muzzleVelocity;
        bullet.GetComponent<BaseBullet>().source = gameObject;

        audioSource.PlayOneShot(fireSound);
    }

    private void updateAmmoText() {
        if (ammoText == null) {
            return;
        }   
        ammoText.text = currentAmmo.ToString();
        while (ammoText.text.Length < 2) {
            ammoText.text = "0" + ammoText.text;
        }
    }

    private void updateChamberAndCharge() {
        if (chamberUI == null) {
            return;
        }
        if (chamberEmpty) {
            chamberUI.GetComponent<SVGImage>().color = new Color(1f, 1f, 1f, 0.5f);
        }
        else {
            chamberUI.GetComponent<SVGImage>().color = new Color(1f, 1f, 1f, 1f);
        }
    }
}
