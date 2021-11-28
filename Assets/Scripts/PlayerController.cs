using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float speed;
    //attack speed

    private Rigidbody2D rb;

    private float hor;
    private float ver;

    private Vector2 move = new Vector2(0, 0);
    private Vector2 mouseDir = new Vector2(0, 0);

    public static PlayerController Instance { get; private set; }

    public int maxHealth = 10;
    private int currentHealth;

    public float attackCD = 1f;
    private float attackTimer = 0f;
    public int attackDamage = 1;

    public float iFrameCD = 1f;
    private float iFrameTimer = 0f;

    private Animator animator;

    private GameManager gm;
    void Awake() {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        gm = GameManager.Instance;

        mouseDir = GetMouseDir();
    }

    void Update() {
        if (currentHealth > 0) {
            mouseDir = GetMouseDir();
            Move();

            if (iFrameTimer < iFrameCD) iFrameTimer += Time.deltaTime;

            if (attackTimer < attackCD) {
                attackTimer += Time.deltaTime;
            } else {
                if (Input.GetKeyDown(KeyCode.Mouse0)) Attack();
            }
        }
    }

    void FixedUpdate() {
        rb.velocity = move;
    }

    private void Move() {
        hor = Input.GetAxisRaw("Horizontal");
        ver = Input.GetAxisRaw("Vertical");

        move = new Vector2(hor, ver);
        move = move.normalized * speed;

        //CHANGE TO BE BASED ON MOUSE DIR, AND FIX HOW YOU CAN CANCEL DEATH ANIM WITH MOUSE0
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
            animator.SetFloat("LookX", mouseDir.x);
            animator.SetFloat("LookY", mouseDir.y);
        }

        animator.SetFloat("MoveX", rb.velocity.normalized.x);
        animator.SetFloat("MoveY", rb.velocity.normalized.y);

        animator.SetFloat("Speed", rb.velocity.magnitude);
    }

    private Vector2 GetMouseDir() {
        Vector2 rawPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
        return (rawPos - (Vector2)transform.position).normalized;
    }

    private void Attack() {
        animator.SetTrigger("Attack");
        attackTimer = 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (currentHealth > 0) {
            //EnemyAttack layer is 10
            if (collision.collider.gameObject.tag == "Attack" && collision.collider.gameObject.layer == 10 && iFrameTimer >= iFrameCD) {
                Damage(collision.collider.gameObject.GetComponentInParent<Enemy>().attackDamage);
                iFrameTimer = 0f;
            }
        }
    }

    public void Damage(int damage) {
        if (currentHealth > 0) {
            currentHealth -= damage;

            animator.SetTrigger("Damage");
        }

        Debug.Log(currentHealth);

        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        animator.SetTrigger("Die");

        gm.enableGameOver();
        Debug.Log("Game Over");
    }

    public Vector3 GetPosition() {
        return transform.position;
    }
}
