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
    private int currentChaos;
    private float chaosLossCD = 5f;
    private float chaosLossTimer = 0f;

    public float dashSpeedMultiplier = 2f;

    public float attackCD = 1f;
    private float attackTimer = 0f;

    public int attackDamage = 1;
    public int thornsDamage = 0;

    public float iFrameCD = 1f;
    private float iFrameTimer = 0f;

    public int critChance = 0;
    public float critMult = 2f;
    public int memories = 0;

    public RectTransform fillBox;
    private Vector3 fillBoxMax;

    private Animator animator;

    private GameManager gm;

    private List<InventoryItem> inventory;
    //must be 9 objects, one for each type
    public GameObject[] itemImages;

    void Awake() {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        currentChaos = maxChaos;

        animator = GetComponent<Animator>();
        gm = GameManager.Instance;

        mouseDir = GetMouseDir();
        
        fillBoxMax = fillBox.localScale;

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

            if (chaosLossTimer < chaosLossCD) {
                chaosLossTimer += Time.deltaTime;
            } else {
                currentChaos--;
                chaosLossTimer = 0f;
            }

            if (attackTimer < attackCD) {
                attackTimer += Time.deltaTime;
            } else {
                if (Input.GetKeyDown(KeyCode.Mouse0)) Attack();
            }

            if (currentChaos < maxChaos - 1) {
                if (Input.GetKeyDown(KeyCode.Space)) {
                    currentChaos--;
                    StartCoroutine("Dash");
                }
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

    private IEnumerator Dash() {
        for (float dashSpd = speed * dashSpeedMultiplier; dashSpd >= speed; dashSpd -= 0.1f) {
            move = new Vector2(hor, ver);
            move = move.normalized * dashSpd;
            yield return null;
        }

        StopCoroutine("Dash");
    }

    public void AddItem(Item item) {
        InventoryItem invItem = null;
        
        bool existsInInv = false;
        //InventoryItem existingInvItem = null;

        foreach (InventoryItem existingItem in inventory) {
            if (existingItem.item.itemType == item.itemType) {
                existsInInv = true;
                invItem = existingItem;
            }
        }

        if (!existsInInv) {
            invItem = new InventoryItem(item, 1);

            foreach (GameObject go in itemImages) {
                if (go.GetComponent<Image>().sprite == ItemAssets.Instance.GetSprite(item.itemType)) {
                    go.SetActive(true);
                }
            }
        } else {
            invItem.AddAmount(1);
        }

        if (invItem == null) Debug.Log("NULL");

        switch (item.itemType) {
            case Item.ItemType.Splinter:
                thornsDamage++;
                //if (!itemImages[0].activeInHierarchy) itemImages[0].SetActive(true);
                
                itemImages[0].GetComponentInChildren<TextMeshProUGUI>().text = invItem.amount.ToString();

                break;
            case Item.ItemType.Page:
                memories++;
                //if (!itemImages[1].activeInHierarchy) itemImages[1].SetActive(true);

                itemImages[1].GetComponentInChildren<TextMeshProUGUI>().text = invItem.amount.ToString();

                break;
            case Item.ItemType.Account:
                critChance += 5;
                //if (!itemImages[2].activeInHierarchy) itemImages[2].SetActive(true);

                itemImages[2].GetComponentInChildren<TextMeshProUGUI>().text = invItem.amount.ToString();

                break;
            case Item.ItemType.Boot:
                speed += 0.25f;
                //if (!itemImages[3].activeInHierarchy) itemImages[3].SetActive(true);

                itemImages[3].GetComponentInChildren<TextMeshProUGUI>().text = invItem.amount.ToString();

                break;
            case Item.ItemType.Shield:
                maxHealth++;
                currentHealth++;
                fillBox.localScale = new Vector3(fillBoxMax.x * Mathf.Clamp((float)currentHealth / (float)maxHealth, 0f, 1f), fillBoxMax.y, fillBoxMax.z);

                //if (!itemImages[4].activeInHierarchy) itemImages[4].SetActive(true);

                itemImages[4].GetComponentInChildren<TextMeshProUGUI>().text = invItem.amount.ToString();

                break;
            case Item.ItemType.Hat:
                maxChaos++;
                currentChaos++;
                //if (!itemImages[5].activeInHierarchy) itemImages[5].SetActive(true);

                itemImages[5].GetComponentInChildren<TextMeshProUGUI>().text = invItem.amount.ToString();

                break;
            case Item.ItemType.Bread:
                healPerWave++;
                Heal(1);
                //if (!itemImages[6].activeInHierarchy) itemImages[6].SetActive(true);

                itemImages[6].GetComponentInChildren<TextMeshProUGUI>().text = invItem.amount.ToString();

                break;
            case Item.ItemType.Sword:
                attackDamage++;
                //if (!itemImages[7].activeInHierarchy) itemImages[7].SetActive(true);

                itemImages[7].GetComponentInChildren<TextMeshProUGUI>().text = invItem.amount.ToString();

                break;
            case Item.ItemType.Tome:
                critMult += .1f;
                //if (!itemImages[8].activeInHierarchy) itemImages[8].SetActive(true);

                itemImages[8].GetComponentInChildren<TextMeshProUGUI>().text = invItem.amount.ToString();

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
            currentHealth -= damage;

            animator.SetTrigger("Damage");
        }

        fillBox.localScale = new Vector3(fillBoxMax.x * Mathf.Clamp((float)currentHealth / (float)maxHealth, 0f, 1f), fillBoxMax.y, fillBoxMax.z);

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
            this.amount += amount;
        }
    }
}
