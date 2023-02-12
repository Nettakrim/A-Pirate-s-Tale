using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

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

    public static int size;
    [SerializeField] private DifficultyOptions<int> sizes;

    [SerializeField] private Mesh[] bookDecorations;
    [SerializeField] private MeshFilter bookDecoration;

    [SerializeField] private Text scoreText;
    [SerializeField] private Text timerText;
    [SerializeField] private Transform canvas;

    [SerializeField] private int pausedSamples;

    private int score;

    private float endAtTime;

    public static bool hasStarted;

    public static int difficulty = 2;

    [SerializeField] private float gameLength;

    [SerializeField] private Animator postProcessAnimation;

    [SerializeField] private Transform cuteDescriptionParent;

    [SerializeField] private Settings settings;

    [SerializeField] private AudioSource[] music;
    private int musicFade;
    private float startingVolume;

    public void Start() {
        instance = this;
        Time.timeScale = 0;
        musicFade = 1;
        hasStarted = false;
        SetTimer(Mathf.CeilToInt(gameLength));
        startingVolume = music[1].volume;
    }

    public void Play(int gameDifficulty) {
        if (playing) return;
        difficulty = gameDifficulty;
        size = sizes.Get();
        UpdateIslands();
        GenerateIsland(Vector3.zero, size-Random.Range(3,6), size-Random.Range(3,6));
        EnemyShip.ships = 0;
        bookDecoration.mesh = bookDecorations[1];
        playing = true;
        bookAnimation.SetTrigger("EnterWorld");
        endAtTime = Time.time + gameLength;
        postProcessAnimation.SetTrigger("Enter"+gameDifficulty);
        settings.StopRebind();
        musicFade = 0;
        music[1].Play();

        Player.instance.Play(gameDifficulty);

        hasStarted = true;
    }

    public void MainMenu() {
        SceneManager.LoadScene(0);
    }

    public void Quit() {
        Application.Quit();
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
                if (Time.timeScale > 0.999f) Time.timeScale = 1;
                if (Time.timeScale == 1) {
                    canvas.GetChild(2).gameObject.SetActive(false);
                }
                music[0].volume = 1-Time.timeScale;
            }

            if (!Player.instance.hasMoved) endAtTime = Time.time + gameLength;
            if (SetTimer(Mathf.CeilToInt(endAtTime - Time.time)) <= 0) {
                GameOver(false);
            }
        } else {
            if (Time.timeScale > 0) {
                Time.timeScale = Mathf.Clamp01(Time.timeScale - (Time.timeScale)*Time.unscaledDeltaTime*8);
                if (Time.timeScale < 0.0001f) Time.timeScale = 0;
            }
        }

        if (musicFade == 1) music[0].volume += Time.unscaledDeltaTime;
        if (musicFade == -1) music[1].volume -= Time.unscaledDeltaTime;
        if (musicFade == 2) {
            music[1].volume += Time.unscaledDeltaTime;
            if (music[1].volume >= startingVolume) {
                music[1].volume = startingVolume;
                musicFade = 0;
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
        if (!hasStarted) return;

        canvas.GetChild(1).gameObject.SetActive(false);
        canvas.GetChild(2).gameObject.SetActive(true);
        if (!playing) {
            bookAnimation.SetTrigger("EnterWorld");
            bookDecoration.mesh = bookDecorations[1];
            postProcessAnimation.SetTrigger("Enter"+difficulty);
            music[1].timeSamples = pausedSamples;
            music[1].Play();
            musicFade = 2;
            playing = true;
        } else {
            bookAnimation.SetTrigger("EnterMain");
            bookDecoration.mesh = bookDecorations[2];
            postProcessAnimation.SetTrigger("Enter2");
            pausedSamples = music[1].timeSamples;
            musicFade = -1;
            playing = false;
            updateCuteProceduralDescription(false);
        }
        Player.instance.canMouseMove = false;
    }

    public void GameOver(bool gotKilled) {
        if (!playing) return;

        TogglePause();

        canvas.GetChild(2).GetChild(1).gameObject.SetActive(false);
        canvas.GetChild(2).GetChild(2).gameObject.SetActive(true);

        hasStarted = false;

        if (score > GetHighscore(difficulty)) {
            SetHighscore(score);
        }
    }

    public void AddToScore() {
        score++;
        scoreText.text = score.ToString();
    }

    private int SetTimer(int secondsLeft) {
        timerText.text = (secondsLeft/60)+":"+(secondsLeft%60).ToString().PadLeft(2,'0');
        return secondsLeft;
    }

    public static int GetHighscore(int difficulty) {
        return PlayerPrefs.GetInt("Highscore"+difficulty, 0);
    }

    public static void SetHighscore(int score) {
        PlayerPrefs.SetInt("Highscore"+difficulty, score);
    }

    public void updateCuteProceduralDescription(bool finished) {
        int startingPirates = Player.instance.startingPirates.Get();
        int pirates = Player.instance.pirateBand.getSize(false);
        bool isHighscore = score > GetHighscore(difficulty);

        string desc = "In which a band of "+startingPirates+" pirates set out on a journey ";
        switch (difficulty) {
            case 1:
                desc += "early in the morning!";
                break;
            case 2:
                desc += "when the sun was high overhead!";
                break;
            case 3:
                desc += "in the dead of night...";
                break;
        }

        string highscoreString = isHighscore ? ", more than any pirate band before them!" : "!";

        int deltaPirates = pirates - startingPirates;
        if (deltaPirates < 0 && pirates != 0) {
            desc += "\nHowever, after a series of fights while looking for treasure, only ";
            if (pirates == 1) {
                desc += "The Captain remained";
            } else {
                desc += pirates+" remained";
            }
            if (score > 0) {
                desc += "...\nLuckily, despite the odds they managed to uncover treasure ";
                if (score == 1) {
                    desc += "hidden away on an island!";
                } else {
                    desc += "on "+score+" different islands"+highscoreString;
                }
                if (finished) {
                    desc+="\nAnd the winds did blow in their favour - they made it out alive and with their treasure!";
                } else {
                    desc+="\nOne can only hope they make it out alive, with or without their treasure...";
                }
            } else {
                if (finished) {
                    desc+=".\nUnfortunately, after much searching they couldn't find any, atleast they made it out alive...";
                } else {
                    desc+=" and they had yet to find any treasure.\nOne can only hope they make it out alive, with or without their treasure...";
                }
            }
        } else if (deltaPirates > 0) {
            desc += "\nAfter pillaging a series of villages, they managed to recruit even more for their cause, so a crew of "+pirates+" pirates did search the seas for treasure...";
            if (score > 1) {
                desc += "\nAnd treasure they did find! after avoiding other marauding bands they found treasure on "+score+" different islands"+highscoreString;
            } else if (score == 1) {
                desc += "\nAnd what luck! After avoiding other marauding bands they found treasure hidden away on an island!";
            } else {
                if (finished) {
                    desc += "\nHowever, they could not find any treasure, despite their valiant efforts...";
                } else {
                    desc += "\nHowever, despite their numbers, they were yet to find any treasure...";
                }
            }
        } else if (pirates == 0) {
            if (Player.instance.bay != null) {
                desc += "\nHowever, after a series of fights while looking for treasure, every last pirate was downed";
                if (score == 0) {
                    desc += ", and they didn't even manage to find any treasure...";
                }
            } else {
                desc += "\nHowever, while looking for treasure, a cannonball breached the ship's hull and every last pirate perished";
                if (score == 0) {
                    desc += " - they didn't even manage to find any treasure...";
                }
            }
            if (score != 0) {
                desc += ".\nAtleast one may find solace in the fact they found ";
                if (score == 1) {
                    desc += "some treasure hidden away on an island...";
                } else {
                    desc += "treasure on "+score+" different islands";
                    if (isHighscore) {
                        desc+= ", more than any pirate band before them...";
                    } else {
                        desc+=" before succumbing to their fate...";
                    }
                }
            }
        } else {
            if (score == 0) {
                if (finished) {
                    desc += "\nHowever, despite searching far and wide, they could not find any treasure...";
                } else {
                    desc += "\nThe search for tresure begins";
                    if (difficulty == 3) desc += "!";
                    else desc += "...";
                }
            } else if (score == 1) {
                desc += "\nAnd what luck! After avoiding other marauding bands they found treasure hidden away on an island!";
            } else {
                desc += "\nAnd their search for treasure was bountiful, finding treasure on "+score+" different islands"+highscoreString;
            }
        }

        cuteDescriptionParent.GetChild(0).GetComponent<Text>().text = "Chapter "+difficulty;
        cuteDescriptionParent.GetChild(1).GetChild(0).GetComponent<Text>().text = desc;
    }
}