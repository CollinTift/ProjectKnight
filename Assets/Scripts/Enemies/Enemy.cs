using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    [Header("Health")]
    [SerializeField] private int maxHealth;
    private int currentHealth;

    public float speed = 1f;

    public EnemyType type;
    private State state;
    
    private Vector3 homePos;
    private Vector3 roamPos;

    private Vector2 lookdir = new Vector2(1, 0);

    private int currentPathIndex;
    private List<Vector3> pathVectorList;

    private float reachedPosDist = 1f;
    private float detectionRange = 3f;

    public float attackRange = 1f;
    public float attackCD = 3f;
    public int attackDamage = 1;

    private float attackTimer = 0f;

    public enum EnemyType {
        Slime,
        Stalker,
        Knight,
        KnightCaptain
    }

    private enum State {
        Roaming,
        Chasing,
        Attacking,
        Returning
    }

    public static void SpawnEnemy(Vector3 worldPos, EnemyType type) {
        Transform enemyTransform;

        switch (type) {
            default:
            case EnemyType.Slime:
                enemyTransform = Instantiate(EnemyAssets.Instance.slimePrefab, worldPos, Quaternion.identity);
                break;
            case EnemyType.Stalker:
                enemyTransform = Instantiate(EnemyAssets.Instance.stalkerPrefab, worldPos, Quaternion.identity);
                break;
            case EnemyType.Knight:
                enemyTransform = Instantiate(EnemyAssets.Instance.knightPrefab, worldPos, Quaternion.identity);
                break;
            case EnemyType.KnightCaptain:
                enemyTransform = Instantiate(EnemyAssets.Instance.knightCaptainPrefab, worldPos, Quaternion.identity);
                break;
        }
    }

    private void Awake() {
        state = State.Roaming;
        currentHealth = maxHealth;
    }

    private void Start() {
        homePos = transform.position;
        roamPos = GetRoamingPos();
    }

    private void Update() {
        if (attackTimer < attackCD) {
            attackTimer += Time.deltaTime;
        }

        switch (state) {
            default:
            case State.Roaming:
                MoveTo(roamPos);

                reachedPosDist = .5f;
                if (Vector3.Distance(transform.position, roamPos) < reachedPosDist) {
                    roamPos = GetRoamingPos();
                }

                FindTarget();
                break;
            case State.Chasing:
                MoveTo(PlayerController.Instance.GetPosition());

                if (Vector3.Distance(transform.position, PlayerController.Instance.GetPosition()) <= attackRange) {
                    if (attackTimer >= attackCD) {
                        StopMoving();

                        PlayerController.Instance.Damage(attackDamage);
                        attackTimer = 0f;
                    }
                }

                if (Vector3.Distance(transform.position, PlayerController.Instance.GetPosition()) > detectionRange) {
                    state = State.Returning;
                }
                break;
            case State.Attacking:
                break;
            case State.Returning:
                MoveTo(homePos);

                reachedPosDist = 1f;
                if (Vector3.Distance(transform.position, homePos) < reachedPosDist) {
                    state = State.Roaming;
                }

                if (Vector3.Distance(transform.position, PlayerController.Instance.GetPosition()) < detectionRange) {
                    state = State.Chasing;
                }

                break;
        }
    }

    public void Damage(int damage) {
        currentHealth -= damage;

        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        //spawn items

        Destroy(gameObject);
    }

    private void MoveTo(Vector3 targetPos) {
        SetTargetPosition(targetPos);
        HandleMovement();
    }

    private void HandleMovement() {
        if (pathVectorList != null) {
            Vector3 targetPos = pathVectorList[currentPathIndex];

            if (Vector3.Distance(transform.position, targetPos) > .1f) {
                Vector3 moveDir = (targetPos - transform.position).normalized;

                //float distanceBefore = Vector3.Distance(transform.position, targetPos);

                transform.position = transform.position + moveDir * speed * Time.deltaTime;
            } else {
                currentPathIndex++;
                if (currentPathIndex >= pathVectorList.Count) {
                    StopMoving();
                }
            }
        }
    }

    private void StopMoving() {
        pathVectorList = null;
    }

    private Vector3 GetRoamingPos() {
        Vector3 randDir = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0).normalized;

        return homePos + randDir * Random.Range(5f, 10f);
    }

    private void FindTarget() {
        if (Vector3.Distance(transform.position, PlayerController.Instance.GetPosition()) < detectionRange) {
            state = State.Chasing;
        }
    }

    public void SetTargetPosition(Vector3 targetPos) {
        currentPathIndex = 0;
        pathVectorList = Pathfinding.Instance.FindPath(transform.position, targetPos);

        if (pathVectorList != null && pathVectorList.Count > 1) {
            pathVectorList.RemoveAt(0);
        }
    }
}
