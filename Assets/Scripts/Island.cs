using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island : MonoBehaviour
{
    [System.NonSerialized] public int sizeX;
    [System.NonSerialized] public int sizeY;
    [System.NonSerialized] public Node[,] nodes;

    public void Update() {

    }

    public float UpdateActive() {
        float distance = Vector3.Distance(Player.getPosition(), transform.position);
        if (gameObject.activeSelf) {
            if (distance > 20) {
                gameObject.SetActive(false);
            }
        } else {
            if (distance < 20) {
                gameObject.SetActive(true);
            }
            if (distance > 40) {
                Destroy(gameObject);
                return -1;
            }
        }
        return distance;
    }

    public void Generate(TerrainManager terrainManager) {
        nodes = new Node[sizeX,sizeY];
        for (int x = 0; x<sizeX; x++) {
            for (int y = 0; y<sizeY; y++) {
                //0 >, 1 v, 2 <, 3 ^
                nodes[x,y] = new Node(new bool[]{x==sizeX-1, y==0, x==0, y==sizeY-1});
            }
        }

        for (int x = 0; x<sizeX; x++) {
            for (int y = 0; y<sizeY; y++) {
                int direction = Random.Range(0,4);
                direction = mirrorDirection(direction, x, y);
                Connect(x,y,direction);
            }
        }

        FloodConnect((sizeX-1)/2, (sizeY-1)/2);

        ConnectRemaining();

        RemoveSmallLoops();

        AddBays();

        //string s = "";
        for (int x = 0; x<sizeX; x++) {
            //s+="\n";
            for (int y = 0; y<sizeY; y++) {
                terrainManager.Generate(nodes[x,y], transform, new Vector3(x-(sizeX-1)/2,0,y-(sizeY-1)/2));
                //s+=nodes[y,(sizeX-1)-x].ToString();
            }
        }
        //print(s);
        
        terrainManager.GenerateTreasure(transform, new Vector3(0,0,0));

        terrainManager.GenerateEnemies(transform, new Vector3(0,1,0), this);
    }

    public void FloodConnect(int x, int y) {
        nodes[x,y].isPartOfMain = true;
        bool deadEnd = true;
        for (int d = 0; d < 4; d++) {
            if (!nodes[x,y].HasConnection(d)) continue;
            Vector2Int offset = getDirectionVector(d);
            Node node = nodes[x+offset.x, y+offset.y];
            if (!node.isPartOfMain) {
                node.isPartOfMain = true;
                deadEnd = false;
                FloodConnect(x+offset.x, y+offset.y);
            }
        }

        if (deadEnd) {
            for (int d = 0; d < 4; d++) {
                int randomDirection = (d+(13*x)+(7*y))%4;
                randomDirection = mirrorDirection(randomDirection, x, y);
                if (!nodes[x,y].HasConnection(randomDirection)) {
                    Vector2Int offset = getDirectionVector(randomDirection);
                    if (!nodes[x+offset.x, y+offset.y].isPartOfMain) {
                        Connect(x,y,randomDirection);
                        FloodConnect(x+offset.x, y+offset.y);
                        return;
                    }
                }
            }
        }
    }

    public void ConnectRemaining() {
        for (int x = 0; x<sizeX; x++) {
            for (int y = 0; y<sizeY; y++) {
                if (!nodes[x,y].isPartOfMain) {
                    for (int d = 0; d < 4; d++) {
                        int randomDirection = (d+(13*x)+(7*y))%4;
                        randomDirection = mirrorDirection(randomDirection, x, y);
                        if (!nodes[x,y].HasConnection(randomDirection)) {
                            Vector2Int offset = getDirectionVector(randomDirection);
                            if (nodes[x+offset.x, y+offset.y].isPartOfMain) {
                                Connect(x,y,randomDirection);
                                FloodConnect(x, y);
                            }
                        }
                    }
                }
            }
        }
    }

    public void RemoveSmallLoops() {
        for (int x = 0; x<sizeX-1; x++) {
            for (int y = 0; y<sizeY-1; y++) {    
                if (nodes[x,y].HasConnection(0) && nodes[x+1,y].HasConnection(3) && nodes[x+1,y+1].HasConnection(2) && nodes[x,y+1].HasConnection(1)) {
                    int remove = Random.Range(0,4);
                    int offsetX = remove <= 1 ? 0 : 1;
                    int offsetY = (remove == 1 || remove == 2) ? 1 : 0;
                    Disconnect(x+offsetX, y+offsetY, remove);
                }
            }
        }    
    }

    public void AddBays() {
        int leftBay = Random.Range(1,sizeY-1);
        int rightBay = Random.Range(1,sizeY-1);
        int topBay = Random.Range(1,sizeX-1);
        int bottomBay = Random.Range(1,sizeX-1);
        nodes[0,leftBay].AddBay();
        nodes[sizeX-1,rightBay].AddBay();
        nodes[bottomBay,0].AddBay();
        nodes[topBay,sizeY-1].AddBay();
    }

    public void Connect(int x, int y, int direction) {
        nodes[x,y].Connect(direction);
        Vector2Int offset = getDirectionVector(direction);
        int oppositeDirection = getOppositeDirection(direction);
        nodes[x+offset.x, y+offset.y].Connect(oppositeDirection);
    }

    public void Disconnect(int x, int y, int direction) {
        nodes[x,y].Disconnect(direction);
        Vector2Int offset = getDirectionVector(direction);
        int oppositeDirection = getOppositeDirection(direction);
        nodes[x+offset.x, y+offset.y].Disconnect(oppositeDirection);
    }    

    public static Vector2Int getDirectionVector(int direction) {
        return direction%2 == 0 ? new Vector2Int(1-direction,0) : new Vector2Int(0,direction-2);
    }

    public static int getDirectionFromVector(Vector2Int vector) {
        if (vector.x == 1) return 0;
        if (vector.y == -1) return 1;
        if (vector.x == -1) return 2;
        return 3;
    }

    public int mirrorDirection(int direction, int x, int y) {
        //0 >, 1 v, 2 <, 3 ^
        if (x == sizeX-1 && direction == 0) direction = 2;
        if (x == 0 && direction == 2) direction = 0;
        if (y == sizeY-1 && direction == 3) direction = 1;
        if (y == 0 && direction == 1) direction = 3;
        return direction;
    }

    public static int getOppositeDirection(int direction) {
        return (direction+2)%4;
    }

    public class Node {
        //0 >, 1 v, 2 <, 3 ^
        bool[] connections = new bool[4];
        public bool[] edges = new bool[4];
        public bool hasBay;

        public Node(bool[] edges) {
            this.edges = edges;
        }

        public void Connect(int direction) {
            connections[direction] = true;
        }

        public void Disconnect(int direction) {
            connections[direction] = false;
        }

        public bool HasConnection(int direction) {
            return connections[direction];
        }

        public void AddBay() {
            hasBay = true;
            for (int e = 0; e < 4; e++) {
                if (edges[e]) connections[e] = true;
            }
        }

        public bool isPartOfMain;

        public override string ToString() {
            return (" ╶╷┌╴─┐┬╵└│├┘┴┤┼"[GetConnections()]).ToString();
        }

        public int GetConnections() {
            return (connections[0]?1:0)+(connections[1]?1:0)*2+(connections[2]?1:0)*4+(connections[3]?1:0)*8;
        }
    }
}
