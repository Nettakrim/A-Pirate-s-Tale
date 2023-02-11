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

    [System.NonSerialized] public float playerDistance;

    [System.NonSerialized] public Target targetType;

    private void Start() {
        movementTarget = transform.localPosition;
    }

    protected override void Update() {
        base.Update();
        float distance = (transform.localPosition-movementTarget).magnitude;
        if ((distance < 0.1f)) {
            Vector3 islandPosition = transform.localPosition+new Vector3((island.sizeX-1)/2,0,(island.sizeY-1)/2);

            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(islandPosition.x), Mathf.RoundToInt(islandPosition.z));

            Island.Node node = island.nodes[pos.x, pos.y];

            Vector3 targetPos;
            if (targetParents.Count != 0) {
                switch (targetType) {
                    case (Target.Player):
                        targetPos = Player.getPosition();
                        break;
                    case (Target.Treasure):
                        targetPos = transform.parent.position;
                        break;
                    case (Target.Ship):
                        targetPos = Player.instance.bay.position;
                        break;
                    default:
                        targetPos = Vector3.zero;
                        break;
                }
            } else {
                targetPos = transform.parent.position;
            }

            Vector3 targetOffset = targetPos - transform.position;
            int targetDirection;

            if (Mathf.Abs(targetOffset.x) > Mathf.Abs(targetOffset.z)) {
                targetDirection = Island.getDirectionFromVector(new Vector2Int((int)Mathf.Sign(targetOffset.x),0));
            } else {
                targetDirection = Island.getDirectionFromVector(new Vector2Int(0,(int)Mathf.Sign(targetOffset.z)));
            }
            targetDirection = (targetDirection + (4-Mathf.RoundToInt(island.transform.rotation.eulerAngles.y/90)))%4;

            Vector3 playerOffset = Player.getPosition() - transform.position;
            playerDistance = Mathf.Abs(playerOffset.x)+Mathf.Abs(playerOffset.z);

            if (island.islandDifficulty <= 1 && playerDistance <= 4.5) {
                //if another enemy is near player lay off on your target
                foreach (EnemyBand enemyBand in island.enemies) {
                    if (enemyBand != this && enemyBand.playerDistance <= 2.5) {
                        targetDirection = Island.getOppositeDirection(targetDirection);
                        break;
                    }
                }
            }

            Vector2Int intMovement = lastIntMovement;

            int direction = Island.getDirectionFromVector(intMovement);
            bool allowFlip = !node.HasConnection(direction) || direction == Island.getOppositeDirection(targetDirection);
            if (allowFlip || direction != targetDirection || Random.value < 0.333f) {
                int a1 = (direction+1)%4;
                int a2 = (direction+3)%4;
                int adjacent = a1 == targetDirection ? a1 : (a2 == targetDirection ? a2 : (Random.Range(0,2) == 0 ? a1 : a2));
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

            //try to avoid chasing the player into a dead end
            if (playerDistance <= 1.5 && island.islandDifficulty <= 2) {
                Vector2Int offset = new Vector2Int(Mathf.RoundToInt(playerOffset.x), Mathf.RoundToInt(playerOffset.z));
                int d = Island.getDirectionFromVector(offset);
                if (node.HasConnection(d) && island.nodes[pos.x + offset.x, pos.y + offset.y].GetNumberOfExits() == 1) {
                    direction = Island.getOppositeDirection(d);
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

    protected override void DistanceBehaviour(float distance, Transform child, Transform target) {
        if (distance < 0.1) {
            Player.instance.pirateBand.ScheduleLayoutUpdate(1);
            ScheduleLayoutUpdate(1);
            Destroy(child.gameObject);
            Destroy(target.gameObject);
        }
    }

    public enum Target {
        Player,
        Treasure,
        Ship
    }
}
