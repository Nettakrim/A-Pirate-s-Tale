using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private int minDamageInclusive;
    [SerializeField] private int maxDamageInclusive;

    void Update() {
        transform.position += transform.forward*speed*Time.deltaTime;
    }

    private void OnCollisionEnter(Collision other) {
        if (other.transform.tag == "Enemy Ship") return;
        if (other.transform.tag == "Player") {
            int damage = Random.Range(minDamageInclusive, maxDamageInclusive+1);
            Player.instance.pirateBand.KillRandom(damage);
        }
        Destroy(gameObject);
    }
}
