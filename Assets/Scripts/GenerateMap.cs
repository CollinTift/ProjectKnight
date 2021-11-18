using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateMap : MonoBehaviour {
    Tilemap tilemap;

    [Header("Base Sprite")]
    public Sprite baseSprite;

    [Header("Map attributes")]
    public int sizeX;
    public int sizeY;

    [Header("Tile Sprites")]
    public Sprite[] floorSprites;
    private Tile[] floorTiles;

    private void Awake() {
        tilemap = GetComponent<Tilemap>();

        ApplyBase();

        floorTiles = InstantiateTiles(floorSprites);
    }

    private void ApplyBase() {
        Tile baseTile = new Tile();
        baseTile.sprite = baseSprite;
        baseTile.color = Color.black;

        for (int x = 0; x < sizeX; x++) {
            for (int y = 0; y < sizeY; y++) {
                tilemap.SetTile(new Vector3Int(x, y, 0), baseTile);
            }
        }
    }

    private Tile[] InstantiateTiles(Sprite[] sprites) { 
        Tile[] tileArray = new Tile[sprites.Length];

        for (int i = 0; i < sprites.Length; i++) {
            tileArray[i] = new Tile();
            tileArray[i].sprite = sprites[i];
        }

        return tileArray;
    }

    //loop through each pixel per tile and create new texture2D to return and apply to a tile for procedural generation!!!!
    //https://docs.unity3d.com/ScriptReference/Tilemaps.TileBase.html
    //https://docs.unity3d.com/ScriptReference/Tilemaps.Tilemap.html

    private class Room {

    }
}
