using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    [Header("Health")]
    [SerializeField] private int maxHealth;
    private int currentHealth;

    public EnemyType type;
    private State state;
    
    private Vector3 homePos;
    private Vector3 roamPos;

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
        switch (state) {
            default:
            case State.Roaming:
                break;
            case State.Chasing:
                break;
            case State.Attacking:
                break;
            case State.Returning:
                break;
        }
    }

    private Vector3 GetRoamingPos() {
        Vector3 randDir = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0).normalized;

        return homePos + randDir * Random.Range(5f, 10f);
    }
}
