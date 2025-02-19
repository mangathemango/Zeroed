using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// * The class that manages the player's inventory<br/><br/>
/// ? Currently, this only takes care of the player's weapons. Other items will be included later on<br/>
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    public GameObject[] weaponSlots = new GameObject[3];
    private Rigidbody rb;
    private int currentWeaponIndex = 0;
    private float switchTimer = 0;
    private int currentWeaponPathIndex = 0;
    public GameObject CurrentWeapon {
        get {return weaponSlots[currentWeaponIndex];}
    }

    private float CurrentWeaponSwitchTime {
        get {return CurrentWeapon.GetComponent<BaseGun>().switchTimeSeconds;}
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        InstantiateAllWeapons();
        EquipWeapon(0);
    }

    private void Update()
    {
        UpdateWeaponSwitching();
    }

    private void UpdateWeaponSwitching() {
        if (currentWeaponPathIndex != currentWeaponIndex) {
            switchTimer -= Time.deltaTime;
            if (switchTimer <= 0) {
                switchTimer = 0;
                currentWeaponPathIndex = currentWeaponIndex;
            }
        }

        if (currentWeaponPathIndex == currentWeaponIndex) {
            if (switchTimer < CurrentWeaponSwitchTime) {
                switchTimer += Time.deltaTime;
            } 
            if (switchTimer >= CurrentWeaponSwitchTime) {
                CurrentWeapon.SetActive(true);
            }
        }
    }

    public void EquipWeapon(int weaponIndex) {
        if (weaponIndex == currentWeaponIndex) {
            return;
        }
        CurrentWeapon.SetActive(false);
        currentWeaponIndex = weaponIndex;
    }

    /// <summary>
    /// * Instantiate all weapons in the player's weapon slots<br/><br/>
    /// ? This is called in Start() to ensure that all weapons are instantiated before the game starts<br/>
    /// </summary>
    private void InstantiateAllWeapons() {
        for (int i = 0; i < weaponSlots.Length; i++) {
            // Instantiate the weapon
            weaponSlots[i] = Instantiate(weaponSlots[i], transform.position, transform.rotation, transform);
            weaponSlots[i].SetActive(false);
            BaseGun gun = weaponSlots[i].GetComponent<BaseGun>();
            if (gun != null) {
                gun.Initialize();
            }
            // Add the weapon's mass to the player's mass
            rb.mass += gun.mass;
        } 
    }
}
