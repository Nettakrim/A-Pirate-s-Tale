using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainManager {
    [SerializeField] private TerrainSection emptys; //  
    [SerializeField] private TerrainSection deadEnds; // ╶
    [SerializeField] private TerrainSection bends; // ┌
    [SerializeField] private TerrainSection straights; // ─
    [SerializeField] private TerrainSection tJunctions; // ┬
    [SerializeField] private TerrainSection crossroads; // ┼

    [SerializeField] private TerrainSection cliffEdges;
    [SerializeField] private TerrainSection cliffCorners;
    [SerializeField] private TerrainSection bays;

    [SerializeField] private TerrainSection treasures;

    [SerializeField] private EnemyBand enemies;

    private static PathConnection[] connectionCombinations = new PathConnection[] {
        new PathConnection(0, new int[]{0,1,2,3}),
        new PathConnection(1, new int[]{0}),
        new PathConnection(1, new int[]{1}),
        new PathConnection(2, new int[]{0}),
        new PathConnection(1, new int[]{2}),
        new PathConnection(3, new int[]{0,2}),
        new PathConnection(2, new int[]{1}),
        new PathConnection(4, new int[]{0}),
        new PathConnection(1, new int[]{3}),
        new PathConnection(2, new int[]{3}),
        new PathConnection(3, new int[]{1,3}),
        new PathConnection(4, new int[]{3}),
        new PathConnection(2, new int[]{2}),
        new PathConnection(4, new int[]{2}),
        new PathConnection(4, new int[]{1}),
        new PathConnection(5, new int[]{0,1,2,3})
    };

    public void Generate(Island.Node node, Transform parent, Vector3 localPosition) {
        PathConnection pathConnection = connectionCombinations[node.GetConnections()];
        TerrainSection terrainSection = getTerrainSection(pathConnection.type);
        terrainSection.Generate(parent, localPosition, pathConnection.rotations);

        Vector3 otherOffset = Vector3.zero;
        for (int e = 0; e < 4; e++) {
            if(node.edges[e]) {
                Vector2 offsetTemp = Island.getDirectionVector(e);
                Vector3 offset = new Vector3(offsetTemp.x, 0, offsetTemp.y);
                TerrainSection section = node.hasBay ? bays : cliffEdges;
                section.Generate(parent, localPosition+offset, new int[]{e});
                if (otherOffset != Vector3.zero) {
                    cliffCorners.Generate(parent, localPosition+offset+otherOffset, new int[]{(e == 3 && node.edges[0]) ? 0 : e});
                }
                otherOffset = offset;
            }
        }
    }

    public void GenerateTreasure(Transform parent, Vector3 localPosition) {
        treasures.Generate(parent, localPosition, new int[]{0,1,2,3});
    }

    public void GenerateEnemies(Transform parent, Vector3 localPosition, Island island, int amount) {
        EnemyBand band = Object.Instantiate(enemies, parent);
        band.transform.localPosition = localPosition;
        band.island = island;
        band.AddPeople(amount);
        island.enemies.Add(band);
    }

    private TerrainSection getTerrainSection(int type) {
        switch (type) {
            case 0:
                return emptys;
            case 1:
                return deadEnds;
            case 2:
                return bends;
            case 3:
                return straights;
            case 4:
                return tJunctions;
            case 5:
                return crossroads;
        }
        return emptys;
    }

    private class PathConnection {
        public int type;
        public int[] rotations;

        public PathConnection(int type, int[] rotations) {
            this.type = type;
            this.rotations = rotations;
        }
    }
}

[System.Serializable]
public class TerrainSection {
    [SerializeField] private GameObject[] variations;

    public void Generate(Transform parent, Vector3 localPosition, int[] rotations) {
        int index = Random.Range(0, variations.Length);
        int rotationIndex = Random.Range(0, rotations.Length);
        Transform obj = Object.Instantiate(variations[index], Vector3.zero, Quaternion.Euler(0,rotations[rotationIndex]*90,0), parent).transform;
        obj.localPosition = localPosition;
    }
}