using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAssets : MonoBehaviour {
    public static ItemAssets Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            DontDestroyOnLoad(gameObject);

            Instance = this;
        }
    }

    [Header("Item Prefab")]
    public Transform itemPF;

    public Sprite splinterSprite;
    public Sprite pageSprite;
    public Sprite accountSprite;
    public Sprite bootSprite;
    public Sprite shieldSprite;
    public Sprite hatSprite;
    public Sprite breadSprite;
    public Sprite swordSprite;
    public Sprite tomeSprite;

    public Sprite GetSprite(Item.ItemType type) {
        switch (type) {
            case Item.ItemType.Splinter: return splinterSprite;
            case Item.ItemType.Page: return pageSprite;
            case Item.ItemType.Account: return accountSprite;
            case Item.ItemType.Boot: return bootSprite;
            case Item.ItemType.Shield: return shieldSprite;
            case Item.ItemType.Hat: return hatSprite;
            case Item.ItemType.Bread: return breadSprite;
            case Item.ItemType.Sword: return swordSprite;
            case Item.ItemType.Tome: return tomeSprite;
        }

        return null;
    }
}
