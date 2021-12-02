using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public GameObject gameOver;
    public GameObject nextFloor;
    public GameObject memoryScreen;
    public GameObject winScreen;

    public TextMeshProUGUI memoryCounter;
    public TextMeshProUGUI memoryCounterMemScreen;

    public TextMeshProUGUI waveCounter;

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
        memoryScreen = GameObject.FindWithTag("MemoryScreen");
        gameOver.SetActive(false);
        nextFloor.SetActive(false);
        memoryScreen.SetActive(false);
        winScreen.SetActive(false);

        waveCounter.SetText("Current wave: " + currentWave);

        StartCoroutine("ShowNextFloorUI");
    }

    private void Update() {
        currentWaveTimer += Time.deltaTime;

        if (currentWaveTimer >= timeBetweenWaves) {
            currentWave++;
            waveCounter.SetText("Current wave: " + currentWave);

            difficulty *= (difficultyScale + 1f);
            enemiesThisWave += Mathf.FloorToInt(baseEnemiesPerWave * difficulty);

            currentWaveTimer = 0f;
        }
    }

    public void NextFloor() {
        memoryScreen.SetActive(true);

        Time.timeScale = 0f;

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

    public void MemMaxHP() {
        if (PlayerController.Instance.memories >= 3) {
            PlayerController.Instance.memories -= 3;
            UpdateMemories();
            PlayerController.Instance.maxHealth++;
        }
    }

    public void MemMaxChaos() {
        if (PlayerController.Instance.memories >= 3) {
            PlayerController.Instance.memories -= 3;
            UpdateMemories();
            PlayerController.Instance.maxChaos++;
        }
    }

    public void MemDamage() {
        if (PlayerController.Instance.memories >= 5) {
            PlayerController.Instance.memories -= 5;
            UpdateMemories();
            PlayerController.Instance.attackDamage++;
        }
    }

    public void MemSpeed() {
        if (PlayerController.Instance.memories >= 2) {
            PlayerController.Instance.memories -= 2;
            UpdateMemories();
            PlayerController.Instance.speed += .02f;
        }
    }

    public void MemCritChance() {
        if (PlayerController.Instance.memories >= 1) {
            PlayerController.Instance.memories -= 1;
            UpdateMemories();
            PlayerController.Instance.critChance += 1;
        }
    }

    public void MemCritDamage() {
        if (PlayerController.Instance.memories >= 1) {
            PlayerController.Instance.memories -= 1;
            UpdateMemories();
            PlayerController.Instance.critMult += .02f;
        }
    }

    public void MemThorns() {
        if (PlayerController.Instance.memories >= 1) {
            PlayerController.Instance.memories -= 1;
            UpdateMemories();
            PlayerController.Instance.thornsDamage++;
        }
    }

    public void MemHealPerWave() {
        if (PlayerController.Instance.memories >= 2) {
            PlayerController.Instance.memories -= 2;
            UpdateMemories();
            PlayerController.Instance.healPerWave++;
        }
    }

    public void MemMaxShield() {
        if (PlayerController.Instance.memories >= 3) {
            PlayerController.Instance.memories -= 3;
            UpdateMemories();
            PlayerController.Instance.maxShield++;
        }
    }

    public void MemDashSpeed() {
        if (PlayerController.Instance.memories >= 3) {
            PlayerController.Instance.memories -= 3;
            UpdateMemories();
            PlayerController.Instance.dashSpeedMultiplier += .1f;
        }
    }

    public void MemReturnHome() {
        if (PlayerController.Instance.memories >= 250) {
            PlayerController.Instance.memories -= 250;
            UpdateMemories();

            winScreen.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void MemFinishUpgrade() {
        Time.timeScale = 1f;
        memoryScreen.SetActive(false);

        StartCoroutine("ShowNextFloorUI");
    }

    public void UpdateMemories() {
        memoryCounter.SetText("Current Memories: " + PlayerController.Instance.memories);
        memoryCounterMemScreen.SetText("Current Memories: " + PlayerController.Instance.memories);
    }
}
