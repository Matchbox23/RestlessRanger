using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class playerScript : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction strafeAction;
    private InputAction jumpAction;
    private InputAction redStanceAction;
    private InputAction greenStanceAction;
    private InputAction blueStanceAction;
    private InputAction powerAction;

    public Swipe swipe;

    private Rigidbody playerBody;
    private Transform tr;
    public bool alive = true;

    private bool grounded = false;
    public Vector3 jumpVelocity = new Vector3(0, 10, 0);
    public float moveSpeed = 0.1f;

    public int greenEnergy = 0;
    public int redEnergy = 0;
    public int blueEnergy = 0;
    public int score = 0;
    public int stance = 0;
    public bool invulnerable = false;
    public float invulnerableTimer = 0;
    public float maxInvulnerable = 0.5f;
    public float acceleration;
    public GameObject shield;
    public GameObject multiplier;

    public Material redSkin;
    public Material greenSkin;
    public Material blueSkin;
    public Material noSkin;
    private MeshRenderer skin;

    private Vector3[] lanes;
    private int targetLane = 1;

    private GameObject gm;
    GameManager gameM;

    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 10f;
    }

    private void Awake()
    {
        gm = GameObject.Find("SceneCore");
        gameM = gm.GetComponent<GameManager>();

        playerInput = GetComponent<PlayerInput>();
        tr = GetComponent<Transform>();
        tr.position = new Vector3(0, 0, 0);

        shield = this.gameObject.transform.GetChild(0).gameObject;
        multiplier = this.gameObject.transform.GetChild(1).gameObject;

        playerBody = GetComponent<Rigidbody>();
        jumpAction = playerInput.actions["Jump"];
        strafeAction = playerInput.actions["Strafe"];
        redStanceAction = playerInput.actions["RedStance"];
        greenStanceAction = playerInput.actions["GreenStance"];
        blueStanceAction = playerInput.actions["BlueStance"];
        powerAction = playerInput.actions["Power"];

        lanes = new Vector3[3];
        lanes[0] = new Vector3(-4, 0, 0);
        lanes[1] = new Vector3(0, 0, 0);
        lanes[2] = new Vector3(4, 0, 0);

        skin = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        playerInput.enabled = true;
        jumpAction.performed += jump;
        strafeAction.performed += strafe;
        redStanceAction.performed += redStance;
        greenStanceAction.performed += greenStance;
        blueStanceAction.performed += blueStance;
        powerAction.performed += power;
    }

    private void OnDisable()
    {
        playerInput.enabled = false;
        jumpAction.performed -= jump;
        strafeAction.performed -= strafe;
        redStanceAction.performed -= redStance;
        greenStanceAction.performed -= greenStance;
        blueStanceAction.performed -= blueStance;
        powerAction.performed -= power;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if(redEnergy<5) redEnergy++;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if(greenEnergy<5) greenEnergy++;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if(blueEnergy<5) blueEnergy++;
        }

        if (!gameM.gameIsPaused)
        {
            moveSpeed += acceleration * Time.deltaTime;
        }
        if (invulnerable) 
        {
            invulnerableTimer += Time.deltaTime;
        }
            
        if(invulnerableTimer > maxInvulnerable && invulnerable)
        {
            invulnerable = false;
            invulnerableTimer = 0;
        }
        if (!alive)
        {
            gameM.gameIsPaused = true;
            Time.timeScale = 0;
        }

        if (swipe.SwipeLeft)
        {
            if (tr.position.x <= 0)
            {
                targetLane = 0;
            }
            if (tr.position.x > 0)
            {
                targetLane = 1;
            }
        }
        else if(swipe.SwipeRight)
        {
            if (tr.position.x >= 0)
            {
                targetLane = 2;
            }
            if (tr.position.x < 0)
            {
                targetLane = 1;
            }
        }

        Vector3 target = new Vector3(lanes[targetLane].x, tr.position.y, lanes[targetLane].y);
        tr.position = Vector3.MoveTowards(tr.position, target, moveSpeed * Time.deltaTime);
    }

    private void jump(InputAction.CallbackContext context)
    {
        jump();
    }

    public void jump()
    {
        if (redEnergy >= 5 && greenEnergy >= 5 && blueEnergy >= 5)
        {
            gameM.playSound("explosion2");
            nuke();
            redEnergy -= 5;
            greenEnergy -= 5;
            blueEnergy -= 5;
            stance = 0;
            skin.material = noSkin;
        }
        else
        {
            gameM.playSound("error");
        }
    }

    private void strafe(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() < 0)
        {
            if (tr.position.x <= 0)
            {
                targetLane = 0;
            }
            if (tr.position.x > 0)
            {
                targetLane = 1;
            }
        }
        else if(context.ReadValue<float>() > 0)
        {
            if (tr.position.x >= 0)
            {
                targetLane = 2;
            }
            if (tr.position.x < 0)
            {
                targetLane = 1;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            grounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            grounded = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //WALL
        if (other.CompareTag("wall") && !invulnerable)
        {
            if(stance != 0 && !shield.activeInHierarchy)
            {
                Debug.Log("Lost Stance!");
                Destroy(other.gameObject);
                if (stance == 1)
                {
                    redEnergy--;
                }
                if (stance == 2)
                {
                    greenEnergy--;
                }
                if (stance == 3)
                {
                    blueEnergy--;
                }
                stance = 0;
                skin.material = noSkin;
                invulnerable = true;
                multiplier.SetActive(false);
                gameM.playSound("explosion2");
            }
            else if (shield.activeInHierarchy)
            {
                Destroy(other.gameObject);
                Debug.Log("Broke Shield!");
                shield.SetActive(false);
                invulnerable = true;
                gameM.playSound("glass");
            }
            else if (stance == 0 && !shield.activeInHierarchy)
            {
                Debug.Log("Died!");
                Destroy(other.gameObject);
                Time.timeScale = 0;
                gameM.gameIsPaused = true;
                alive = false;
            }
        }

        //ORB
        int scoreFactor = 1;
        int energyFactor = 1;
        if (multiplier.activeInHierarchy)
        {
            scoreFactor = 5;
            energyFactor = 2;
        }
        if (other.CompareTag("redOrb"))
        {
            Destroy(other.gameObject);

            //Only gain energy if not in same stance
            if(stance != 1)
            {
                redEnergy += 1 * energyFactor;
            }
            if(redEnergy > 5)
            {
                redEnergy = 5;
            }

            //Gain more score if in the same stance
            if(stance == 1)
            {
                score += 1 * 2 * (scoreFactor);
            }
            else
            {
                score += 1 * (scoreFactor);
            }

            multiplier.SetActive(false);
            gameM.playSound("orbpickup");
        }
        if (other.CompareTag("greenOrb"))
        {
            Destroy(other.gameObject);

            //Only gain energy if not in same stance
            if (stance != 2)
            {
                greenEnergy += 1 * energyFactor;
            }
            if (greenEnergy > 5)
            {
                greenEnergy = 5;
            }

            //Gain more score if in the same stance
            if (stance == 2)
            {
                score += 1 * 2 * (scoreFactor);
            }
            else
            {
                score += 1 * (scoreFactor);
            }

            multiplier.SetActive(false);
            gameM.playSound("orbpickup");
        }
        if (other.CompareTag("blueOrb"))
        {
            Destroy(other.gameObject);

            //Only gain energy if not in same stance
            if (stance != 3)
            {
                blueEnergy += 1 * energyFactor;
            }
            if (blueEnergy > 5)
            {
                blueEnergy = 5;
            }

            //Gain more score if in the same stance
            if (stance == 3)
            {
                score += 1 * 2 * (scoreFactor);
            }
            else
            {
                score += 1 * (scoreFactor);
            }

            multiplier.SetActive(false);
            gameM.playSound("orbpickup");
        }
    }

    void redStance(InputAction.CallbackContext context)
    {
        redStance();
    }

    void greenStance(InputAction.CallbackContext context)
    {
        greenStance();
    }

    void blueStance(InputAction.CallbackContext context)
    {
        blueStance();
    }

    public void redStance()
    {
        if (redEnergy == 5)
        {
            stance = 1;
            skin.material = redSkin;
            multiplier.SetActive(false);
            shield.SetActive(false);
            gameM.playSound("stancechange");
        }
        else
        {
            gameM.playSound("error");
        }
    }

    public void greenStance()
    {
        if (greenEnergy == 5)
        {
            stance = 2;
            skin.material = greenSkin;
            shield.SetActive(false);
            gameM.playSound("stancechange");
        }
        else
        {
            gameM.playSound("error");
        }
    }

    public void blueStance()
    {
        if (blueEnergy == 5)
        {
            stance = 3;
            skin.material = blueSkin;
            multiplier.SetActive(false);
            gameM.playSound("stancechange");
        }
        else
        {
            gameM.playSound("error");
        }
    }

    void power(InputAction.CallbackContext context)
    {
        usePower();
    }

    public void usePower()
    {
        if (stance == 1 && grounded)
        {
            redEnergy--;
            playerBody.velocity += jumpVelocity;
            if (redEnergy == 0)
            {
                stance = 0;
                skin.material = noSkin;
            }
            Debug.Log("Leaped!");
        }
        if (stance == 2 && !multiplier.activeInHierarchy)
        {
            greenEnergy--;
            if (greenEnergy == 0)
            {
                stance = 0;
                skin.material = noSkin;
            }
            multiplier.SetActive(true);
            Debug.Log("Multiplier Active!");
        }
        if (stance == 3 && !shield.activeInHierarchy)
        {
            blueEnergy--;
            if (blueEnergy == 0)
            {
                stance = 0;
                skin.material = noSkin;
            }
            shield.SetActive(true);
            Debug.Log("Shield up!");
        }
        if (stance == 0)
        {
            gameM.playSound("error");
        }
        else
        {
            gameM.playSound("powerused");
        }
    }

    public void nuke()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("wall");
        for(int i = 0; i < walls.Length; i++)
        {
            Destroy(walls[i]);
        }
        gameM.playSound("explosion2");
    }

}
