using UnityEngine;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine.UI;

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
        currentWeapon = playerInventory.currentWeapon;

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

    private void Update () {
        if (currentWeapon == null) {
            currentWeapon = playerInventory.currentWeapon;
        }
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
        int currentAmmo = currentGun.currentAmmoInMag + currentGun.currentAmmoInChamber;
        ammoCountUI.text = currentAmmo.ToString();
        while (ammoCountUI.text.Length < 2) {
            ammoCountUI.text = "0" + ammoCountUI.text;
        }
    }

    private float currentReloadRotation = 0;
    private void UpdateChamberUI() {
        if (currentGun.currentAmmoInChamber <= 0) {
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