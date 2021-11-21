using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateBackground : MonoBehaviour {
    Tilemap tilemap;

    [Header("Parallax")]
    private Vector2 startPos;
    public GameObject cam;
    public float parallaxEffect;

    [Header("Background Tiles")]
    public int sizeX;
    public int sizeY;
    public Sprite[] bgSprites;
    private Tile[] bgTiles;

    private void Awake() {
        tilemap = gameObject.GetComponent<Tilemap>();

        startPos = new Vector3(-50, -50, 1);

        bgTiles = new Tile[bgSprites.Length];

        for (int i = 0; i < bgSprites.Length; i++) {
            bgTiles[i] = ScriptableObject.CreateInstance<Tile>();
            bgTiles[i].sprite = bgSprites[i];
        }

        for (int x = 0; x < sizeX; x++) {
            for (int y = 0; y < sizeY; y++) {
                tilemap.SetTile(new Vector3Int(x, y, 1), bgTiles[Random.Range(0, bgTiles.Length)]);
            }
        }
    }

    private void FixedUpdate() {
        float distX = (cam.transform.position.x * parallaxEffect);
        float distY = (cam.transform.position.y * parallaxEffect);

        transform.position = new Vector3(startPos.x + distX, startPos.y + distY, transform.position.z);
    }
}
