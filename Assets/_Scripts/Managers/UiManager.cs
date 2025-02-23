using UnityEngine;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine.UI;

/// <summary>
/// * Updates the UI elements of the game
/// </summary>
public class UiUpdater: Singleton<UiUpdater> {
    private PlayerInventory playerInventory;
    private GameObject currentWeapon; 
    private BaseGun currentGun;
    [SerializeField] private TextMeshProUGUI ammoCountUI;
    [SerializeField] private TextMeshProUGUI fireModeUI;
    [SerializeField] private TextMeshProUGUI gunNameUI;
    [SerializeField] private SVGImage chamberUI;
    [SerializeField] private SVGImage chargeUI;
    [SerializeField] private RawImage ReloadingUI;
    private void Start () {

        if (playerInventory == null) {
            playerInventory = GameObject.Find("Player").GetComponent<PlayerInventory>();
        }
        if (playerInventory == null) {
            Debug.LogError("Player not found!");
        }
    }

    private void Update () {
        currentWeapon = playerInventory.CurrentWeapon;
        if (currentWeapon.GetComponent<BaseGun>() != null) {
            currentGun = currentWeapon.GetComponent<BaseGun>();
        }
        if (currentGun == null) {
            Debug.LogError("Gun not found!");
        }
        if (currentGun != null) {
            UpdateGunUI();
        }
    }
    private void UpdateGunUI () {
        UpdateAmmoText();
        UpdateChamberUI();
        UpdateFireMode();
        UpdateGunName();
    }
    private void UpdateAmmoText() {
        ammoCountUI.text = currentGun.getTotalAmmo.ToString();
        while (ammoCountUI.text.Length < 2) {
            ammoCountUI.text = "0" + ammoCountUI.text;
        }
    }

    private float currentReloadRotation = 0;
    private void UpdateChamberUI() {
        if (!currentGun.hasBulletInChamber) {
            chamberUI.color = new Color(1f, 1f, 1f, 0.5f);
            chargeUI.color = new Color(1f, 1f, 1f, 0.5f);
        }
        else {
            chamberUI.color = new Color(1f, 1f, 1f, 1f);
            chargeUI.color = new Color(1f, 1f, 1f, 1f);
        }
        if (currentGun.reloading) {
            chargeUI.color = new Color(1f, 1f, 1f, 0f);
            ReloadingUI.color = new Color(1f, 1f, 1f, 1f);
            currentReloadRotation += 1; 
            ReloadingUI.transform.rotation = Quaternion.Euler(0, 0, currentReloadRotation);
        } else {
            ReloadingUI.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    private void UpdateFireMode() {
        fireModeUI.text = currentGun.currentFireMode.ToString().ToUpper();
    }

    private void UpdateGunName() {
        gunNameUI.text = currentGun.gunName;
    }
}