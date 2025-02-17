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

    public IEnumerator EquipWeapon(int targetIndex) {
        if (targetIndex < 0 || targetIndex >= weaponSlots.Length) {
            yield break;
        }
        if (currentIndex == targetIndex) {
            yield break;
        }
        float switchTime = weaponSlots[targetIndex].GetComponent<BaseGun>().switchTimeSeconds;
        
        if (currentWeapon != null) {
            switchTime += currentWeapon.GetComponent<BaseGun>().switchTimeSeconds;
            currentWeapon.SetActive(false);
            currentWeapon = weaponSlots[targetIndex];
            yield return new WaitForSeconds(switchTime);
        } else {
            currentWeapon = weaponSlots[targetIndex];
        }

        weaponSlots[targetIndex ].SetActive(true);
        currentIndex = targetIndex;
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
