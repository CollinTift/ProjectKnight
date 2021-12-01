using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    public int healPerWave = 1;

    public int maxChaos = 10;
    public int currentChaos;
    private float chaosLossCD = 10f;
    private float chaosLossTimer = 0f;

    public float dashSpeedMultiplier = 50f;
    private float dashTimer = 0f;
    private float dashLength = 1f;

    public float attackCD = 1f;
    private float attackTimer = 0f;

    private int shielding = 0;

    public int attackDamage = 1;
    public int thornsDamage = 0;

    public float iFrameCD = 1f;
    private float iFrameTimer = 0f;

    public int critChance = 0;
    public float critMult = 2f;
    public int memories = 0;

    private bool dashing = false;

    public RectTransform healthFillBox;
    private Vector3 healthFillBoxMax;

    public RectTransform chaosFillBox;
    private Vector2 chaosFillBoxMax; //stores width and height of max Rect

    private Animator animator;

    private GameManager gm;

    private List<InventoryItem> inventory;
    //must be 9 objects, one for each type
    public GameObject[] itemImages;

    public GameObject shield;

    void Awake() {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        currentChaos = maxChaos;

        animator = GetComponent<Animator>();
        gm = GameManager.Instance;

        mouseDir = GetMouseDir();
        
        healthFillBoxMax = healthFillBox.localScale;
        chaosFillBoxMax = new Vector2(chaosFillBox.rect.width, chaosFillBox.rect.height);

        inventory = new List<InventoryItem>();
        inventory.Clear();

        foreach (GameObject go in itemImages) {
            go.SetActive(false);
        }
    }

    void Update() {
        if (currentHealth > 0) {
            mouseDir = GetMouseDir();
            Move();

            if (iFrameTimer < iFrameCD) iFrameTimer += Time.deltaTime;

            if (shielding > 0) {
                shield.SetActive(true);
            } else {
                shield.SetActive(false);
            }

            if (currentChaos > 0) {
                if (chaosLossTimer < chaosLossCD) {
                    chaosLossTimer += Time.deltaTime;
                } else {
                    currentChaos--;
                    chaosFillBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, chaosFillBoxMax.x * (float)currentChaos / (float)maxChaos);
                    chaosLossTimer = 0f;
                }
            }

            if (attackTimer < attackCD) {
                attackTimer += Time.deltaTime;
            } else {
                if (Input.GetKeyDown(KeyCode.Mouse0)) Attack();
            }

            if (currentChaos <= maxChaos && currentChaos > 0) {
                if (Input.GetKeyDown(KeyCode.Space)) {
                    if (Mathf.Abs(move.x) >= .2f || Mathf.Abs(move.y) >= .2f) {
                        currentChaos--;
                        chaosFillBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, chaosFillBoxMax.x * (float)currentChaos / (float)maxChaos);
                        //StartCoroutine("Dash");
                        Dash();
                    } else {
                        currentChaos--;
                        chaosFillBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, chaosFillBoxMax.x * (float)currentChaos / (float)maxChaos);
                        shielding++;
                    }
                }
            }

            if (dashing) {
                dashTimer += Time.deltaTime;

                if (dashTimer >= dashLength) {
                    dashing = false;
                    dashTimer = 0f;
                }
            }
        }
    }

    void FixedUpdate() {
        if (dashing) {
            rb.velocity = move * dashSpeedMultiplier;
        } else {
            rb.velocity = move;
        }
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

    private void Dash() {
        dashing = true;
        rb.velocity = Vector2.zero;
    }

    public void AddItem(Item item) {
        InventoryItem invItem = null;
        
        bool existsInInv = false;

        foreach (InventoryItem existingItem in inventory) {
            if (existingItem.item.itemType == item.itemType) {
                existsInInv = true;
                invItem = existingItem;
                break;
            }
        }

        if (!existsInInv) {
            invItem = new InventoryItem(item, 1);
            inventory.Add(invItem);

            foreach (GameObject go in itemImages) {
                if (go.GetComponent<Image>().sprite == ItemAssets.Instance.GetSprite(item.itemType)) {
                    go.SetActive(true);
                }
            }
        } else {
            invItem.AddAmount(1);
        }

        switch (item.itemType) {
            case Item.ItemType.Splinter:
                thornsDamage++;
                
                itemImages[0].GetComponentInChildren<TextMeshProUGUI>().text = "" + invItem.amount;

                break;
            case Item.ItemType.Page:
                memories++;

                itemImages[1].GetComponentInChildren<TextMeshProUGUI>().text = "" + invItem.amount;

                break;
            case Item.ItemType.Account:
                critChance += 5;

                itemImages[2].GetComponentInChildren<TextMeshProUGUI>().text = "" + invItem.amount;

                break;
            case Item.ItemType.Boot:
                speed += 0.25f;

                itemImages[3].GetComponentInChildren<TextMeshProUGUI>().text = "" + invItem.amount;

                break;
            case Item.ItemType.Shield:
                maxHealth++;
                currentHealth++;
                healthFillBox.localScale = new Vector3(healthFillBoxMax.x * Mathf.Clamp((float)currentHealth / (float)maxHealth, 0f, 1f), healthFillBoxMax.y, healthFillBoxMax.z);

                itemImages[4].GetComponentInChildren<TextMeshProUGUI>().text = "" + invItem.amount;

                break;
            case Item.ItemType.Hat:
                maxChaos++;
                currentChaos++;
                chaosFillBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, chaosFillBoxMax.x * (float)currentChaos / (float)maxChaos);

                itemImages[5].GetComponentInChildren<TextMeshProUGUI>().text = "" + invItem.amount;

                break;
            case Item.ItemType.Bread:
                healPerWave++;
                Heal(1);

                itemImages[6].GetComponentInChildren<TextMeshProUGUI>().text = "" + invItem.amount;

                break;
            case Item.ItemType.Sword:
                attackDamage++;

                itemImages[7].GetComponentInChildren<TextMeshProUGUI>().text = "" + invItem.amount;

                break;
            case Item.ItemType.Tome:
                critMult += .1f;

                itemImages[8].GetComponentInChildren<TextMeshProUGUI>().text = "" + invItem.amount;

                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (currentHealth > 0) {
            //EnemyAttack layer is 10
            if (collision.collider.gameObject.tag == "Attack" && collision.collider.gameObject.layer == 10 && iFrameTimer >= iFrameCD) {
                Damage(collision.collider.gameObject.GetComponentInParent<Enemy>().attackDamage);

                if (thornsDamage > 0) collision.collider.gameObject.GetComponentInParent<Enemy>().Damage(thornsDamage);

                iFrameTimer = 0f;
            }
        }
    }

    public void Damage(int damage) {
        if (currentHealth > 0) {
            if (shielding > 0) {
                shielding -= damage;

                if (shielding < 0) {
                    currentHealth += shielding;
                    shielding = 0;
                }
            } else {
                currentHealth -= damage;
                animator.SetTrigger("Damage");
            }
        }

        healthFillBox.localScale = new Vector3(healthFillBoxMax.x * Mathf.Clamp((float)currentHealth / (float)maxHealth, 0f, 1f), healthFillBoxMax.y, healthFillBoxMax.z);

        if (currentHealth <= 0) {
            Die();
        }
    }

    public void Heal(int heal) {
        if (currentHealth > 0 && currentHealth <= maxHealth) {
            if (currentHealth + heal <= maxHealth) {
                currentHealth += heal;
            } else {
                currentHealth = maxHealth;
            }
        }
    }

    public void AddChaos(int chaos) {
        if (currentChaos + chaos <= maxChaos) {
            currentChaos += chaos;
        } else {
            currentChaos = maxChaos;
        }
        
        chaosFillBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, chaosFillBoxMax.x * (float)currentChaos / (float)maxChaos);
    }

    private void Die() {
        animator.SetTrigger("Die");

        gm.enableGameOver();
        Debug.Log("Game Over");
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    public class InventoryItem {
        public Item item { get; private set; }
        public int amount { get; private set; }

        public InventoryItem(Item item, int amount) {
            this.item = item;
            this.amount = amount;
        }

        public void AddAmount(int amount) {
            this.amount = this.amount + amount;
        }
    }
}
