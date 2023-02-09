using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private Island islandPrefab;

    private List<Island> islands = new List<Island>();
    [SerializeField] private Transform islandsParent;

    [SerializeField] private EnemyShip enemyShipPrefab;

    public static int size = 12;

    public void Start() {
        instance = this;
        UpdateIslands();
        GenerateIsland(Vector3.zero, 7, 7);
        EnemyShip.ships = 0;
    }

    private void Update() {
        if (EnemyShip.ships < 5) {
            GenerateShip();
        }
    }

    private void GenerateShip() {
        Vector3 playerPos = Player.getPosition();
        playerPos = new Vector3(Mathf.Round(playerPos.x/size)*size, 1, Mathf.Round(playerPos.z/size)*size);
        Vector3 offset = (Quaternion.Euler(0,Random.Range(0f,360f),0) * Vector3.forward)*20;
        offset = new Vector3(Mathf.Round(offset.x/size)*size, 0, Mathf.Round(offset.z/size)*size);
        Instantiate(enemyShipPrefab, playerPos+offset, Quaternion.identity);
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
        island.Generate(terrainManager);
        island.transform.rotation = Quaternion.Euler(0,Random.Range(0,4)*90,0);
        islands.Add(island);
    }
}