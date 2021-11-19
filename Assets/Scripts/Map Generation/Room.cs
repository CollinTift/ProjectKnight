using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {
    public Vector2Int pos;
    public int width, height;
    public RoomType roomType;
    public Corridor.Direction enteringCorDir;

    public enum RoomType {
        empty,
        combat,
        loot,
        trap,
        entrance,
        exit,
        COUNT
    }    

    public Room(Vector2Int pos, int width, int height, RoomType roomType):this(pos, width, height, roomType, null) {}

    public Room(Vector2Int pos, int width, int height, RoomType roomType, Corridor corridor) {
        this.pos = pos;

        this.width = width;
        this.height = height;

        this.roomType = roomType;

        if (corridor != null) {
            enteringCorDir = corridor.direction;
        }
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
}
