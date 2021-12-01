using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropAssets : MonoBehaviour
{
    public static PropAssets Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            DontDestroyOnLoad(gameObject);

            Instance = this;
        }
    }

    [Header("Prop prefab")]
    public Transform propPF;

    public Sprite[] bookSprites;
    public Sprite[] chairSprites;
    public Sprite[] tableSprites;
    public Sprite[] rackSprites;
    public Sprite[] shelfSprites;
    public Sprite[] platterSprites;

    public static Sprite GetRandomSprite(Sprite[] sprites) {
        if (sprites != null) return sprites[Random.Range(0, sprites.Length)];
        return null;
    }
}
