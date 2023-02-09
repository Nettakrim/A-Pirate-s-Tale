using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField] private Player player;

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Bay") {
            player.bay = other.transform.parent;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.transform.parent == player.bay) player.bay = null;        
    }
}
