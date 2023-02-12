using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShip : MonoBehaviour
{
    private Vector3 movementTarget;
    private Vector3 lastMovement;

    [SerializeField] private DifficultyOptions<float> sailSpeed;
    [SerializeField] private float rotateSpeed;

    public static int ships;

    private int aimingStage;
    private float lastStageAt;
    [SerializeField] private float aimDuration;
    [SerializeField] private float aimDistance;
    [SerializeField] private float shootCoodown;

    [SerializeField] private GameObject cannonBallPrefab;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        Vector3 playerOffset = Player.getPosition() - transform.position;
        if (Mathf.Abs(playerOffset.x) > Mathf.Abs(playerOffset.z)) {
            lastMovement = new Vector3((int)Mathf.Sign(playerOffset.x)*GameManager.size,0,0);
        } else {
            lastMovement = new Vector3(0,0,(int)Mathf.Sign(playerOffset.z)*GameManager.size);
        }
        Vector3 offset = new Vector3(GameManager.size/2, 0, GameManager.size/2);
        transform.position -= offset;
        movementTarget = transform.position;

        //push toward player
        int axis = Random.Range(0,2);
        int distance = Random.Range(0,GameManager.size);
        if (axis == 0) {
            transform.position -= new Vector3(distance*Mathf.Sign(playerOffset.x),0,0);
        } else {
            transform.position -= new Vector3(0,0,distance*Mathf.Sign(playerOffset.z));
        }
        ships++;
    }

    void Update()
    {
        if (!Player.instance.hasMoved) return;

        float distance = (transform.position-movementTarget).magnitude;
        Vector3 playerOffset = Player.getPosition() - (transform.position + transform.forward/2);
        float playerDistance = playerOffset.magnitude;
        
        if ((distance < 0.1f)) {
            Vector3 movement;
            if (playerDistance >= GameManager.size*2 || Random.Range(0f,1f) > 0.25f) {
                if (Mathf.Abs(playerOffset.x) > Mathf.Abs(playerOffset.z)) {
                    movement = new Vector3((int)Mathf.Sign(playerOffset.x),0,0);
                } else {
                    movement = new Vector3(0,0,(int)Mathf.Sign(playerOffset.z));
                }
                movement = movement.normalized * GameManager.size;
                if (movement == -lastMovement) {
                    movement = Quaternion.Euler(0,Random.Range(-1,1)*180+90,0)*movement;
                }
            } else {
                movement = (Quaternion.Euler(0,Random.Range(1,4)*90f,0) * lastMovement).normalized * GameManager.size;
            }
            movementTarget += movement;
            lastMovement = movement;
            if (playerDistance > 35) {
                ships--;
                Destroy(gameObject);
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, movementTarget, Time.deltaTime*sailSpeed.Get());
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lastMovement, Vector3.up), rotateSpeed*Time.deltaTime);
    
        if (aimingStage == 1) {
            if (Time.time - lastStageAt > aimDuration) {
                //shoot
                Instantiate(cannonBallPrefab, transform.GetChild(2).GetChild(1).position, Quaternion.LookRotation(Player.getPosition()-transform.position, Vector3.up));
                aimingStage = 2;
                lastStageAt = Time.time;
                transform.GetChild(1).gameObject.SetActive(false);
            }

            if (playerDistance > aimDistance*1.25f || Player.instance.bay != null) {
                aimingStage = 0;
                audioSource.Stop();
                lastStageAt = (Time.time-shootCoodown)+0.5f;
                transform.GetChild(1).gameObject.SetActive(false);
            }

            transform.GetChild(2).rotation = Quaternion.Lerp(transform.GetChild(2).rotation, Quaternion.LookRotation(Player.getPosition() - transform.position, Vector3.up), rotateSpeed*Time.deltaTime*0.5f);
        } else if (aimingStage == 2) {
            if (Time.time - lastStageAt > shootCoodown) {
                aimingStage = 0;
            }
        } else {
            if (playerDistance < aimDistance && Player.instance.bay == null) {
                aimingStage = 1;
                audioSource.Play();
                transform.GetChild(1).gameObject.SetActive(true);
                lastStageAt = Time.time;
            }
            transform.GetChild(2).rotation = Quaternion.Lerp(transform.GetChild(2).rotation, Quaternion.LookRotation(Player.getPosition() - transform.position, Vector3.up), rotateSpeed*Time.deltaTime*0.1f);
        }

        if (!GameManager.playing) {
            transform.GetChild(1).gameObject.SetActive(false);
        }    
    }

    private void OnCollisionEnter(Collision other) {
        if (other.transform.tag == transform.tag) {
            movementTarget-=lastMovement;
            lastMovement = -lastMovement;
        } else if (other.transform.tag == "Player") {
            Player.instance.pirateBand.KillRandom(3);
        }
    }
}
