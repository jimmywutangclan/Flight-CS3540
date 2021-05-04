using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float reach = 5f;
    public float regularSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 10;
    public float gravity = 10;
    public float airControl = 10;
    // how steep the angle the player can be on and still be able to jump
    // this prevents skyrim-like jump spamming up steep cliffs
    public float jumpAngle = 40f;

    public float swingStaminaRequired = 10f;

    public UIManager uIManager;

    [SerializeField]
    private Image reticle;
    [SerializeField]
    private Color reticleEnemyColor;

    CharacterController controller;
    Vector3 input;
    Vector3 moveDirection;

    private bool isJumping = false;
    private bool isSwinging = false;
    private bool hasActivatedSwingEffect = false;
    private bool lockSprint = false;

    private Animator anim;
    private float totalSwingTime;
    private float currentSwingTime;
    private float currentSpeed;

    private WorldSettings worldSettings;

    private RaycastController raycast;
    private ItemEquipper equipper;
    private PlayerAttack attack;
    private TreeController treeController;
    private CameraAdjuster cameraAdjuster;
    private PlayerStatistics playerStatistics;
    private SprintAdjuster equipped;
    private ItemCollector itemCollector;

    private Color reticleMissColor;

    // Start is called before the first frame update
    void Start()
    {
        worldSettings = FindObjectOfType<WorldSettings>();
        controller = GetComponent<CharacterController>();
        worldSettings = FindObjectOfType<WorldSettings>();
        anim = GetComponent<Animator>();

        raycast = GetComponent<RaycastController>();
        if (raycast != null)
        {
            int inventoryLayerMask = 1 << LayerMask.NameToLayer("Enemy");
            raycast.AddLayerMask(inventoryLayerMask);
        }

        equipper = GetComponent<ItemEquipper>();
        attack = GetComponent<PlayerAttack>();
        treeController = GetComponent<TreeController>();
        cameraAdjuster = GetComponent<CameraAdjuster>();
        playerStatistics = GetComponent<PlayerStatistics>();
        itemCollector = GetComponent<ItemCollector>();

        totalSwingTime = GetSwimgTimeByAnimator(anim);

        reticleMissColor = reticle.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (!worldSettings.IsGameOver())
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            input = Vector3.ClampMagnitude((transform.right * moveHorizontal + transform.forward * moveVertical), 1f);

            input *= regularSpeed;

            HandleSprint();

            if (controller.isGrounded)
            {
                isJumping = false;

                moveDirection = HandleSlide(input);

                if (moveDirection.y == input.y && Input.GetButton("Jump")) // If we aren't sliding and try to jump
                {
                        isJumping = true;
                        moveDirection.y = Mathf.Sqrt(2 * jumpHeight * gravity);
                }
            }
            else
            {
                // we are midair
                input.y = moveDirection.y;
                moveDirection = Vector3.Lerp(moveDirection, input, Time.deltaTime * airControl);
            }

            /* Control jump and animations. */
            if (Input.GetButtonDown("Fire1") && !isSwinging) // Handle's player clicking
                HandlePlayerClick();
            else if (isSwinging) // If the player is currently swinging
                anim.SetInteger("activeState", 5);
            else if (isJumping) // If the player is in midair
            {
                // do jump animation, for now it is just idle.
                anim.SetInteger("activeState", 0);
            }
            else if (Input.GetKey(KeyCode.W) && playerStatistics.IsSprinting)
                anim.SetInteger("activeState", 6);
            else if (Input.GetKey(KeyCode.W)) // Walk forward anim
                anim.SetInteger("activeState", 1);
            else if (Input.GetKey(KeyCode.S)) // Walk back anim
                anim.SetInteger("activeState", 2);
            else if (Input.GetKey(KeyCode.A)) // Walk left anim
                anim.SetInteger("activeState", 3);
            else if (Input.GetKey(KeyCode.D)) // Walk right anim
                anim.SetInteger("activeState", 4);
            else // Idle anim
            {
                anim.SetInteger("activeState", 0);
                GetComponentInChildren<RotateOnFire>().SetDoRotate(false);
            }

            if (isSwinging)
            {
                currentSwingTime += Time.deltaTime;

                if (currentSwingTime >= totalSwingTime / 2 && !hasActivatedSwingEffect)
                {
                    ActivateSwingEffect();
                }

                if (currentSwingTime >= totalSwingTime)
                {
                    isSwinging = false;
                }
            }

            moveDirection.y -= gravity * Time.deltaTime;

            controller.Move(moveDirection * Time.deltaTime);

            ReticleEffect();
        }
    }

    private void ReticleEffect()
    {
        RaycastHit hit = raycast.GetHitObject();

        if (hit.collider != null && Vector3.Distance(transform.position, hit.point) < reach &&
            (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
            hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactive")))
        {
            reticle.color = Color.Lerp(reticle.color, reticleEnemyColor, Time.deltaTime * 2);
            reticle.transform.localScale = Vector3.Lerp(reticle.transform.localScale, new Vector3(.7f, .7f, 1f), Time.deltaTime * 2);
        }
        else
        {
            //currentProjectilePrefab = arrowPrefab;
            reticle.color = Color.Lerp(reticle.color, reticleMissColor, Time.deltaTime * 2);
            reticle.transform.localScale = Vector3.Lerp(reticle.transform.localScale, Vector3.one, Time.deltaTime * 2);
        }

    }

    private Vector3 HandleSlide(Vector3 inp)
    {
        Vector3 move = inp;

        Physics.SphereCast(transform.position, 0.5f, Vector3.down, out RaycastHit ground, 1f);

        // gets a flat vector to compare to
        Vector3 levelGround = new Vector3(ground.normal.x, 0, ground.normal.z);

        float groundAngle = 90f - Vector3.Angle(ground.normal, levelGround);

        // If steep slope, slide down. Prohibits jumping if the angle the player is on is
        // too steep; prevents skyrim-like jump spamming up steep cliffs
        if (groundAngle >= jumpAngle)
        {
            // slides in the direction of the slope going down
            Vector3 slideDirection = (levelGround.normalized - ground.normal) * currentSpeed * 2;
            move += slideDirection;
        }

        return move;
    }

    private void HandlePlayerClick()
    {
        if (uIManager.UIShown) return;

        RaycastHit hit = raycast.GetHitObject();
        if (equipper.EquippedItem != null && equipper.EquippedItem.Properties.Consumable) // Consume the item
        { }
        else if (hit.collider != null && hit.collider.CompareTag("Inventory")) // Pickup item
        { }
        else // Swing item
        {
            if (!playerStatistics.StaminaCooldown) // if the player isn't on stamina cooldown
            {
                anim.SetInteger("activeState", 5);
                GetComponentInChildren<RotateOnFire>().SetDoRotate(true);
                currentSwingTime = 0;
                hasActivatedSwingEffect = false;
                isSwinging = true;

                // Removes stamina on swing
                playerStatistics.ChangePlayerStamina(-1 * swingStaminaRequired);
            }

        }
    }

    private void HandleSprint()
    {
        // Handles sprinting and its effects. Currently still applies if the player is midair.
        // The second check ensures that the player does not start sprinting if less than a minimum threshold,
        // but that if it already was sprinting it can continue to do so.
        if (Input.GetKey(KeyCode.LeftShift) && !lockSprint && (playerStatistics.IsSprinting || playerStatistics.CanStartSprinting()))
        {
            currentSpeed = sprintSpeed;
            if (input.magnitude != 0) // If the player is moving
            {
                // Adds the sprint only to the forward direction
                Vector3 forwardMovement = Vector3.Project(input, transform.forward);
                Vector3 sideMovement = input - forwardMovement;
                forwardMovement *= sprintSpeed / regularSpeed;
                input = forwardMovement + sideMovement;

                // Updates camera position to match animation
                cameraAdjuster.SetSprintPosition();

                // Updates item position to avoid visual shearing
                if (equipped != null)
                    equipped.SetSprint();
                else
                    equipped = GameObject.FindGameObjectWithTag("ItemHoldPoint").GetComponentInChildren<SprintAdjuster>();

                playerStatistics.IsSprinting = true;
            }
            else // Resets camera position if the player is no longer moving and sprinting
            {
                cameraAdjuster.ResetCameraPosition();
                if (equipped != null)
                    equipped.SetNormal();
                else
                    equipped = GameObject.FindGameObjectWithTag("ItemHoldPoint").GetComponentInChildren<SprintAdjuster>();

                playerStatistics.IsSprinting = false;
            }
        }
        else
        {
            currentSpeed = regularSpeed;
            cameraAdjuster.ResetCameraPosition();
            if (equipped != null)
                equipped.SetNormal();
            else
                equipped = GameObject.FindGameObjectWithTag("ItemHoldPoint").GetComponentInChildren<SprintAdjuster>();

            playerStatistics.IsSprinting = false;

            // Prevents player from holding down shift and sprinting as soon as stamina regens.
            if (Input.GetKey(KeyCode.LeftShift))
                lockSprint = true;
        }

        // Unlocks sprint if it is locked and player releases shift.
        if (lockSprint && Input.GetKeyUp(KeyCode.LeftShift))
            lockSprint = false;
    }


    private void ActivateSwingEffect()
    {
        RaycastHit hit = raycast.GetHitObject();

        hasActivatedSwingEffect = true;

        if (hit.collider != null)
        {
            // If player hits a tree with an axe
            if (hit.collider.CompareTag("TreeCollider"))
            {
                treeController.MineTree(hit.collider);
            }
            else if (hit.collider.CompareTag("RockCollider"))
            {
                itemCollector.AddItemFromSource(hit.collider.gameObject.GetComponent<RockController>());
            }
            else if (hit.collider.CompareTag("Bush"))
            {
                itemCollector.AddItemFromSource(hit.collider.gameObject.GetComponent<ItemSource>());
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")) // If player swings at an enemy
            {
                attack.Attack(hit.collider.gameObject);
            }
        }
    }
    public float GetReach()
    {
        return reach;
    }

    public void PlayerDies()
    {
        transform.rotation *= Quaternion.Euler(0, 0, -90);
        worldSettings.SetGameOver();
        treeController.ReinitializeTrees();
        GetComponentInChildren<MouseLook>().SetDeathRotation();
    }

    private float GetSwimgTimeByAnimator(Animator animator)
    {
        float animTime = 0;

        RuntimeAnimatorController runtimeAC = anim.runtimeAnimatorController;
        for (int i = 0; i < runtimeAC.animationClips.Length; i++)
        {
            if (runtimeAC.animationClips[i].name == "SwingSword")
            {
                animTime = runtimeAC.animationClips[i].length;
            }
        }

        return animTime;
    }
}
