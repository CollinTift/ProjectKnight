using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {
    public Vector2Int pos;
    public int width, height;
    public int numDoors;
    public Door[] doors;
    RoomType roomType;

    public enum RoomType {
        empty,
        combat,
        loot,
        trap,
        entrance,
        exit,
        COUNT
    }    

    public Room(Vector2Int pos, int width, int height, RoomType roomType, int numDoors) {
        this.pos = pos;

        this.width = width;
        this.height = height;

        this.roomType = roomType;

        this.numDoors = numDoors;
        doors = new Door[numDoors];

        GenerateDoors();
    }

    public bool CollidesWith(Room room) {
        for (int x = pos.x; x < pos.x + width; x++) {
            for (int y = pos.y; y < pos.y + height; y++) {
                // for each tile in this room, see if it is within the bounds of the parameter room
                if (x >= room.pos.x - 2 && x < room.pos.x + room.width + 2) {
                    if (y >= room.pos.y - 2 && y < room.pos.y + room.height + 2) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void GenerateDoors() {
        int index = 0;
        while (index < numDoors) {
            Door tempDoor;

            switch (Random.Range(0, 4)) {
                case 0:
                    tempDoor = new Door(pos.x + Random.Range(0, width), pos.y - 1, Door.Side.bottom);
                    break;
                case 1:
                    tempDoor = new Door(pos.x + Random.Range(0, width), pos.y + height, Door.Side.top);
                    break;
                case 2:
                    tempDoor = new Door(pos.x - 1, pos.y + Random.Range(0, height), Door.Side.left);
                    break;
                default:
                    tempDoor = new Door(pos.x + width, pos.y + Random.Range(0, height), Door.Side.right);
                    break;
            }

            bool safeToUse = true;

            for (int i = 0; i < index; i++) {
                if (tempDoor.pos == doors[i].pos) {
                    safeToUse = false;
                    break;
                }
            }

            if (safeToUse) {
                doors[index] = tempDoor;
                index++;
            }
        }
    }

    public class Door {
        public Vector2Int pos;
        public Door connectedDoor;
        public Side side;

        public enum Side {
            bottom,
            top,
            left,
            right
        }

        public Door(int x, int y, Side side) {
            pos = new Vector2Int(x, y);
            this.side = side;

            connectedDoor = null;
        }

        public void ConnectTo(Door target) {
            connectedDoor = target;
        }
    }
}
