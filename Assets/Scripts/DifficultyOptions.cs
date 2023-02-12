using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DifficultyOptions<T>
{
    [SerializeField] private T[] options;

    public T Get() {
        return Get(GameManager.difficulty);
    }

    public T Get(int difficulty) {
        return options[difficulty-1];
    }
}
