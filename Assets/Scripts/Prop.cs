using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour {
    private int maxHealth;
    private int currentHealth;

    private float iFrameTimer = 0f;
    private float iFrameCD = .8f;

    public enum PropType {
        Book,
        Chair,
        Table
    }

    public PropType propType;
    public RectTransform fillBox;

    private Vector3 fillBoxMax;

    private SpriteRenderer sp;
    private Rigidbody2D rb;

    void Start() {
        currentHealth = maxHealth;

        sp = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

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
            default:
                break;
        }

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
        if (collision.collider.gameObject.layer == 9) {
            currentHealth -= PlayerController.Instance.attackDamage;
        } else if (collision.collider.gameObject.layer == 10) {
            currentHealth -= collision.collider.GetComponent<Enemy>().attackDamage;
        } else {
            return;
        }

        iFrameTimer = 0f;

        if (currentHealth <= 0) {
            Die();
        }

        fillBox.localScale = new Vector3(fillBoxMax.x * Mathf.Clamp(currentHealth / maxHealth, 0f, 1f), fillBoxMax.y, fillBoxMax.z);
    }

    private void Die() {
        //drop item based on rng and item determined by prop type

        Destroy(gameObject);
    }
}
