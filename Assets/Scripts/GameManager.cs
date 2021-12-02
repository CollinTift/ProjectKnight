using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public GameObject gameOver;
    public GameObject nextFloor;

    [Header("Editable fields")]
    public float timeBetweenWaves = 60f; //time in seconds between each wave
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

    public int currentFloor = 1;

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
        nextFloor.SetActive(false);

        StartCoroutine("ShowNextFloorUI");
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

    public void NextFloor() {
        currentFloor++;
        timeBetweenWaves -= 1f;
        //kill all enemies, kill all props, kill all items, generate new map,  teleport player to spawn, setup everything again

        foreach (GameObject go in FindObjectsOfType(typeof(GameObject))) {
            if (go.layer == 7 || go.layer == 11 || go.layer == 12) {
                if (go.GetComponent<Enemy>() != null) {
                    go.GetComponent<Enemy>().DestroyEnemy();
                } else {
                    Destroy(go);
                }
            }
        }

        GenerateMap generateMap = FindObjectOfType<GenerateMap>();
        generateMap.SetupMap();

        StartCoroutine("ShowNextFloorUI");
    }

    private IEnumerator ShowNextFloorUI() {
        nextFloor.SetActive(true);

        TextMeshProUGUI nextFloorUI = nextFloor.GetComponentInChildren<TextMeshProUGUI>();
        nextFloorUI.SetText("Floor: " + currentFloor);

        for (float alpha = 1f; alpha >= 0; alpha-= 0.01f) {
            nextFloorUI.faceColor = new Color(nextFloorUI.faceColor.r, nextFloorUI.faceColor.g, nextFloorUI.faceColor.b, alpha);
            yield return null;
        }

        nextFloor.SetActive(false);
    }

    public void enableGameOver() {
        gameOver.SetActive(true);
        Time.timeScale = 0f;
    }
}
