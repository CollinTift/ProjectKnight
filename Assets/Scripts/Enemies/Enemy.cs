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
    private Vector3 roamPos;

    private Vector2 lookdir = new Vector2(1, 0);

    public float detectionRange = 3f;

    public float attackRange = 1f;
    public float attackCD = 3f;
    public int attackDamage = 1;

    private float attackTimer = 0f;

    private AIDestinationSetter aIDestinationSetter;
    private AIPath aiPath;

    private Animator animator;

    public enum EnemyType {
        SwordKnight,
        MageKnight,
        CaptainKnight,
        ArchMage
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
        aIDestinationSetter.target = PlayerController.Instance.gameObject.transform;

        aiPath = gameObject.GetComponent<AIPath>();
        animator = gameObject.GetComponent<Animator>();
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
                //if at roam pos, delay, then get new roam pos
                break;
            case State.Chasing:
                //if player is in detection range, go to them
                break;
            case State.Attacking:
                //play attack anim and raycast in lookDir to attackRange; deal damage if hit player
                break;
            case State.Returning:
                //move to home position
                break;
        }

        animator.SetFloat("MoveX", aiPath.velocity.normalized.x);
        animator.SetFloat("MoveY", aiPath.velocity.normalized.y);
        animator.SetFloat("Speed", aiPath.velocity.magnitude);

        Debug.Log(aiPath.velocity + gameObject.ToString());
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
        return new Vector3(Random.Range(detectionRange / 2, detectionRange), Random.Range(detectionRange / 2, detectionRange), 0);
    }

    //IMPLEMENT ***BASIC*** A* pathfinding
    //it was not basic
}
