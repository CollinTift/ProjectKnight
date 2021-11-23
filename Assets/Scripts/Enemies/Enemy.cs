using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        switch (type) {
            default:
            case EnemyType.Slime:
                Instantiate(EnemyAssets.Instance.slimePrefab, worldPos, Quaternion.identity);
                break;
            case EnemyType.Stalker:
                Instantiate(EnemyAssets.Instance.stalkerPrefab, worldPos, Quaternion.identity);
                break;
            case EnemyType.Knight:
                Instantiate(EnemyAssets.Instance.knightPrefab, worldPos, Quaternion.identity);
                break;
            case EnemyType.KnightCaptain:
                Instantiate(EnemyAssets.Instance.knightCaptainPrefab, worldPos, Quaternion.identity);
                break;
        }
    }

    private void Awake() {
        state = State.Roaming;
        currentHealth = maxHealth;
    }

    private void Start() {
        homePos = transform.position;
        //roamPos = GetRoamingPos();
    }

    private void Update() {
        if (attackTimer < attackCD) {
            attackTimer += Time.deltaTime;
        }

        switch (state) {
            default:
            case State.Roaming:
                //get roaming position and move towards
                if (Vector2.Distance(transform.position, roamPos) < .5f) {
                    roamPos = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f)).normalized * detectionRange;
                }

                //call a* poathfinding

                transform.position = Vector2.MoveTowards(transform.position, roamPos, speed * Time.deltaTime);

                break;
            case State.Chasing:
                //if player is in detection range, go to them
                float distance = Vector2.Distance(PlayerController.Instance.GetPosition(), transform.position);

                if (distance < detectionRange) {
                    transform.position = Vector2.MoveTowards(transform.position, PlayerController.Instance.GetPosition(), speed * Time.deltaTime);
                }

                break;
            case State.Attacking:
                //play attack anim and raycast in lookDir to attackRange; deal damage if hit player
                break;
            case State.Returning:
                //move to home position
                break;
        }
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

    //IMPLEMENT ***BASIC*** A* pathfinding
}
