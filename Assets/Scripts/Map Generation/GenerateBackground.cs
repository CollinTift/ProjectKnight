using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateBackground : MonoBehaviour {
    Tilemap tilemap;

    [Header("Background Tiles")]
    public Sprite[] bgSprites;
    private Tile[] bgTiles;

    private void Awake() {
        tilemap = gameObject.GetComponent<Tilemap>();
    }
}
