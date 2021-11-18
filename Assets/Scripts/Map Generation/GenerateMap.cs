using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class GenerateMap : MonoBehaviour {
    Tilemap tilemap;

    [Header("Base Sprite")]
    public Sprite baseSprite;
    public Sprite testSprite;
    public Sprite testDoorSprite;

    [Header("Map attributes")]
    public int sizeX;
    public int sizeY;
    public int numRooms = 2;
    public int minRoomDimension;
    public int maxRoomDimension;
    private Room[] rooms;

    [Header("Tile Sprites")]
    public Sprite[] floorSprites;
    private Tile[] floorTiles;

    private void Awake() {
        tilemap = GetComponent<Tilemap>();
        rooms = new Room[numRooms];

        ApplyBase();

        floorTiles = InstantiateTiles(floorSprites);

        GenerateRooms();
        //GenerateCorridors();
    }

    private void GenerateRooms() {
        //generate room and test to make sure it does not collide with others, if it does generate new until max is rteached
        int index = 0;

        while (index < numRooms) {
            Vector2Int randPos = new Vector2Int(Random.Range(0, sizeX), Random.Range(0, sizeY));
            int randWidth = Random.Range(minRoomDimension, maxRoomDimension);
            int randHeight = Random.Range(minRoomDimension, maxRoomDimension);
            Room.RoomType randType;

            // must have entrance and exit room
            if (index == 0) {
                randType = Room.RoomType.entrance;
            } else if (index == 1) {
                randType = Room.RoomType.exit;
            } else {
                randType = (Room.RoomType)Random.Range(0, (int)Room.RoomType.COUNT - 2);
            }

            Room newRoom = new Room(randPos, randWidth, randHeight, randType, 2);

            bool safeToUse = true;

            if (newRoom.pos.x + newRoom.width <= sizeX - 15 && newRoom.pos.y + newRoom.height <= sizeY - 15 && newRoom.pos.x >= 15 && newRoom.pos.y >= 15) {
                for (int i = 0; i < index; i++) {
                    if (newRoom.CollidesWith(rooms[i])) {
                        safeToUse = false;
                        break;
                    }
                }
            } else {
                safeToUse = false;
            }

            if (safeToUse) {
                rooms[index] = newRoom;
                index++;
            }
        }

        //draw tiles for each room on tilemap (and doors)
        Tile testTile = ScriptableObject.CreateInstance<Tile>();
        testTile.sprite = testSprite;

        Tile testDoorTile = ScriptableObject.CreateInstance<Tile>();
        testDoorTile.sprite = testDoorSprite;

        for (int i = 0; i < index; i++) {
            for (int x = rooms[i].pos.x; x < rooms[i].pos.x + rooms[i].width; x++) {
                for (int y = rooms[i].pos.y; y < rooms[i].pos.y + rooms[i].height; y++) {
                    tilemap.SetTile(new Vector3Int(x, y, 0), testTile);
                }
            }

            for (int j = 0; j < rooms[i].numDoors; j++) {
                tilemap.SetTile(new Vector3Int(rooms[i].doors[j].pos.x, rooms[i].doors[j].pos.y, 0), testDoorTile);
            }
        }
    }

    private void ApplyBase() {
        Tile baseTile = ScriptableObject.CreateInstance<Tile>();
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
            tileArray[i] = ScriptableObject.CreateInstance<Tile>();
            tileArray[i].sprite = sprites[i];
        }

        return tileArray;
    }

    //loop through each pixel per tile and create new texture2D to return and apply to a tile for procedural generation!!!!
    //https://docs.unity3d.com/ScriptReference/Tilemaps.TileBase.html
    //https://docs.unity3d.com/ScriptReference/Tilemaps.Tilemap.html
}
