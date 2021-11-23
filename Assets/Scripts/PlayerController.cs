using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float speed;
    //attack speed

    private Rigidbody2D rb;

    private float hor;
    private float ver;

    private Vector2 lookDir;

    public static PlayerController Instance { get; private set; }

    public int maxHealth = 10;
    private int currentHealth;

    void Awake() {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        lookDir = new Vector2(1, 0);
    }

    void Update() {
        Move();
    }

    void FixedUpdate() {
        rb.velocity = new Vector2(hor * speed, ver * speed);
    }

    private void Move() {
        hor = Input.GetAxis("Horizontal");
        ver = Input.GetAxis("Vertical");
        Vector2 move = new Vector2(hor, ver);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f)) {
            lookDir.Set(move.x, move.y);
            lookDir.Normalize();
        }
    }

    public void Damage(int damage) {
        currentHealth -= damage;

        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        Debug.Log("Game Over");
    }

    public Vector3 GetPosition() {
        return transform.position;
    }
}
