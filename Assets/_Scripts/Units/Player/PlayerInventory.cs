using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public GameObject weaponPrefab;
    [NonSerialized] public GameObject currentWeapon;
    private void Start()
    {
        holdWeapon();
    }

    private void Update()
    {
        if (currentWeapon == null)
        {
            holdWeapon();
        }
    }
    void holdWeapon() {
        currentWeapon = Instantiate(weaponPrefab, transform.position, transform.rotation, transform);
        BaseGun gun = currentWeapon.GetComponent<BaseGun>();
        if (gun != null) {
            gun.playerPosition = transform;
        }
    }
}
