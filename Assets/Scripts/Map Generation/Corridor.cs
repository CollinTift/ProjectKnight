using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor {
    public enum Direction {
        north,
        east,
        south,
        west
    }

    public Vector2Int startPos;
    public Vector2Int endPos;
    public int length;
    public Direction direction;

    public Corridor(Room room, int lengthMax, bool firstCorridor) {
        direction = (Direction)Random.Range(0, 4);

        Direction oppositeDir = (Direction)(((int)room.enteringCorDir + 2) % 4);

        //if going opposite way rotate 90 deg
        if (!firstCorridor && direction == oppositeDir) {
            direction = (Direction)(((int)direction + 1) % 4);
        }

        length = Random.Range(2, lengthMax);

        switch (direction) {
            case Direction.north:
                startPos = new Vector2Int(Random.Range(room.pos.x, room.pos.x + room.width - 1), room.pos.y + room.height);
                length = Mathf.Clamp(length, 2, Object.FindObjectOfType<GenerateMap>().sizeY - startPos.y);
                endPos = new Vector2Int(startPos.x, startPos.y + length - 1);
                break;
            case Direction.east:
                startPos = new Vector2Int(room.pos.x + room.width, Random.Range(room.pos.y, room.pos.y + room.height - 1));
                length = Mathf.Clamp(length, 2, Object.FindObjectOfType<GenerateMap>().sizeX - startPos.x);
                endPos = new Vector2Int(startPos.x + length - 1, startPos.y);
                break;
            case Direction.south:
                startPos = new Vector2Int(Random.Range(room.pos.x, room.pos.x + room.width), room.pos.y);
                length = Mathf.Clamp(length, 2, startPos.y);
                endPos = new Vector2Int(startPos.x, startPos.y - length + 1);
                break;
            case Direction.west:
                startPos = new Vector2Int(room.pos.x, Random.Range(room.pos.y, room.pos.y + room.height));
                length = Mathf.Clamp(length, 2, startPos.x);
                endPos = new Vector2Int(startPos.x - length + 1, startPos.y);
                break;
        }
    }
}
