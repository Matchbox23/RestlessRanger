using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private PlayerInput playerInput;
    public playerScript player;
    public GameObject wall;
    public GameObject redOrb;
    public GameObject blueOrb;
    public GameObject greenOrb;
    public GameObject rowMarker;

    public float moveSpeed = 0.01f;

    private InputAction pauseAction;

    public int rows = 0;
    private Vector3[] lanes;
    private GameObject[] orbs;
    public float acceleration;

    public GameObject hud;
    public GameObject pauseMenu;
    public GameObject deathMenu;

    private float rowTimer = 0;
    private float rowCD;

    public TMP_Text redEnergyText;
    public TMP_Text greenEnergyText;
    public TMP_Text blueEnergyText;
    public TMP_Text scoreText;
    public TMP_Text deathMenuScoreText;

    public AudioClip error;
    public AudioClip explosion1;
    public AudioClip explosion2;
    public AudioClip orbpickup;
    public AudioClip powerused;
    public AudioClip stancechange;
    public AudioClip glass;
    public AudioSource audioS;
    public AudioSource audioS2;
    private float audioTimer = 0;
    private float audioCD = 0.5f;

    public bool gameIsPaused = false;

    private void Awake()
    {
        lanes = new Vector3[3];
        lanes[0] = new Vector3(-4, 0, 100);
        lanes[1] = new Vector3(0, 0, 100);
        lanes[2] = new Vector3(4, 0, 100);

        orbs = new GameObject[3];
        orbs[0] = redOrb;
        orbs[1] = greenOrb;
        orbs[2] = blueOrb;
        rowCD = 6f /moveSpeed;


        playerInput = player.GetComponent<PlayerInput>();
        pauseAction = playerInput.actions["Pause"];
    }

    private void OnEnable()
    {
        pauseAction.performed += pause;
    }

    private void OnDisable()
    {
        pauseAction.performed -= pause;
    }

    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 25;
        rows = 0;
        rowTimer = 0;
        audioTimer = 0;
        player.alive = true;
        Time.timeScale = 1;
        gameIsPaused = false;
        hud.SetActive(true);
        deathMenu.SetActive(false);
        player.redEnergy = 0;
        player.greenEnergy = 0;
        player.blueEnergy = 0;
        player.score = 0;
        gameIsPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameIsPaused)
        {
            moveSpeed += acceleration * Time.deltaTime;
        }
        rowCD = 25f / moveSpeed;
        rowTimer += Time.deltaTime;
        audioTimer += Time.deltaTime;
        if(rows < 8 && rowTimer > rowCD)
        {
            spawnRow();
        }
        if (!player.alive)
        {
            gameIsPaused = true;
            Time.timeScale = 0;
            hud.SetActive(false);
            deathMenu.SetActive(true);
            playSound("explosion1");
            deathMenuScoreText.text = player.score.ToString();
        }

        redEnergyText.text = player.redEnergy.ToString();
        greenEnergyText.text = player.greenEnergy.ToString();
        blueEnergyText.text = player.blueEnergy.ToString();
        scoreText.text = player.score.ToString();
    }

    void spawnRow()
    {
        rowTimer = 0;
        Instantiate(rowMarker, new Vector3(0, 0, 40), Quaternion.identity);
        rows++;
        int numberOfObstacles = Random.Range(0, 3);
        int numberOfOrbs = Random.Range(0, 3 - numberOfObstacles);

        bool[] occupiedLanes = new bool[3];
        Vector3 position;

        for(int i = 0; i < numberOfObstacles; i++)
        {
            int lane;

            //Choose the position of the wall here
            while (true){
                lane = Random.Range(0, 3);
                if (!occupiedLanes[lane])
                {
                    occupiedLanes[lane] = true;
                    break;
                }
            }
            position = new Vector3(lanes[lane].x, lanes[lane].y + 1.5f, lanes[lane].z);
            Instantiate(wall, position, Quaternion.identity);
        }

        for (int i = 0; i < numberOfOrbs; i++)
        {
            int lane;

            //Choose the position of the orb here
            while (true)
            {
                lane = Random.Range(0, 3);
                if (!occupiedLanes[lane])
                {
                    occupiedLanes[lane] = true;
                    break;
                }
            }
            position = new Vector3(lanes[lane].x, lanes[lane].y + 0.5f, lanes[lane].z);

            //Which orb?
            Instantiate(orbs[Random.Range(0, 3)], position, Quaternion.identity);
        }
    }

    void pause(InputAction.CallbackContext context)
    {
        togglePause();
    }

    public void togglePause()
    {
        if (!gameIsPaused && player.alive)
        {
            gameIsPaused = true;
            Time.timeScale = 0;
            hud.SetActive(false);
            pauseMenu.SetActive(true);
        }
        else if (gameIsPaused && player.alive)
        {
            gameIsPaused = false;
            Time.timeScale = 1;
            hud.SetActive(true);
            pauseMenu.SetActive(false);
        }
    }

    public void restart()
    {
        Time.timeScale = 1;
        gameIsPaused = false;
        SceneManager.LoadScene("Game");
    }

    public void mainMenu()
    {
        Time.timeScale = 1;
        gameIsPaused = false;
        SceneManager.LoadScene("Main Menu");
    }

    public void playSound(string audio)
    {
        if (audioTimer > audioCD)
        {
            audioTimer = 0;
            switch (audio)
            {
                case "explosion1":
                    audioS2.PlayOneShot(explosion1);
                    break;
                case "explosion2":
                    audioS.PlayOneShot(explosion2);
                    break;
                case "powerused":
                    audioS.PlayOneShot(powerused);
                    break;
                case "stancechange":
                    audioS.PlayOneShot(stancechange);
                    break;
                case "orbpickup":
                    audioS.PlayOneShot(orbpickup);
                    break;
                case "error":
                    audioS.PlayOneShot(error);
                    break;
                case "glass":
                    audioS.PlayOneShot(glass);
                    break;
            }
        }
    }
}
