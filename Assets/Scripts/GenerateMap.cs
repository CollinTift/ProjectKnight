using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateMap : MonoBehaviour {
    Tilemap tilemap;

    private void Awake() {
        tilemap = GetComponent<Tilemap>();
    }
}
