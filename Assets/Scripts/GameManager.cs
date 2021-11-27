using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public GameObject gameOver;

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

    public void enableGameOver() {
        gameOver.SetActive(true);
        Time.timeScale = 0f;
    }
}
