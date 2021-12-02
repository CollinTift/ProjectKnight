using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode()]
public class Tooltip : MonoBehaviour {
    public TextMeshProUGUI header;
    public TextMeshProUGUI content;

    public LayoutElement layoutEl;

    public int characterWraplimit;

    private static Tooltip current;

    public RectTransform rectTransform;

    public void Awake() {
        current = this;
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update() {
        Vector2 pos = Input.mousePosition;

        float pivotX = pos.x / Screen.width;
        float pivotY = pos.y / Screen.height;

        rectTransform.pivot = new Vector2(pivotX, pivotY);
        transform.position = pos;
    }

    public void SetText(string content, string header = "") {
        if (string.IsNullOrEmpty(header)) {
            this.header.gameObject.SetActive(false);
        } else {
            this.header.gameObject.SetActive(true);
            this.header.text = header;
        }

        this.content.text = content;

        int headerLength = this.header.text.Length;
        int contentLength = this.content.text.Length;

        layoutEl.enabled = (headerLength > characterWraplimit || contentLength > characterWraplimit) ? true : false;
    }

    public static void Show(string content, string header = "") {
        current.gameObject.SetActive(true);
        current.SetText(content, header);
    }

    public static void Hide() {
        current.gameObject.SetActive(false);
    }
}
