using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public string content;
    public string header;

    public void OnPointerEnter(PointerEventData eventData) {
        Invoke("DelayTooltip", 0.5f);
    }

    private void DelayTooltip() {
        Tooltip.Show(content, header);
    }

    public void OnPointerExit(PointerEventData eventData) {
        CancelInvoke("DelayTooltip");
        Tooltip.Hide();
    }
}
