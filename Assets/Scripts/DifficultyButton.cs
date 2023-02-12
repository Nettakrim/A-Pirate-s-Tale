using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DifficultyButton : Button
{
    [SerializeField] private int difficulty;
    [SerializeField] private string message;
    [SerializeField] private Text noteText;

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        noteText.text = "In which a band of "+Player.instance.startingPirates.Get(difficulty)+" pirates set out on a quest for treasure!\n"+message+"\nHighscore: "+GameManager.GetHighscore(difficulty)+" Treasure";
    }
}
