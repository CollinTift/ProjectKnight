using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    Rigidbody2D rb;
    private int damage;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float force, int damage) {
        rb.AddForce(direction * force);
        this.damage = damage;
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.collider.gameObject.layer == 3) PlayerController.Instance.Damage(damage);
        Destroy(gameObject);
    }
}
