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

    private Animator animator;

    void Awake() {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        lookDir = new Vector2(1, 0);

        animator = GetComponent<Animator>();
    }

    void Update() {
        if (currentHealth > 0) Move();
        Debug.Log(currentHealth);
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

        animator.SetFloat("MoveX", rb.velocity.normalized.x);
        animator.SetFloat("MoveY", rb.velocity.normalized.y);
        animator.SetFloat("Speed", rb.velocity.magnitude);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (currentHealth > 0) {
            //enemy layer is 7
            if (collision.collider.gameObject.tag == "Attack" && collision.collider.gameObject.layer == 7) {
                Damage(collision.collider.gameObject.GetComponentInParent<Enemy>().attackDamage);
            }
        }
    }

    public void Damage(int damage) {
        if (currentHealth > 0) {
            currentHealth -= damage;

            animator.SetTrigger("Damage");
        }

        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        animator.SetTrigger("Die");
        Debug.Log("Game Over");
    }

    public Vector3 GetPosition() {
        return transform.position;
    }
}
