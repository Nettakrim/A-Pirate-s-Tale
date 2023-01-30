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

    public void Start() {
        instance = this;
        GenerateIsland(Vector3.zero, 7, 7);
    }

    public void UpdateIslands() {
        int s = 12;

        Vector3[] offsets = new Vector3[] {
            new Vector3( 0,0, s),
            new Vector3( s,0, s),
            new Vector3( s,0, 0),
            new Vector3( s,0,-s),
            new Vector3( 0,0,-s),
            new Vector3(-s,0,-s),
            new Vector3(-s,0, 0),
            new Vector3(-s,0, s)
        };

        int s2 = s*2;

        float[] distances = new float[] {s2,s2,s2,s2,s2,s2,s2,s2};

        Vector3 playerPos = Player.getPosition();
        playerPos = new Vector3(Mathf.Round(playerPos.x/s)*s, 0, Mathf.Round(playerPos.z/s)*s);

        List<Island> remove = new List<Island>();

        foreach (Island island in islands) {
            float d = island.UpdateActive();
            if (d == -1) {
                remove.Add(island);
            }
            if (d < s2+s) {
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
            if (distances[i] >= s) {
                GenerateIsland(playerPos+offsets[i], s-Random.Range(3,6), s-Random.Range(3,6));
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