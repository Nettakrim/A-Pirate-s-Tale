using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateBand : Group
{
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Treasure Chest") {

        }

        if (other.tag == "Enemy Band") {
            targetParent = other.transform;
            other.GetComponent<EnemyBand>().targetParent = transform;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Enemy Band") {
            other.GetComponent<EnemyBand>().targetParent = null;
            targetParent = null;
        }        
    }
}
