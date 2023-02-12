using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village : MonoBehaviour
{
    [SerializeField] private int minDamageInclusive;
    [SerializeField] private int maxDamageInclusive;

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            Island.current.collected = true;
            int heal = Random.Range(minDamageInclusive, maxDamageInclusive+1);
            Player.instance.pirateBand.AddPeople(heal);
            Destroy(gameObject);
        }
    }
}
