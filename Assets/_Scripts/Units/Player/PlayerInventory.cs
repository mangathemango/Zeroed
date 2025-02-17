using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public GameObject[] weaponSlots = new GameObject[2];
    [NonSerialized] public GameObject currentWeapon;
    
    private int currentIndex = -1;
    private void Start()
    {
        InstantiateAllWeapons();
        EquipWeapon(0);
    }

    public void EquipWeapon(int index) {
        if (index < 0 || index >= weaponSlots.Length) {
            return;
        }
        if (currentIndex == index) {
            return;
        }
        if (currentWeapon != null) {
            currentWeapon.SetActive(false);
        }
        currentWeapon = weaponSlots[index];
        currentWeapon.SetActive(true);
        currentIndex = index;
        BaseGun gun = currentWeapon.GetComponent<BaseGun>();
        if (gun != null) {
            gun.playerTransform = transform;
        }
    }

    private void InstantiateAllWeapons() {
        for (int i = 0; i < weaponSlots.Length; i++) {
            weaponSlots[i] = Instantiate(weaponSlots[i], transform.position, transform.rotation, transform);
            weaponSlots[i].SetActive(false);
        }
    }
}
