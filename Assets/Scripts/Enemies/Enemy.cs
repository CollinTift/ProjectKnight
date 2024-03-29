using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour {
    [Header("Health")]
    public int maxHealth;
    private int currentHealth;

    public EnemyType type;
    private State state;
    
    private Vector3 homePos;

    private Vector3 lastKnownPlayerPos;
    private bool playerLeftSight;

    private Vector2 lookDir = new Vector2(1, 0);

    public float detectionRange = 3f;

    public float attackRange = 1f;
    public float attackCD = 3f;
    public int attackDamage = 1;

    public GameObject projectile;

    public float iFrameCD = 1f;
    private float iFrameTimer = 0f;

    private float surveyTimer = 0f;
    private float attackTimer = 0f;

    private AIDestinationSetter aIDestinationSetter;
    private AIPath aiPath;

    private Rigidbody2D rb;
    private Animator animator;
    private GameObject target;
    private GridGraph gg;

    private bool canSpawnProj = true; //must be done bc called in animator, is scuffed

    public RectTransform fillBox;
    private Vector3 fillBoxMax;

    public enum EnemyType {
        SwordKnight,
        MageKnight,
        CaptainKnight,
        ArchMage
    }

    private enum State {
        Roaming,
        Surveying,
        Chasing,
        Attacking,
        Returning
    }

    private enum AbilityType {
        Attack,
        Block
    }

    private bool lastWasAttack = true;
    private bool shielding = false;

    public static Transform SpawnEnemy(Vector3 worldPos, EnemyType type, Transform parent) {
        Transform newEnemy;

        switch (type) {
            default:
            case EnemyType.SwordKnight:
                newEnemy = Instantiate(EnemyAssets.Instance.swordKnightPF, worldPos, Quaternion.identity, parent);
                break;
            case EnemyType.MageKnight:
                newEnemy = Instantiate(EnemyAssets.Instance.mageKnightPF, worldPos, Quaternion.identity, parent);
                break;
            case EnemyType.CaptainKnight:
                newEnemy = Instantiate(EnemyAssets.Instance.captainKnightPF, worldPos, Quaternion.identity, parent);
                break;
            case EnemyType.ArchMage:
                newEnemy = Instantiate(EnemyAssets.Instance.archMagePF, worldPos, Quaternion.identity, parent);
                break;
        }

        return newEnemy;
    }

    private void Awake() {
        state = State.Roaming;

        //for now
        aIDestinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        gg = AstarPath.active.data.gridGraph;

        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        aiPath = gameObject.GetComponent<AIPath>();
        animator = gameObject.GetComponent<Animator>();

        target = new GameObject("target");        
    }

    private void Start() {
        homePos = transform.position;
        currentHealth = maxHealth;

        SetTarget(GetRoamingPos());
        aIDestinationSetter.target = target.transform;
        fillBoxMax = fillBox.localScale;
    }

    private void Update() {
        if (attackTimer < attackCD) attackTimer += Time.deltaTime;
        if (iFrameTimer < iFrameCD) iFrameTimer += Time.deltaTime;

        switch (state) {
            default:
            case State.Roaming:
                //if at roam pos, delay, then get new roam pos
                if (Vector2.Distance(transform.position, target.transform.position) < .5f) state = State.Surveying;

                if (Vector2.Distance(transform.position, PlayerController.Instance.GetPosition()) < detectionRange) state = State.Chasing;

                break;
            case State.Surveying:
                //time survey, then get roaming position
                
                if (surveyTimer < 5f) {
                    surveyTimer += Time.deltaTime;
                    if (Vector2.Distance(transform.position, PlayerController.Instance.GetPosition()) < detectionRange) {
                        state = State.Chasing;
                    }
                } else {
                    surveyTimer = 0f;

                    SetTarget(GetRoamingPos());
                    state = State.Roaming;
                }

                break;
            case State.Chasing:
                playerLeftSight = Vector2.Distance(transform.position, PlayerController.Instance.GetPosition()) > detectionRange;
                
                if (!playerLeftSight) {
                    lastKnownPlayerPos = PlayerController.Instance.GetPosition();
                    SetTarget(lastKnownPlayerPos);

                    if (Vector2.Distance(transform.position, lastKnownPlayerPos) < attackRange) state = State.Attacking;
                } else {
                    SetTarget(lastKnownPlayerPos);

                    if (Vector2.Distance(transform.position, lastKnownPlayerPos) < .5f) {
                        if (Vector2.Distance(transform.position, PlayerController.Instance.GetPosition()) < detectionRange) {
                            SetTarget(PlayerController.Instance.GetPosition());
                        } else {
                            state = State.Roaming;
                        }
                    }
                }

                //RaycastHit2D cast = Physics2D.Raycast(transform.position, lookDir, attackRange);
                //if (cast.collider.CompareTag("Prop")) {
                //    state = State.Attacking;
                //}

                break;
            case State.Attacking:
                //play attack anim; deal damage if hit player
                if (attackTimer >= attackCD) {
                    if (type == EnemyType.SwordKnight) {
                        if (lastWasAttack) {
                            animator.SetTrigger("Block");
                            lastWasAttack = false;
                        } else {
                            animator.ResetTrigger("Block");
                            animator.SetTrigger("Attack");
                            lastWasAttack = true;
                        }
                    } else if (type == EnemyType.MageKnight) {
                        animator.SetTrigger("Attack");
                    }

                    attackTimer = 0f;
                    canSpawnProj = true;
                } else {
                    animator.ResetTrigger("Attack");
                }

                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attacking")) state = State.Chasing;

                break;
            case State.Returning:
                //move to home position
                if (Vector2.Distance(transform.position, homePos) > .5f) {
                    SetTarget(homePos);
                } else {
                    state = State.Roaming;
                }

                break;
        }

        if (Mathf.Abs(aiPath.velocity.magnitude) > 0 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attacking")) {
            lookDir.x = aiPath.velocity.x;
            lookDir.y = aiPath.velocity.y;

            animator.SetFloat("LookX", lookDir.x);
            animator.SetFloat("LookY", lookDir.y);
        }
        
        animator.SetFloat("MoveX", aiPath.velocity.normalized.x);
        animator.SetFloat("MoveY", aiPath.velocity.normalized.y);
        animator.SetFloat("Speed", aiPath.velocity.magnitude);
    }

    public void AllowMovement() {
        aiPath.canMove = true;
    }

    public void DenyMovement() {
        rb.velocity = Vector2.zero;
        aiPath.canMove = false;
    }

    public void SpawnMageProj() {
        if (projectile != null && canSpawnProj) {
            GameObject go = Instantiate(projectile, transform.position, Quaternion.identity);
            Projectile proj = go.GetComponent<Projectile>();
            proj.Launch(PlayerController.Instance.GetPosition() - transform.position, attackDamage, 1f);

            canSpawnProj = false;
        }
    }

    private void SetTarget(Vector3 targetPos) {
        target.transform.position = targetPos;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (currentHealth > 0) {
            //PlayerAttack layer is 9
            if (collision.collider.gameObject.tag == "Attack" && collision.collider.gameObject.layer == 9 && iFrameTimer >= iFrameCD) {
                if (shielding) {
                    //play shield bounce sound
                } else {
                    //play damage sound

                    //handle crits
                    int randCrit = Random.Range(0, 100);
                    if (randCrit < PlayerController.Instance.critChance) {
                        Damage(Mathf.RoundToInt(PlayerController.Instance.attackDamage * PlayerController.Instance.critMult));
                    } else {
                        Damage(PlayerController.Instance.attackDamage);
                    }
                }

                iFrameTimer = 0f;
            }
        }

        fillBox.localScale = new Vector3(fillBoxMax.x * Mathf.Clamp((float)currentHealth / (float)maxHealth, 0f, 1f), fillBoxMax.y, fillBoxMax.z);
    }

    public void SetShielding(int even) {
        if (even % 2 == 0) {
            shielding = true;
        } else {
            shielding = false;
        }
    }

    public void Damage(int damage) {
        fillBox.localScale = new Vector3(fillBoxMax.x * Mathf.Clamp((float)currentHealth / (float)maxHealth, 0f, 1f), fillBoxMax.y, fillBoxMax.z);
        
        currentHealth -= damage;

        animator.SetTrigger("Damage");

        if (currentHealth <= 0) Die();
    }

    private void Die() {
        PlayerController.Instance.AddChaos(maxHealth);

        //chance to spawn items
        animator.SetTrigger("Die");

        int randDrop = Random.Range(0, 100);
        switch (type) {
            case EnemyType.SwordKnight:
                if (randDrop < 5) {
                    Item.SpawnItem(transform.position, Item.ItemType.Shield);
                } else if (randDrop >= 5 && randDrop < 10) {
                    Item.SpawnItem(transform.position, Item.ItemType.Sword);
                } else if (randDrop >= 10 && randDrop < 30) {
                    Item.SpawnItem(transform.position, Item.ItemType.Boot);
                }

                break;
            case EnemyType.MageKnight:
                if (randDrop < 10) {
                    Item.SpawnItem(transform.position, Item.ItemType.Tome);
                } else if (randDrop >= 10 && randDrop < 30) {
                    Item.SpawnItem(transform.position, Item.ItemType.Hat);
                }

                break;
        }

        PlayerController.Instance.memories++;
        GameManager.Instance.UpdateMemories();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.simulated = false;
    }

    public void DestroyEnemy() {
        Destroy(target);
        Destroy(gameObject);
    }

    private Vector3 GetRoamingPos() {
        Vector2 point = Random.insideUnitCircle * detectionRange;
        Vector3 newPoint = point;
        newPoint.z = 0;

        newPoint += aiPath.position;

        var nearestNode = gg.GetNearestForce(newPoint, NNConstraint.Default);

        return (Vector3)nearestNode.node.position;
    }

    //IMPLEMENT ***BASIC*** A* pathfinding
    //it was not basic
}
