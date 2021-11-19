using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor {
    public Room startRoom;
    public Room endRoom;
    public Vector2Int startPos;
    public Vector2Int endPos;
    public int length;

    public Corridor(Vector2Int startPos, Vector2Int endPos) {
        this.startPos = startPos;
        this.endPos = endPos;
    }

    //do side RNG in GenerateMap.cs then call this for every corridor we want (try one per room for noW)
    public List<Vector2Int> InitializeCorridor() {
        List<Vector2Int> corridorTiles = new List<Vector2Int>();
        corridorTiles.Clear();

        int xDist = Mathf.Abs(endPos.x - startPos.x);
        int yDist = Mathf.Abs(endPos.y - startPos.y);

        if (startPos.x < endPos.x) {
            //start = left
            if (startPos.y < endPos.y) {
                //start = below
                for (int i = 0; i < xDist; i++) {
                    corridorTiles.Add(new Vector2Int(startPos.x + i, startPos.y));
                }

                for (int i = 0; i < yDist; i++) {
                    corridorTiles.Add(new Vector2Int(endPos.x, startPos.y + i));
                }
            } else if (startPos.y > endPos.y) {
                //start = above
                for (int i = 0; i < xDist; i++) {
                    corridorTiles.Add(new Vector2Int(startPos.x + i, startPos.y));
                }

                for (int i = 0; i < yDist; i++) {
                    corridorTiles.Add(new Vector2Int(endPos.x, startPos.y - i));
                }
            } else {
                //start = same Y
                for (int i = 0; i < xDist; i++) {
                    corridorTiles.Add(new Vector2Int(startPos.x + i, startPos.y));
                }
            }
        } else if (startPos.x > endPos.x) {
            //start = right
            if (startPos.y < endPos.y) {
                //start = below
                for (int i = 0; i < xDist; i++) {
                    corridorTiles.Add(new Vector2Int(startPos.x - i, startPos.y));
                }

                for (int i = 0; i < yDist; i++) {
                    corridorTiles.Add(new Vector2Int(endPos.x, startPos.y + i));
                }
            } else if (startPos.y > endPos.y) {
                //start = above
                for (int i = 0; i < xDist; i++) {
                    corridorTiles.Add(new Vector2Int(startPos.x - i, startPos.y));
                }

                for (int i = 0; i < yDist; i++) {
                    corridorTiles.Add(new Vector2Int(endPos.x, startPos.y - i));
                }
            } else {
                //start = same Y
                for (int i = 0; i < xDist; i++) {
                    corridorTiles.Add(new Vector2Int(startPos.x - i, startPos.y));
                }
            }
        } else {
            //start = same X
            if (startPos.y < endPos.y) {
                //start = below
                for (int i = 0; i < yDist; i++) {
                    corridorTiles.Add(new Vector2Int(startPos.x, startPos.y + i));
                }
            } else if (startPos.y > endPos.y) {
                //start = above
                for (int i = 0; i < yDist; i++) {
                    corridorTiles.Add(new Vector2Int(startPos.x, startPos.y - i));
                }
            } else {
                //start = same Y wtf this is not possible return null
                return null;
            }
        }

        return corridorTiles;
    }
}
