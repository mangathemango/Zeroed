using UnityEngine;

public class PlayerInputHandler: Singleton<PlayerInputHandler>
{
    PlayerInventory playerInventory;
    PlayerMovement movement;
    BaseGun currentGun;
    private void Start()
    {
        playerInventory = GetComponent<PlayerInventory>();
        movement = GetComponent<PlayerMovement>();
    }
    private void Update()
    {
        // Gun controls
        currentGun = playerInventory.CurrentWeapon.GetComponent<BaseGun>();
        if (Input.GetMouseButtonDown(0)) {
            currentGun.PressTrigger();
        }
        if (Input.GetMouseButton(1)) {
            currentGun.AimDownSight();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            currentGun.Reload();
        }
        if (Input.GetMouseButtonDown(2)) {
            currentGun.Charge();
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            currentGun.Melee();
        }
        if (Input.GetKeyDown(KeyCode.V)) {
            currentGun.SwitchFireMode();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            playerInventory.EquipWeapon(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            playerInventory.EquipWeapon(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            playerInventory.EquipWeapon(2);
        }

        // Player Controls
        movement.moveHorizontal = Input.GetAxisRaw("Horizontal");
        movement.moveVertical = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            movement.Dash();
        }
    }
}