using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

// Без шлема кнопки этой панели нажимаются мышкой.
// Когда шлем подключён, скрипт ничего не делает: кнопки нажимаются рукой (UIElement).
public class MouseClickButtons : MonoBehaviour
{
    private void Update()
    {
        if (SteamVR.initializedState == SteamVR.InitializedStates.InitializeSuccess) return;
        if (!Input.GetMouseButtonDown(0) || Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10f))
        {
            Button button = hit.collider.GetComponent<Button>();
            if (button != null && button.transform.IsChildOf(transform) && button.gameObject.activeInHierarchy)
                button.onClick.Invoke();
        }
    }
}
