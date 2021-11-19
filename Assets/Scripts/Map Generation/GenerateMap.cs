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

    [Header("Map attributes")]
    public int sizeX;
    public int sizeY;
    public int numRooms = 2;
    public int minRoomDimension;
    public int maxRoomDimension;
    private Room[] rooms;
    private Corridor[] corridors;

    [Header("Tile Sprites")]
    public Sprite[] floorSprites;
    private Tile[] floorTiles;

    private void Awake() {
        tilemap = GetComponent<Tilemap>();
        rooms = new Room[numRooms];
        corridors = new Corridor[numRooms - 1];

        ApplyBase();

        floorTiles = InstantiateTiles(floorSprites);

        GenerateRooms();
    }

    private void GenerateRooms() {
        //-----------------OLD ROOM GEN---------------
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
                randType = (Room.RoomType)Random.Range(0, (int)Room.RoomType.COUNT - 2); //-2 because of entrance and exit
            }

            Room newRoom = new Room(randPos, randWidth, randHeight, randType);

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

        ////---------------NEW SEXY SQUEAKY CLEAN ROOM GEN-------------
        //rooms[0] = new Room(
        //            new Vector2Int(Mathf.FloorToInt(sizeX / 2), Mathf.FloorToInt(sizeY / 2)),
        //            Random.Range(minRoomDimension, maxRoomDimension),
        //            Random.Range(minRoomDimension, maxRoomDimension),
        //            Room.RoomType.entrance);

        //corridors[0] = new Corridor(
        //            rooms[0],
        //            corridorLengthMax,
        //            true);

        //for (int i = 1; i < rooms.Length; i++) {
        //    rooms[i] = new Room(corridors[i - 1].endPos,
        //            Random.Range(minRoomDimension, maxRoomDimension),
        //            Random.Range(minRoomDimension, maxRoomDimension),
        //            (Room.RoomType)Random.Range(0, (int)Room.RoomType.COUNT));

        //    if (i < corridors.Length) {
        //        corridors[i] = new Corridor(rooms[i], corridorLengthMax, false);
        //    }
        //}

        //draw tiles for each room on tilemap
        Tile testTile = ScriptableObject.CreateInstance<Tile>();
        testTile.sprite = testSprite;

        for (int i = 0; i < numRooms; i++) {
            //rooms
            for (int x = rooms[i].pos.x; x < rooms[i].pos.x + rooms[i].width; x++) {
                for (int y = rooms[i].pos.y; y < rooms[i].pos.y + rooms[i].height; y++) {
                    tilemap.SetTile(new Vector3Int(x, y, 0), testTile);
                }
            }

            //add 1 corridor between each room
            if (i < corridors.Length) {
                //STORE RANDOM TILE ON A SIDE OF ROOM (determined by where startRoom is in relation to endRoom) TO GEN CORRIDORS ON AND TO
                Vector2Int startPos;
                Vector2Int endPos;

                if (rooms[i].pos.x < rooms[i + 1].pos.x) {
                    //left
                    if (rooms[i].pos.y < rooms[i + 1].pos.y) {
                        //below
                        startPos = new Vector2Int(rooms[i].pos.x + rooms[i].width - 1, Random.Range(rooms[i].pos.y, rooms[i].pos.y + rooms[i].height));
                        endPos = new Vector2Int(Random.Range(rooms[i + 1].pos.x, rooms[i + 1].pos.x + rooms[i + 1].width), rooms[i + 1].pos.y);
                    } else if (rooms[i].pos.y > rooms[i + 1].pos.y) {
                        //above
                        startPos = new Vector2Int(rooms[i].pos.x + rooms[i].width - 1, Random.Range(rooms[i].pos.y, rooms[i].pos.y + rooms[i].height));
                        endPos = new Vector2Int(Random.Range(rooms[i + 1].pos.x, rooms[i + 1].pos.x + rooms[i + 1].width), rooms[i + 1].pos.y + rooms[i + 1].height - 1);
                    } else {
                        //same height
                        startPos = new Vector2Int(rooms[i].pos.x + rooms[i].width - 1, Random.Range(rooms[i].pos.y, rooms[i].pos.y + rooms[i].height));
                        endPos = new Vector2Int(rooms[i + 1].pos.x, Random.Range(rooms[i + 1].pos.y, rooms[i + 1].pos.y + rooms[i + 1].height));
                    }
                } else if (rooms[i].pos.x > rooms[i + 1].pos.x) {
                    //right
                    if (rooms[i].pos.y < rooms[i + 1].pos.y) {
                        //below
                        startPos = new Vector2Int(rooms[i].pos.x, Random.Range(rooms[i].pos.y, rooms[i].pos.y + rooms[i].height));
                        endPos = new Vector2Int(Random.Range(rooms[i + 1].pos.x, rooms[i + 1].pos.x + rooms[i + 1].width), rooms[i + 1].pos.y);
                    } else if (rooms[i].pos.y > rooms[i + 1].pos.y) {
                        //above
                        startPos = new Vector2Int(rooms[i].pos.x, Random.Range(rooms[i].pos.y, rooms[i].pos.y + rooms[i].height));
                        endPos = new Vector2Int(Random.Range(rooms[i + 1].pos.x, rooms[i + 1].pos.x + rooms[i + 1].width), rooms[i + 1].pos.y + rooms[i + 1].height - 1);
                    } else {
                        //same height
                        startPos = new Vector2Int(rooms[i].pos.x, Random.Range(rooms[i].pos.y, rooms[i].pos.y + rooms[i].height));
                        endPos = new Vector2Int(Random.Range(rooms[i + 1].pos.x, rooms[i + 1].pos.x + rooms[i + 1].width), rooms[i + 1].pos.y);
                    }
                } else {
                    //same columns
                    if (rooms[i].pos.y < rooms[i + 1].pos.y) {
                        //below
                        startPos = new Vector2Int(Random.Range(rooms[i].pos.x, rooms[i].pos.x + rooms[i].width), rooms[i].pos.y + rooms[i].height - 1);
                        endPos = new Vector2Int(Random.Range(rooms[i + 1].pos.x, rooms[i + 1].pos.x + rooms[i + 1].width), rooms[i + 1].pos.y);
                    } else if (rooms[i].pos.y > rooms[i + 1].pos.y) {
                        //above
                        startPos = new Vector2Int(Random.Range(rooms[i].pos.x, rooms[i].pos.x + rooms[i].width), rooms[i].pos.y);
                        endPos = new Vector2Int(Random.Range(rooms[i + 1].pos.x, rooms[i + 1].pos.x + rooms[i + 1].width), rooms[i + 1].pos.y + rooms[i + 1].height - 1);
                    } else {
                        //this will literally NEVER happen but the compiler throws a hissy fit because it thinks it's possible
                        startPos = new Vector2Int();
                        endPos = new Vector2Int();
                    }
                }

                corridors[i] = new Corridor(startPos, endPos);
            }
        }

        for (int i = 0; i < corridors.Length; i++) {
            List<Vector2Int> corridorTiles = corridors[i].InitializeCorridor();

            foreach (Vector2Int vector in corridorTiles) {
                tilemap.SetTile(new Vector3Int(vector.x, vector.y, 0), testTile);
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
