using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBand : Group
{
    [System.NonSerialized] public Island island;

    private Vector3 movementTarget = new Vector3(0,1,0);
    private Vector3 lastMovement;
    private Vector2Int lastIntMovement;
    private Vector3 lookDirection = new Vector3(1,0,0);

    [SerializeField] private float walkSpeed;
    [SerializeField] private float rotateSpeed;

    private void Update() {
        float distance = (transform.localPosition-movementTarget).magnitude;
        if ((distance < 0.1f)) {
            Vector3 islandPosition = transform.localPosition+new Vector3(island.sizeX/2,0,island.sizeY/2);

            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(islandPosition.x), Mathf.RoundToInt(islandPosition.z));

            Island.Node node = island.nodes[pos.x, pos.y];

            Vector2Int intMovement = lastIntMovement;

            Vector3 playerOffset = Player.instance.transform.position - transform.position;
            int playerDirection;
            if (Mathf.Abs(playerOffset.x) > Mathf.Abs(playerOffset.y)) {
                playerDirection = Island.getDirectionFromVector(new Vector2Int((int)Mathf.Sign(playerOffset.x),0));
            } else {
                playerDirection = Island.getDirectionFromVector(new Vector2Int(0,(int)Mathf.Sign(playerOffset.x)));
            }

            int direction = Island.getDirectionFromVector(intMovement);
            if (!node.HasConnection(direction) || direction != playerDirection || Random.value < 0.333f) {
                int adjacent = (direction+(Random.Range(0,2)*2+1))%4;
                if (node.HasConnection(adjacent)) {
                    direction = adjacent;
                } else if (node.HasConnection((adjacent+2)%4)) {
                    direction = Island.getOppositeDirection(adjacent);
                } else {
                    direction = Island.getOppositeDirection(direction);
                }
            } else if (Random.value < 0.333f || direction != playerDirection) {
                int adjacent = (direction+(Random.Range(0,2)*2+1))%4;
                if (node.HasConnection(adjacent)) {
                    intMovement = Island.getDirectionVector(adjacent);
                } else if (node.HasConnection((adjacent+2)%4)) {
                    direction = Island.getOppositeDirection(adjacent);
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
            

            //Vector3 offset = new Vector3(0,0.125f,0);
            //if (Physics.Raycast(movementTarget + offset, new Vector3(x,0,0), 1)) {
            //    x = 0; 
            //} 
            //if (Physics.Raycast(movementTarget + offset, new Vector3(0,0,z), 1)) {
            //    z = 0;
            //}
            //if (x != 0 && z != 0) {
            //    if (lastMovement.x == 0) z = 0;
            //    else x = 0;
            //}
            Vector3 movement = new Vector3(intMovement.x,0,intMovement.y);
            
            //if (!Physics.Raycast(movementTarget+offset+movement, Vector3.down, 0.15f)) {
            //    movement = Vector3.zero;
            //}
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
