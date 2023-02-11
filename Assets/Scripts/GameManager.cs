using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private TerrainManager[] terrainManagers;
    [SerializeField] private Island islandPrefab;

    private List<Island> islands = new List<Island>();
    [SerializeField] private Transform islandsParent;

    [SerializeField] private EnemyShip enemyShipPrefab;

    [SerializeField] private Animator bookAnimation;

    public static bool playing;

    public static int size = 12;

    [SerializeField] private Mesh[] bookDecorations;
    [SerializeField] private MeshFilter bookDecoration;

    [SerializeField] private Text scoreText;
    [SerializeField] private Text timerText;

    private int score;

    private float timerStartedAt;

    public void Start() {
        instance = this;
        Time.timeScale = 0;
    }

    public void Play() {
        UpdateIslands();
        GenerateIsland(Vector3.zero, 7, 7);
        EnemyShip.ships = 0;
    }

    public void Quit() {
        SceneManager.LoadScene(0);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7)) {
            TogglePause();
        }

        if (playing) {
            if (EnemyShip.ships < 5) {
                GenerateShip();
            }

            if (Time.timeScale < 1) {
                Time.timeScale = Mathf.Clamp01(Time.timeScale + (1-Time.timeScale)*Time.unscaledDeltaTime*8);
            }
        } else {
            if (Time.timeScale > 0) {
                Time.timeScale = Mathf.Clamp01(Time.timeScale - (Time.timeScale)*Time.unscaledDeltaTime*8);
            }
        }
    }

    private void GenerateShip() {
        Vector3 playerPos = Player.getPosition();
        playerPos = new Vector3(Mathf.Round(playerPos.x/size)*size, 1, Mathf.Round(playerPos.z/size)*size);
        Vector3 offset = (Quaternion.Euler(0,Random.Range(0f,360f),0) * Vector3.forward)*20;
        offset = new Vector3(Mathf.Round(offset.x/size)*size, 0, Mathf.Round(offset.z/size)*size);
        Instantiate(enemyShipPrefab, playerPos+offset, Quaternion.identity, islandsParent);
    }

    public void UpdateIslands() {
        Vector3[] offsets = new Vector3[] {
            new Vector3( 0,   0, size),
            new Vector3( size,0, size),
            new Vector3( size,0, 0),
            new Vector3( size,0,-size),
            new Vector3( 0,   0,-size),
            new Vector3(-size,0,-size),
            new Vector3(-size,0, 0),
            new Vector3(-size,0, size)
        };

        int s2 = size*2;

        float[] distances = new float[] {s2,s2,s2,s2,s2,s2,s2,s2};

        Vector3 playerPos = Player.getPosition();
        playerPos = new Vector3(Mathf.Round(playerPos.x/size)*size, 0, Mathf.Round(playerPos.z/size)*size);

        List<Island> remove = new List<Island>();

        foreach (Island island in islands) {
            float d = island.UpdateActive();
            if (d == -1) {
                remove.Add(island);
            }
            if (d < s2+size) {
                for (int i = 0; i < 8; i++) {
                    distances[i] = Mathf.Min(
                        Vector3.Distance(playerPos+offsets[i], island.transform.position),
                        distances[i]
                    );
                }
            }
        }

        foreach (Island removeIsland in remove) {
            islands.Remove(removeIsland);
        }

        for (int i = 0; i < 8; i++) {
            if (distances[i] >= size) {
                GenerateIsland(playerPos+offsets[i], size-Random.Range(3,6), size-Random.Range(3,6));
            }
        }
    }

    public void GenerateIsland(Vector3 position, int sizeX, int sizeY) {
        Island island = Instantiate(islandPrefab, position, Quaternion.identity, islandsParent);
        island.sizeX = sizeX;
        island.sizeY = sizeY;
        island.Generate(terrainManagers[Random.Range(0,terrainManagers.Length)]);
        island.transform.rotation = Quaternion.Euler(0,Random.Range(0,4)*90,0);
        islands.Add(island);
    }

    public void TogglePause() {
        if (!playing) {
            bookAnimation.SetTrigger("EnterWorld");
            bookDecoration.mesh = bookDecorations[0];
            playing = true;
        } else {
            bookAnimation.SetTrigger("EnterMain");
            bookDecoration.mesh = bookDecorations[1];
            playing = false;
        }
    }

    public void AddToScore() {
        score++;
        scoreText.text = score.ToString();
    }
}