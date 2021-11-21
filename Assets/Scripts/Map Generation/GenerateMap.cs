using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;

public class GenerateMap : MonoBehaviour {
    Tilemap tilemap;
    GameObject vCam;
    TilemapCollider2D tc;
    Rigidbody2D rb;

    [Header("Base Sprite")]
    public Sprite[] baseSprites;
    private Tile[] baseTiles;

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
    public int[] floorWeights;
    private Tile[] floorTiles;
    private float totalWeight;
    private float[] actualWeights;

    private void Awake() {
        tilemap = GetComponent<Tilemap>();
        rooms = new Room[numRooms];
        corridors = new Corridor[numRooms - 1];

        baseTiles = InstantiateTiles(baseSprites);
        for (int i = 0; i < baseTiles.Length; i++) {
            baseTiles[i].colliderType = Tile.ColliderType.Grid;
        }

        ApplyBase();

        floorTiles = InstantiateTiles(floorSprites);

        totalWeight = 0f;
        for (int i = 0; i < floorWeights.Length; i++) {
            totalWeight += floorWeights[i];
        }

        actualWeights = new float[floorWeights.Length];
        for (int i = 0; i < actualWeights.Length; i++) {
            if (i == 0) {
                actualWeights[i] = floorWeights[i] / totalWeight;
            } else {
                actualWeights[i] = (floorWeights[i] / totalWeight) + actualWeights[i - 1];
            }
        }

        GenerateRooms();

        rb = gameObject.AddComponent<Rigidbody2D>();
        tc = gameObject.AddComponent<TilemapCollider2D>();
        gameObject.AddComponent<CompositeCollider2D>();

        tc.usedByComposite = true;
        rb.bodyType = RigidbodyType2D.Static;

        vCam = GameObject.FindWithTag("MainVCam");

        GameObject cameraConfiner = new GameObject("Camera Confiner");
        cameraConfiner.layer = 6; //camera confiner layer
        cameraConfiner.transform.position = Vector3.zero;
        cameraConfiner.AddComponent<PolygonCollider2D>();

        PolygonCollider2D pc = cameraConfiner.GetComponent<PolygonCollider2D>();
        pc.points = new Vector2[4] {
            new Vector2(0, 0),
            new Vector2(sizeX, 0),
            new Vector2(sizeX, sizeY),
            new Vector2(0, sizeY)
        };

        vCam.GetComponent<CinemachineConfiner>().m_BoundingShape2D = pc;
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

        for (int i = 0; i < numRooms; i++) {
            //rooms
            for (int x = rooms[i].pos.x; x < rooms[i].pos.x + rooms[i].width; x++) {
                for (int y = rooms[i].pos.y; y < rooms[i].pos.y + rooms[i].height; y++) {
                    tilemap.SetTile(new Vector3Int(x, y, 0), GetFloorTileWeighted());
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
                tilemap.SetTile(new Vector3Int(vector.x, vector.y, 0), GetFloorTileWeighted());
            }
        }
    }

    private void ApplyBase() {
        for (int x = 0; x < sizeX; x++) {
            for (int y = 0; y < sizeY; y++) {
                tilemap.SetTile(new Vector3Int(x, y, 0), GetBaseTileUnweighted());
            }
        }
    }

    private Tile[] InstantiateTiles(Sprite[] sprites) { 
        Tile[] tileArray = new Tile[sprites.Length];

        for (int i = 0; i < sprites.Length; i++) {
            tileArray[i] = ScriptableObject.CreateInstance<Tile>();
            tileArray[i].sprite = sprites[i];
            tileArray[i].colliderType = Tile.ColliderType.None;
        }

        return tileArray;
    }

    private Tile GetFloorTileUnweighted() {
        return floorTiles[Random.Range(0, floorTiles.Length)];
    }

    private Tile GetBaseTileUnweighted() {
        return baseTiles[Random.Range(0, baseTiles.Length)];
    }

    private Tile GetFloorTileWeighted() {
        float randVal = Random.Range(0f, totalWeight);

        for (int i = 0; i < actualWeights.Length; i++) {
            if (i == 0) {
                if (randVal < actualWeights[0] && randVal >= 0f) {
                    return floorTiles[0];
                }
            } else {
                if (randVal < actualWeights[i] && randVal >= actualWeights[i - 1]) {
                    return floorTiles[i];
                }
            }
        }

        //literally impossible but compiler throwing hissy fit yet again
        //nevermind apparently its almost always this rip
        return floorTiles[0];
    }

    //loop through each pixel per tile and create new texture2D to return and apply to a tile for procedural generation!!!!
    //https://docs.unity3d.com/ScriptReference/Tilemaps.TileBase.html
    //https://docs.unity3d.com/ScriptReference/Tilemaps.Tilemap.html
}
