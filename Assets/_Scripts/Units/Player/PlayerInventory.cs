using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// * The class that manages the player's inventory<br/><br/>
/// ? Currently, this only takes care of the player's weapons. Other items will be included later on<br/>
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private GameObject[] weaponSlots = new GameObject[3];
    private int currentWeaponIndex = 0;
    private int currentWeaponHeldIndex = 0;
    private float switchTimer = 0;
    private Rigidbody rb;
    public GameObject CurrentWeapon {
        get {return weaponSlots[currentWeaponIndex];}
    }

    // TODO: This will need better caching to avoid calling GetComponent() every frame
    private float CurrentWeaponSwitchTime {
        get {return CurrentWeapon.GetComponent<BaseGun>().switchTimeSeconds;}
    }

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        InstantiateAllWeapons();
        EquipWeapon(0);
    }

    /// <summary>
    /// * Equip a weapon from the player's inventory<br/><br/>
    /// ? The delay between switching weapons is handled by the atrocity that is ActivateCurrentWeapon() down there<br/>
    /// </summary>
    /// <param name="weaponIndex">The index of the gun inside the weaponSlots[] array</param>
    public void EquipWeapon(int weaponIndex) {
        if (weaponSlots[weaponIndex] == null) {
            return;
        }
        
        if (weaponIndex == currentWeaponIndex) {
            return;
        }

        CurrentWeapon.GetComponent<BaseGun>().Disable();
        currentWeaponIndex = weaponIndex;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!CurrentWeapon.activeInHierarchy) {
            ActivateCurrentWeapon();
        }
    }

    /// <summary>
    /// * Activates the current weapon<br/><br/>
    /// ? This function is a state machine that handles the activation of the current weapon after being switched to.<br/>
    /// ! Note that currentWeaponIndex is the index of the weapon that the player *wants* to switch to, <br/>
    /// ! not the weapon that the player is currently holding. <br/>
    /// ! When switching between weapons, the value of currentWeaponIndex is changed immediately. <br/>
    /// ! However, the actual gun GameObject (aka CurrentWeapon) is only activated after a certain delay.<br/><br/>
    /// 
    /// ? An example: We call EquipWeapon(1) to switch from gun 0 (index 0) to gun 1 (index 1).<br/>
    /// ? This means that currentWeaponHeldIndex = 0, currentWeaponIndex = 1, and CurrentWeapon is inactive.
    /// ? There are 3 states that we will go through:<br/>
    /// ?   1. Player is holding gun 0   (currentWeaponHeldIndex != currentWeaponIndex, which procs HandleWeaponSwitch())<br/>
    /// ?   2. Gun 0 is put away         (HandleWeaponSwitch() turns currentWeaponHeldIndex to 1, which procs HandleWeaponActivation())<br/>
    /// ?   3. Player takes out gun 1    (HandleWeaponActivation() sets CurrentWeapon to active)<br/>
    /// </summary>
    private void ActivateCurrentWeapon() {
        if (currentWeaponHeldIndex != currentWeaponIndex) {
            HandleWeaponSwitch();
        } else {
            HandleWeaponActivation();
        }
    }

    /// <summary>
    /// * Handles the weapon switch, used by the function ActivateCurrentWeapon()<br/><br/>
    /// ? This function sets currentWeaponHeldIndex to currentWeaponIndex after a certain delay of [switchTimer]<br/>
    /// ? The reason why the [switchTimer] variable is used instead of WaitForSeconds() is too complicated to be explained with words.<br/>
    /// ? Just read HandleWeaponActivation() and try to figure it out yourself<br/>
    /// </summary>
    private void HandleWeaponSwitch() {
        if (switchTimer > 0) {
            switchTimer -= Time.deltaTime;
        } else {
            switchTimer = 0;
            currentWeaponHeldIndex = currentWeaponIndex;
        }
    }

    /// <summary>
    /// * Handles the current weapon activation, used by the function ActivateCurrentWeapon()<br/><br/>
    /// ? This function sets the current weapon to active after a certain delay of [CurrentWeaponSwitchTime]<br/>
    /// ? At the same time, it's also setting the variable [switchTimer] to the time that has been waited so far.<br/>
    /// ? If you're still confused after allat, just contact Mango. I'll explain it with illustrations<br/>
    /// </summary>
    private void HandleWeaponActivation() {
        if (switchTimer < CurrentWeaponSwitchTime) {
            switchTimer += Time.deltaTime;
        } else {
            switchTimer = CurrentWeaponSwitchTime;
            CurrentWeapon.GetComponent<BaseGun>().Enable();
        }
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
