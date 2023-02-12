using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village : MonoBehaviour
{
    [SerializeField] private int minDamageInclusive;
    [SerializeField] private int maxDamageInclusive;

    private bool collected;

    private void Update() {
        if (collected) {
            float y = transform.GetChild(0).localScale.y;
            if (y > 0.1) {
                y-=Time.deltaTime*4*((y-0.1f)*1.111f);
            }
            transform.GetChild(0).localScale = new Vector3(1,y,1);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            Island.current.collected = true;
            int heal = Random.Range(minDamageInclusive, maxDamageInclusive+1);
            Player.instance.pirateBand.AddPeople(heal);
            collected = true;
        }
    }
}
