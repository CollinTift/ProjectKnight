using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.Tilemaps;

public class Enemy : MonoBehaviour {
    [Header("Health")]
    [SerializeField] private int maxHealth;
    private int currentHealth;

    public EnemyType type;
    private State state;
    
    private Vector3 homePos;

    private Vector2 lookdir = new Vector2(1, 0);

    public float detectionRange = 3f;

    public float attackRange = 1f;
    public float attackCD = 3f;
    public int attackDamage = 1;

    private float surveyTimer = 0f;
    private float attackTimer = 0f;

    private AIDestinationSetter aIDestinationSetter;
    private AIPath aiPath;

    private Animator animator;
    private GameObject target;
    private GridGraph gg;

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

    public static void SpawnEnemy(Vector3 worldPos, EnemyType type) {
        switch (type) {
            default:
            case EnemyType.SwordKnight:
                Instantiate(EnemyAssets.Instance.swordKnightPF, worldPos, Quaternion.identity);
                break;
            case EnemyType.MageKnight:
                Instantiate(EnemyAssets.Instance.mageKnightPF, worldPos, Quaternion.identity);
                break;
            case EnemyType.CaptainKnight:
                Instantiate(EnemyAssets.Instance.captainKnightPF, worldPos, Quaternion.identity);
                break;
            case EnemyType.ArchMage:
                Instantiate(EnemyAssets.Instance.archMagePF, worldPos, Quaternion.identity);
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
        target = new GameObject();
    }

    private void Start() {
        homePos = transform.position;

        SetTarget(GetRoamingPos());
        aIDestinationSetter.target = target.transform;
    }

    private void Update() {

        if (attackTimer < attackCD) {
            attackTimer += Time.deltaTime;
        }

        switch (state) {
            default:
            case State.Roaming:
                //if at roam pos, delay, then get new roam pos

                if (Vector2.Distance(transform.position, target.transform.position) < .5f) {
                    state = State.Surveying;
                }

                if (Vector2.Distance(transform.position, PlayerController.Instance.GetPosition()) < detectionRange) {
                    state = State.Chasing;
                }

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
                //if player is in detection range, go to them
                if (Vector2.Distance(transform.position, PlayerController.Instance.GetPosition()) < detectionRange) {
                    SetTarget(PlayerController.Instance.GetPosition());
                    if (Vector2.Distance(transform.position, PlayerController.Instance.GetPosition()) < attackRange) {
                        animator.SetTrigger("Attacking");
                        state = State.Attacking;
                    }
                } else {
                    state = State.Returning;
                }
                break;
            case State.Attacking:
                //play attack anim and raycast in lookDir to attackRange; deal damage if hit player
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attacking")) {
                    state = State.Chasing;
                }

                Debug.Log("Attacking");

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

        animator.SetFloat("MoveX", aiPath.velocity.normalized.x);
        animator.SetFloat("MoveY", aiPath.velocity.normalized.y);
        animator.SetFloat("Speed", aiPath.velocity.magnitude);

        //Debug.Log(aiPath.velocity + gameObject.ToString());
    }

    private void SetTarget(Vector3 targetPos) {
        target.transform.position = targetPos;
    }

    public void Damage(int damage) {
        currentHealth -= damage;

        //apply damaged animation (change color as well)

        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        //spawn items

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
