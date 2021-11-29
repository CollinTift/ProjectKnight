using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour {
    [Header("Health")]
    [SerializeField] private int maxHealth;
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

    private Animator animator;
    private GameObject target;
    private GridGraph gg;

    private bool canSpawnProj = true; //must be done bc called in animator, is scuffed

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

    public static void SpawnEnemy(Vector3 worldPos, EnemyType type, Transform parent) {
        switch (type) {
            default:
            case EnemyType.SwordKnight:
                Instantiate(EnemyAssets.Instance.swordKnightPF, worldPos, Quaternion.identity, parent);
                break;
            case EnemyType.MageKnight:
                Instantiate(EnemyAssets.Instance.mageKnightPF, worldPos, Quaternion.identity, parent);
                break;
            case EnemyType.CaptainKnight:
                Instantiate(EnemyAssets.Instance.captainKnightPF, worldPos, Quaternion.identity, parent);
                break;
            case EnemyType.ArchMage:
                Instantiate(EnemyAssets.Instance.archMagePF, worldPos, Quaternion.identity, parent);
                break;
        }
    }

    private void Awake() {
        state = State.Roaming;
        currentHealth = maxHealth;

        //for now
        aIDestinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        gg = AstarPath.active.data.gridGraph;

        aiPath = gameObject.GetComponent<AIPath>();
        animator = gameObject.GetComponent<Animator>();
        target = new GameObject("target");
    }

    private void Start() {
        homePos = transform.position;

        SetTarget(GetRoamingPos());
        aIDestinationSetter.target = target.transform;
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

        //Debug.Log(aiPath.velocity + gameObject.ToString());
    }

    public void AllowMovement() {
        aiPath.canMove = true;
    }

    public void DenyMovement() {
        aiPath.canMove = false;
    }

    public void SpawnMageProj() {
        if (projectile != null && canSpawnProj) {
            GameObject go = Instantiate(projectile, transform.position, Quaternion.identity);
            Projectile proj = go.GetComponent<Projectile>();
            proj.Launch(PlayerController.Instance.GetPosition() - transform.position, attackDamage, 0.5f);

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
                    Damage(PlayerController.Instance.attackDamage);
                }

                iFrameTimer = 0f;
            }
        }
    }

    public void SetShielding(int even) {
        if (even % 2 == 0) {
            shielding = true;
        } else {
            shielding = false;
        }
    }

    public void Damage(int damage) {
        currentHealth -= damage;

        animator.SetTrigger("Damage");

        if (currentHealth <= 0) Die();
    }

    private void Die() {
        //chance to spawn items
        animator.SetTrigger("Die");
        GetComponent<Rigidbody2D>().isKinematic = false;
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
