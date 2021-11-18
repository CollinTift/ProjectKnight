using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {
    int x, y;
    int width, height;

    public enum RoomType {
        combat,
        loot,
        trap,
        exit
    }

    RoomType roomType;

    public Room(int x, int y, int width, int height, RoomType roomType) {
        this.x = x;
        this.y = y;

        this.width = width;
        this.height = height;

        this.roomType = roomType;
    }
}
