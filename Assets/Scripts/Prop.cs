using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour {
    private int maxHealth;
    private int currentHealth;

    private float iFrameTimer = 0f;
    private float iFrameCD = .6f;

    public enum PropType {
        Book,
        Chair,
        Table,
        WeaponRack,
        Bookshelf,
        Platter
    }

    public PropType propType;

    public RectTransform fillBox;
    private Vector3 fillBoxMax;

    private SpriteRenderer sp;
    private Rigidbody2D rb;

    public static void SpawnProp(Vector3 worldPos, PropType type, Transform parent) {
        Transform newProp = Instantiate(PropAssets.Instance.propPF, worldPos, Quaternion.identity, parent);
        newProp.GetComponent<Prop>().propType = type;
    }

    void Start() {
        sp = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        switch (propType) {
            case PropType.Book:
                maxHealth = 1;
                sp.sprite = PropAssets.GetRandomSprite(PropAssets.Instance.bookSprites);
                rb.drag = 5f;
                break;
            case PropType.Chair:
                maxHealth = 3;
                sp.sprite = PropAssets.GetRandomSprite(PropAssets.Instance.chairSprites);
                rb.drag = 10f;
                break;
            case PropType.Table:
                maxHealth = 5;
                sp.sprite = PropAssets.GetRandomSprite(PropAssets.Instance.tableSprites);
                rb.drag = 20f;
                break;
            case PropType.WeaponRack:
                maxHealth = 7;
                sp.sprite = PropAssets.GetRandomSprite(PropAssets.Instance.rackSprites);
                rb.drag = 10f;
                break;
            case PropType.Bookshelf:
                maxHealth = 6;
                sp.sprite = PropAssets.GetRandomSprite(PropAssets.Instance.shelfSprites);
                rb.drag = 30f;
                break;
            case PropType.Platter:
                maxHealth = 2;
                sp.sprite = PropAssets.GetRandomSprite(PropAssets.Instance.platterSprites);
                rb.drag = 5f;
                break;
            default:
                break;
        }

        currentHealth = maxHealth;
        gameObject.AddComponent<PolygonCollider2D>();
        fillBoxMax = fillBox.localScale;
    }

    private void Update() {
        if (iFrameTimer < iFrameCD) {
            iFrameTimer += Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        //player is 9, enemy is 10
        if (collision.collider.gameObject.layer == 9 && iFrameTimer >= iFrameCD) {
            currentHealth -= PlayerController.Instance.attackDamage;
        } else if (collision.collider.gameObject.layer == 10 && iFrameTimer >= iFrameCD) {
            currentHealth -= collision.collider.GetComponent<Enemy>().attackDamage;
        }

        iFrameTimer = 0f;

        if (currentHealth <= 0) {
            Die();
        }

        fillBox.localScale = new Vector3(fillBoxMax.x * Mathf.Clamp((float)currentHealth / (float)maxHealth, 0f, 1f), fillBoxMax.y, fillBoxMax.z);
    }

    private void Die() {
        //drop item based on rng and item determined by prop type
        int randDrop = Random.Range(0, 100);

        switch (propType) {
            case PropType.Book:
                if (randDrop < 10) {
                    Item.SpawnItem(transform.position, Item.ItemType.Account);
                } else if (randDrop >= 10 && randDrop < 40) {
                    Item.SpawnItem(transform.position, Item.ItemType.Page);
                }
                
                break;
            case PropType.Chair:
                if (randDrop < 20) Item.SpawnItem(transform.position, Item.ItemType.Splinter);

                break;
            case PropType.Table:
                if (randDrop < 40) Item.SpawnItem(transform.position, Item.ItemType.Splinter);

                break;
            case PropType.WeaponRack:
                if (randDrop < 25) {
                    Item.SpawnItem(transform.position, Item.ItemType.Sword);
                } else if (randDrop >= 25 && randDrop < 50) {
                    Item.SpawnItem(transform.position, Item.ItemType.Shield);
                }

                break;
            case PropType.Bookshelf:
                if (randDrop < 10) {
                    Item.SpawnItem(transform.position, Item.ItemType.Tome);
                } else if (randDrop >= 10 && randDrop < 30) {
                    Item.SpawnItem(transform.position, Item.ItemType.Splinter);
                } else if (randDrop >= 30 && randDrop < 50) {
                    Item.SpawnItem(transform.position, Item.ItemType.Page);
                }

                break;
            case PropType.Platter:
                if (randDrop < 50) Item.SpawnItem(transform.position, Item.ItemType.Bread);

                break;
        }

        PlayerController.Instance.currentChaos++;

        Destroy(gameObject);
    }
}
