using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// * The class that manages the player's inventory<br/><br/>
/// ? Currently, this only takes care of the player's weapons. Other items will be included later on<br/>
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    public GameObject[] weaponSlots = new GameObject[2];
    private Rigidbody rb;
    private int currentWeaponIndex = 0;
    public GameObject CurrentWeapon {
        get {return weaponSlots[currentWeaponIndex];}
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        InstantiateAllWeapons();
        StartCoroutine(EquipWeapon(0));
    }

    /// <summary>
    /// * Equip a weapon from the player's weapon slots<br/><br/>
    /// ? Note: Switching weapons is delayed by the current weapon switch time AND the target weapon switch time<br/>
    /// </summary>
    /// <param name="targetWeaponIndex">The index of the weapon inside weaponSlots array</param>
    public IEnumerator EquipWeapon(int targetWeaponIndex) {
        // If the player has no current weapon, equip the target weapon immediately
        if (CurrentWeapon.activeSelf == false) {
            currentWeaponIndex = targetWeaponIndex;
            CurrentWeapon.SetActive(true);
            yield break;
        } 

        // Array bounds check
        if (targetWeaponIndex < 0 || targetWeaponIndex >= weaponSlots.Length) {
            yield break;
        }

        // If the player is already using the target weapon, do nothing
        if (currentWeaponIndex == targetWeaponIndex) {
            yield break;
        }
        
        // Switch time is the sum of the current weapon's switch time and the target weapon's switch time
        float switchTime = weaponSlots[targetWeaponIndex].GetComponent<BaseGun>().switchTimeSeconds +
                           CurrentWeapon.GetComponent<BaseGun>().switchTimeSeconds;

        // Disable current weapon
        CurrentWeapon.SetActive(false);

        // Switch to target weapon
        //? Note: The weapon is switched immediately, but the weapon can only be fired after [switch time] has passed
        currentWeaponIndex = targetWeaponIndex;

        yield return new WaitForSeconds(switchTime);
        //! This set active thing may cause some scaling issues later, but it's fine for now
        CurrentWeapon.SetActive(true);
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
