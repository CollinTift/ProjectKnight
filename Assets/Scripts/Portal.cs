using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {
    public static void SpawnPortal(Vector3 worldPos) {
        Instantiate(PropAssets.Instance.portalPF, worldPos, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            GameManager.Instance.NextFloor();
        }
    }
}
