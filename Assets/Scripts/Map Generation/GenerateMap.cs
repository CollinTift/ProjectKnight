using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;
using Pathfinding;

public class GenerateMap : MonoBehaviour {
    Tilemap tilemap;
    GameObject vCam;
    TilemapCollider2D tc;
    CompositeCollider2D cc;
    Rigidbody2D rb;

    [Header("Base Sprite")]
    //Must be a 3x5 grid, in the following layout left to right top to bottom:
    //Top left corner, top edge, top right corner,
    //left edge, BASE SPRITE WITHOUT WALL, right edge,
    //Bot left corner, bot edge, bot right corner
    //Inverse top left corner, inverse top right corner, inverse bot left corner
    //Inverse bot right corner, double horizontal, double vertical
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

        rb = gameObject.AddComponent<Rigidbody2D>();
        tc = gameObject.AddComponent<TilemapCollider2D>();
        cc = gameObject.AddComponent<CompositeCollider2D>();

        tc.usedByComposite = true;
        cc.geometryType = CompositeCollider2D.GeometryType.Polygons;
        
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

        SetupMap();
    }

    public void SetupMap() {
        ApplyBase();

        GenerateRooms();
        UpdateBaseTiles();

        if (GameManager.Instance.enemiesThisWave > 0) SpawnEnemies(GameManager.Instance.enemiesThisWave);
        SpawnProps(10 + GameManager.Instance.currentWave * 3);

        AstarPath.active.Scan();
    }

    private void Update() {
        if (GameManager.Instance.enemiesThisWave > 0) {
            SpawnEnemies(GameManager.Instance.enemiesThisWave);
            PlayerController.Instance.Heal(PlayerController.Instance.healPerWave);
            GameManager.Instance.enemiesThisWave = 0;
        }
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
            } else if (index == 2) {
                randType = Room.RoomType.armory;
            } else if (index == 3) {
                randType = Room.RoomType.library;
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

    private void SpawnEnemies(int numEnemies) {
        GameObject waveHolder = new GameObject("Wave: " + GameManager.Instance.currentWave + ", Num: " + GameManager.Instance.enemiesThisWave);
        waveHolder.layer = 7;

        List<Room> spawnableRooms = new List<Room>();
        spawnableRooms.Clear();

        foreach (Room room in rooms) {
            switch (room.roomType) {
                case Room.RoomType.armory:
                case Room.RoomType.library:
                    spawnableRooms.Add(room);
                    break;
                default:
                    break;
            }
        }

        for (int i = 0; i < numEnemies; i++) {
            Room randRoom = spawnableRooms[Random.Range(0, spawnableRooms.Count)];

            Transform enemy;

            switch (randRoom.roomType) {
                case Room.RoomType.armory:
                    enemy = Enemy.SpawnEnemy(
                        new Vector3(Random.Range(randRoom.pos.x, randRoom.pos.x + randRoom.width) + .5f, Random.Range(randRoom.pos.y, randRoom.pos.y + randRoom.height) + .5f, 0),
                        Enemy.EnemyType.SwordKnight,
                        waveHolder.transform);
                    break;
                case Room.RoomType.library:
                    enemy = Enemy.SpawnEnemy(
                        new Vector3(Random.Range(randRoom.pos.x, randRoom.pos.x + randRoom.width) + .5f, Random.Range(randRoom.pos.y, randRoom.pos.y + randRoom.height) + .5f, 0),
                        Enemy.EnemyType.MageKnight,
                        waveHolder.transform);
                    break;
                default:
                    enemy = null;
                    break;
            }

            enemy.GetComponent<Enemy>().maxHealth += Mathf.RoundToInt(GameManager.Instance.currentWave * GameManager.Instance.difficulty);
            enemy.GetComponent<Enemy>().attackDamage += Mathf.RoundToInt(GameManager.Instance.currentWave * GameManager.Instance.difficulty);
        }
    }

    private void SpawnProps(int numProps) {
        GameObject propHolder = new GameObject("Prop holder");

        List<Room> spawnableRooms = new List<Room>();
        spawnableRooms.Clear();

        foreach (Room room in rooms) {
            switch (room.roomType) {
                case Room.RoomType.armory:
                case Room.RoomType.library:
                case Room.RoomType.diner:
                case Room.RoomType.treasury:
                    spawnableRooms.Add(room);
                    break;
                case Room.RoomType.exit:
                    Portal.SpawnPortal(new Vector3(Random.Range(room.pos.x, room.pos.x + room.width) + .5f, Random.Range(room.pos.y, room.pos.y + room.height) + .5f, 0f));
                    break;
                default:
                    break;
            }
        }

        for (int i = 0; i < numProps; i++) {
            Room randRoom = spawnableRooms[Random.Range(0, spawnableRooms.Count)];
            int randProp;
            Prop.PropType randType;

            switch (randRoom.roomType) {
                default:
                case Room.RoomType.armory:
                    //spawn weapon racks, tables, chairs
                    randProp = Random.Range(0, 3);

                    switch(randProp) {
                        default:
                        case 0:
                            randType = Prop.PropType.WeaponRack;
                            break;
                        case 1:
                            randType = Prop.PropType.Table;
                            break;
                        case 2:
                            randType = Prop.PropType.Chair;
                            break;
                    }

                    break;
                case Room.RoomType.library:
                    //spawn books, tables, chairs, bookshelves
                    randProp = Random.Range(0, 4);

                    switch (randProp) {
                        default:
                        case 0:
                            randType = Prop.PropType.Book;
                            break;
                        case 1:
                            randType = Prop.PropType.Table;
                            break;
                        case 2:
                            randType = Prop.PropType.Chair;
                            break;
                        case 3:
                            randType = Prop.PropType.Bookshelf;
                            break;
                    }
                    break;
                case Room.RoomType.diner:
                    //spawn platters, tables, chairs
                    randProp = Random.Range(0, 3);

                    switch (randProp) {
                        default:
                        case 0:
                            randType = Prop.PropType.Platter;
                            break;
                        case 1:
                            randType = Prop.PropType.Table;
                            break;
                        case 2:
                            randType = Prop.PropType.Chair;
                            break;
                    }
                    
                    break;
                case Room.RoomType.treasury:
                    randProp = Random.Range(0, 6);

                    randType = (Prop.PropType)randProp;

                    break;
            }

            Prop.SpawnProp(
                new Vector3(Random.Range(randRoom.pos.x, randRoom.pos.x + randRoom.width) + .5f, Random.Range(randRoom.pos.y, randRoom.pos.y + randRoom.height) + .5f, 0),
                randType,
                propHolder.transform);
        }
    }

    private void ApplyBase() {
        for (int x = 0; x < sizeX; x++) {
            for (int y = 0; y < sizeY; y++) {
                tilemap.SetTile(new Vector3Int(x, y, 0), GetBaseTileUnweighted());
            }
        }
    }

    private void UpdateBaseTiles() {
        Vector3Int upLeft, up, upRight, left, right, downLeft, down, downRight;
        for (int x = 0; x < sizeX; x++) {
            for (int y = 0; y < sizeY; y++) {
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                bool isFloor = false;                

                for (int i = 0; i < floorTiles.Length; i++) {
                    if (tilemap.GetTile(tilePos) == floorTiles[i]) {
                        isFloor = true;
                        break;
                    }
                }

                if (!isFloor) {
                    bool hasUpLeft = false;
                    bool hasUp = false;
                    bool hasUpRight = false;
                    bool hasLeft = false;
                    bool hasRight = false;
                    bool hasDownLeft = false;
                    bool hasDown = false;
                    bool hasDownRight = false;

                    upLeft = new Vector3Int(x - 1, y + 1, 0);
                    up = new Vector3Int(x, y + 1, 0);
                    upRight = new Vector3Int(x + 1, y + 1, 0);
                    left = new Vector3Int(x - 1, y, 0);
                    right = new Vector3Int(x + 1, y, 0);
                    downLeft = new Vector3Int(x - 1, y - 1, 0);
                    down = new Vector3Int(x, y - 1, 0);
                    downRight = new Vector3Int(x + 1, y - 1, 0);                    

                    for (int i = 0; i < floorTiles.Length; i++) {
                        if (tilemap.HasTile(upLeft) && tilemap.GetTile(upLeft) == floorTiles[i]) {
                            hasUpLeft = true;
                        }

                        if (tilemap.HasTile(up) && tilemap.GetTile(up) == floorTiles[i]) {
                            hasUp = true;
                        }

                        if (tilemap.HasTile(upRight) && tilemap.GetTile(upRight) == floorTiles[i]) {
                            hasUpRight = true;
                        }

                        if (tilemap.HasTile(left) && tilemap.GetTile(left) == floorTiles[i]) {
                            hasLeft = true;
                        }

                        if (tilemap.HasTile(right) && tilemap.GetTile(right) == floorTiles[i]) {
                            hasRight = true;
                        }

                        if (tilemap.HasTile(downLeft) && tilemap.GetTile(downLeft) == floorTiles[i]) {
                            hasDownLeft = true;
                        }

                        if (tilemap.HasTile(down) && tilemap.GetTile(down) == floorTiles[i]) {
                            hasDown = true;
                        }

                        if (tilemap.HasTile(downRight) && tilemap.GetTile(downRight) == floorTiles[i]) {
                            hasDownRight = true;
                        }
                    }

                    if (hasUp) {
                        if (hasLeft) {
                            //top left
                            tilemap.SetTile(tilePos, baseTiles[0]);
                        } else if (hasRight) {
                            //top right
                            tilemap.SetTile(tilePos, baseTiles[2]);
                        } else if (hasDown) {
                            //double hor
                            tilemap.SetTile(tilePos, baseTiles[13]);
                        } else {
                            //top
                            tilemap.SetTile(tilePos, baseTiles[1]);
                        }
                    } else if (hasDown) {
                        if (hasLeft) {
                            //down left
                            tilemap.SetTile(tilePos, baseTiles[6]);
                        } else if (hasRight) {
                            //down right
                            tilemap.SetTile(tilePos, baseTiles[8]);
                        } else {
                            //down
                            tilemap.SetTile(tilePos, baseTiles[7]);
                        }
                    } else if (hasLeft) {
                        if (hasRight) {
                            //double vert
                            tilemap.SetTile(tilePos, baseTiles[14]);
                        } else {
                            //left
                            tilemap.SetTile(tilePos, baseTiles[3]);
                        }
                    } else if (hasRight) {
                        //right
                        tilemap.SetTile(tilePos, baseTiles[5]);
                    } else if (hasUpLeft) {
                        //up left
                        tilemap.SetTile(tilePos, baseTiles[9]);
                    } else if (hasUpRight) {
                        //up right
                        tilemap.SetTile(tilePos, baseTiles[10]);
                    } else if (hasDownLeft) {
                        //down left
                        tilemap.SetTile(tilePos, baseTiles[11]);
                    } else if (hasDownRight) {
                        //down right
                        tilemap.SetTile(tilePos, baseTiles[12]);
                    } else {
                        //center
                        tilemap.SetTile(tilePos, baseTiles[4]);
                    }
                }
            }
        }

        AstarPath.active.AddWorkItem(new AstarWorkItem(ctx => {
            var gg = AstarPath.active.data.gridGraph;

            for (int x = 0; x < sizeX; x++) {
                for (int y = 0; y < sizeY; y++) {
                    var node = gg.GetNode(x, y);

                    bool isBaseTile = false;
                    for (int i = 0; i < baseTiles.Length; i++) {
                        if (tilemap.GetTile(new Vector3Int(x, y, 0)) == baseTiles[i]) {
                            isBaseTile = true;
                            break;
                        }
                    }

                    if (isBaseTile) {
                        node.Walkable = false;
                    } else {
                        node.Walkable = true;
                    }
                }
            }

            gg.GetNodes(node => gg.CalculateConnections((GridNodeBase)node));
        }));
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

    public void GenerateNewRoom() {
        //should clear tilemap, generate new room, scan for A* graph, do everything required
    }

    //loop through each pixel per tile and create new texture2D to return and apply to a tile for procedural generation!!!!
    //https://docs.unity3d.com/ScriptReference/Tilemaps.TileBase.html
    //https://docs.unity3d.com/ScriptReference/Tilemaps.Tilemap.html
}
