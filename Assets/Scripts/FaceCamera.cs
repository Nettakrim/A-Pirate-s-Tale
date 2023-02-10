using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] private bool update;

    private static Quaternion rotation = Quaternion.Euler(0,-90,0);

    void Start() {
        UpdateFaceCamera();
        if (!update) Destroy(this);
    }

    void Update() {
        UpdateFaceCamera();
    }

    void UpdateFaceCamera() {
        transform.rotation = rotation;
    }
}
