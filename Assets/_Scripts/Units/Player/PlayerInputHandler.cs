using UnityEngine;

public class PlayerInputHandler: Singleton<PlayerInputHandler>
{
    Player player;
    BaseGun gun;
    private void Start()
    {
        if (!GetComponent<Player>())
        {
            player = gameObject.AddComponent<Player>();
        }
        else
        {
            player = GetComponent<Player>();
        }
    }
    private void Update()
    {
        // Gun controls
        gun = player.currentWeapon.GetComponent<BaseGun>();
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
            gun.switchFireMode();
        }

        // Player Controls
        player.moveHorizontal = Input.GetAxisRaw("Horizontal");
        player.moveVertical = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(player.Dash());
        }
    }
}