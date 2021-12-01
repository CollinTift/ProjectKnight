using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {
    public enum ItemType {
        Splinter,
        Page,
        Account,
        Boot,
        Shield,
        Hat,
        Bread,
        Sword,
        Tome
    }

    public ItemType itemType;

    public static Transform SpawnItem(Vector3 worldPos, ItemType type) {
        Transform newItem = Instantiate(ItemAssets.Instance.itemPF, worldPos, Quaternion.identity);
        newItem.GetComponent<Item>().itemType = type;
        newItem.GetComponent<SpriteRenderer>().sprite = ItemAssets.Instance.GetSprite(type);
        return newItem;
    }

    public void Start() {
        gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.CompareTag("Player")) {
            PlayerController.Instance.AddItem(this);
            Destroy(gameObject);
        }
    }

    public override string ToString() {
        return base.ToString() + " " + itemType;
    }
}
