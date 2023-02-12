using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [SerializeField] private float rotationRange;

    private void Start() {
        transform.rotation *= Quaternion.Euler(0,Random.Range(-rotationRange,rotationRange),0);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            Island.current.collected = true;
            GameManager.instance.AddToScore();
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<Animation>().Play();
        }
    }
}
