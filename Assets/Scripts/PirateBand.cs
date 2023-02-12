using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateBand : Group
{
    protected override void onDefeat() {
        GameManager.instance.GameOver(true);
    }
}
