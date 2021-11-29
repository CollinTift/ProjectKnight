using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    private int damage;
    private Vector3 shootDir;
    private float speed;

    private void Update() {
        transform.position += shootDir * Time.deltaTime * speed;
    }

    public void Launch(Vector3 shootDir, int damage, float speed) {
        this.shootDir = shootDir;
        this.damage = damage;
        this.speed = speed;

        transform.forward = Vector2.up;
        transform.rotation = Quaternion.FromToRotation(transform.position, shootDir);

        Destroy(gameObject, 10f);
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.collider.gameObject.layer == 3) PlayerController.Instance.Damage(damage);

        if (other.collider.gameObject.layer != 10) Destroy(gameObject);
    }
}
