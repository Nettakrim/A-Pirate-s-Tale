using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBand : Group
{
    [System.NonSerialized] public Island island;

    private Vector3 movementTarget;
    private Vector3 lastMovement;
    private Vector2Int lastIntMovement;
    private Vector3 lookDirection = new Vector3(1,0,0);

    [SerializeField] private float walkSpeed;
    [SerializeField] private float rotateSpeed;

    private void Start() {
        movementTarget = transform.localPosition;
    }

    private void Update() {
        float distance = (transform.localPosition-movementTarget).magnitude;
        if ((distance < 0.1f)) {
            Vector3 islandPosition = transform.localPosition+new Vector3((island.sizeX-1)/2,0,(island.sizeY-1)/2);

            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(islandPosition.x), Mathf.RoundToInt(islandPosition.z));

            Island.Node node = island.nodes[pos.x, pos.y];

            Vector3 playerOffset = Player.getPosition() - transform.position;
            int playerDirection;

            if (Mathf.Abs(playerOffset.x) > Mathf.Abs(playerOffset.z)) {
                playerDirection = Island.getDirectionFromVector(new Vector2Int((int)Mathf.Sign(playerOffset.x),0));
            } else {
                playerDirection = Island.getDirectionFromVector(new Vector2Int(0,(int)Mathf.Sign(playerOffset.z)));
            }
            playerDirection = (playerDirection + (4-Mathf.RoundToInt(island.transform.rotation.eulerAngles.y/90)))%4;

            Vector2Int intMovement = lastIntMovement;
            int direction = Island.getDirectionFromVector(intMovement);
            bool allowFlip = !node.HasConnection(direction) || direction == Island.getOppositeDirection(playerDirection);
            if (allowFlip || direction != playerDirection || Random.value < 0.333f) {
                int a1 = (direction+1)%4;
                int a2 = (direction+3)%4;
                int adjacent = a1 == playerDirection ? a1 : (a2 == playerDirection ? a2 : (Random.Range(0,2) == 0 ? a1 : a2));
                int oppositeAdjacent = Island.getOppositeDirection(adjacent);
                int oppositeDirection = Island.getOppositeDirection(direction);
                if (node.HasConnection(adjacent)) {
                    direction = adjacent;
                } else if (node.HasConnection(oppositeAdjacent)) {
                    direction = oppositeAdjacent;
                } else if (allowFlip && node.HasConnection(oppositeDirection)){
                    direction = oppositeDirection;
                }
            }
            direction = island.mirrorDirection(direction, pos.x, pos.y);

            //bays may cause this
            if (!node.HasConnection(direction)) {
                int adjacent = (direction+(Random.Range(0,2)*2+1))%4;
                if (node.HasConnection(adjacent)) {
                    direction = adjacent;
                } else if (node.HasConnection((adjacent+2)%4)) {
                    direction = Island.getOppositeDirection(adjacent);
                }
            }

            intMovement = Island.getDirectionVector(direction);
            
            Vector3 movement = new Vector3(intMovement.x,0,intMovement.y);

            if (movement != Vector3.zero) {
                movementTarget = new Vector3(movementTarget.x+movement.x, 1, movementTarget.z+movement.z);
                lookDirection = movement;
            }
            lastMovement = movement;
            lastIntMovement = intMovement;
        }

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, movementTarget, Time.deltaTime*walkSpeed);

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookDirection, Vector3.up), rotateSpeed*Time.deltaTime);
    }
}
