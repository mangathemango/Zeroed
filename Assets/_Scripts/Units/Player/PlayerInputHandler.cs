using UnityEngine;

public class PlayerInputHandler: Singleton<PlayerInputHandler>
{
    PlayerInventory playerInventory;
    PlayerMovement movement;
    BaseGun gun;
    private void Start()
    {
        playerInventory = GetComponent<PlayerInventory>();
        movement = GetComponent<PlayerMovement>();
    }
    private void Update()
    {
        // Gun controls
        gun = playerInventory.currentWeapon.GetComponent<BaseGun>();
        if (Input.GetMouseButtonDown(0)) {
            StartCoroutine(gun.PressTrigger());
        }
        if (Input.GetMouseButton(1)) {
            StartCoroutine(gun.AimDownSight());
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            StartCoroutine(gun.Reload());
        }
        if (Input.GetMouseButtonDown(2)) {
            StartCoroutine(gun.Charge());
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            gun.Melee();
        }
        if (Input.GetKeyDown(KeyCode.V)) {
            gun.SwitchFireMode();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            StartCoroutine(playerInventory.EquipWeapon(0));
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            StartCoroutine(playerInventory.EquipWeapon(1));
        }

        // Player Controls
        movement.moveHorizontal = Input.GetAxisRaw("Horizontal");
        movement.moveVertical = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(movement.Dash());
        }
    }
}