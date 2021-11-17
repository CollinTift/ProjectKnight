using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float speed;
    //attack speed

    private Rigidbody2D rb;

    private float hor;
    private float ver;

    private Vector2 lookDir;

    public static PlayerController Instance { get; private set; }

    void Awake() {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();

        lookDir = new Vector2(1, 0);
    }

    void Update() {
        Move();
    }

    void FixedUpdate() {
        Vector2 pos = rb.position;

        pos.x = pos.x + speed * hor * Time.deltaTime;
        pos.y = pos.y + speed * ver * Time.deltaTime;

        rb.MovePosition(pos);
    }

    private void Move() {
        hor = Input.GetAxis("Horizontal");
        ver = Input.GetAxis("Vertical");
        Vector2 move = new Vector2(hor, ver);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f)) {
            lookDir.Set(move.x, move.y);
            lookDir.Normalize();
        }
    }
}
