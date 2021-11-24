using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAssets : MonoBehaviour {
    public static EnemyAssets Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public Transform swordKnightPF;
    public Transform mageKnightPF;
    public Transform captainKnightPF;
    public Transform archMagePF;
}
