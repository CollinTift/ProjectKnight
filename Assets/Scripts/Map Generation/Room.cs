using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {
    public Vector2Int pos;
    public int width, height;
    public int numDoors;
    public Door[] doors;

    public enum RoomType {
        combat,
        loot,
        trap,
        entrance,
        exit,
        COUNT
    }

    RoomType roomType;

    public Room(Vector2Int pos, int width, int height, RoomType roomType, int numDoors) {
        this.pos = pos;

        this.width = width;
        this.height = height;

        this.roomType = roomType;

        this.numDoors = numDoors;

        GenerateDoors();
    }

    public bool CollidesWith(Room room) {
        for (int x = pos.x; x < pos.x + width; x++) {
            for (int y = pos.y; y < pos.y + height; y++) {
                // for each tile in this room, see if it is within the bounds of the parameter room
                if (x >= room.pos.x && x < room.pos.x + room.width) {
                    if (y >= room.pos.y && y < room.pos.y + room.height) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void GenerateDoors() {
        
    }

    public class Door {
        public Vector2Int pos;
        public Door connectedDoor;

        public Door(int x, int y) {
            pos = new Vector2Int(x, y);
            connectedDoor = null;
        }

        public void ConnectTo(Door target) {
            connectedDoor = target;
        }
    }
}
