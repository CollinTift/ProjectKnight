using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAssets : MonoBehaviour {
    public static EnemyAssets Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public Transform slimePrefab;
    public Transform stalkerPrefab;
    public Transform knightPrefab;
    public Transform knightCaptainPrefab;
}
