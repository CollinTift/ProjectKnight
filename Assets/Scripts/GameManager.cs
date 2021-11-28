using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public GameObject gameOver;

    [Header("Editable fields")]
    public float timeBetweenWaves = 10f; //time in seconds between each wave
    public float difficultyScale = .05f; //percent of difficulty increase
    public int baseEnemiesPerWave = 5; //enemies per wave at wave 0, increases based on difficulty

    /* 
     * function is: y = baseEnemiesPerWave * (1 + difficultyScale)^(currentWave) 
     * https://www.desmos.com/calculator
     */

    [Header("Do NOT edit")]
    public float difficulty = 1f;
    public int currentWave = 0;
    public int enemiesThisWave = 0;

    private float currentWaveTimer = 0f;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            DontDestroyOnLoad(gameObject);

            Instance = this;
        }

        gameOver = GameObject.FindWithTag("GameOverUI");
        gameOver.SetActive(false);
    }

    private void Update() {
        currentWaveTimer += Time.deltaTime;

        if (currentWaveTimer >= timeBetweenWaves) {
            currentWave++;

            difficulty *= (difficultyScale + 1f);
            enemiesThisWave += Mathf.FloorToInt(baseEnemiesPerWave * difficulty);

            currentWaveTimer = 0f;
        }
    }

    public void enableGameOver() {
        gameOver.SetActive(true);
        Time.timeScale = 0f;
    }
}
