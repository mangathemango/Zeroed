using System;
using System.Collections;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public GameObject[] weaponSlots = new GameObject[2];
    [NonSerialized] public GameObject currentWeapon;
    private Rigidbody rb;
    
    private int currentIndex = -1;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        InstantiateAllWeapons();
        StartCoroutine(EquipWeapon(0));
    }

    public IEnumerator EquipWeapon(int index) {
        if (index < 0 || index >= weaponSlots.Length) {
            yield break;
        }
        if (currentIndex == index) {
            yield break;
        }
        if (currentWeapon != null) {
            currentWeapon.SetActive(false);
            yield return new WaitForSeconds(currentWeapon.GetComponent<BaseGun>().switchTimeSeconds);
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
            rb.mass += weaponSlots[i].GetComponent<BaseGun>().mass;
        } 
    }
}
