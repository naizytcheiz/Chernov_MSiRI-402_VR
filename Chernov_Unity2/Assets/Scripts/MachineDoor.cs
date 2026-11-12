using UnityEngine;
using UnityEngine.Serialization;
using Valve.VR.InteractionSystem;

// Раздвижная дверь станка. Рука тянет ручку через LinearDrive, тот пишет положение в LinearMapping (0..1).
// Скрипт переводит это число в движение двери: сначала дверь чуть выходит из проёма, потом едет в сторону.
public class MachineDoor : MonoBehaviour
{
    [FormerlySerializedAs("vynos")] public Vector3 pullOut = new Vector3(0f, 0f, 0.003f);   // выход из проёма на первых 15 % хода
    [FormerlySerializedAs("sdvig")] public Vector3 slide = new Vector3(-0.6f, 0f, 0f);      // сдвиг в сторону на остальном ходе
    private Vector3 closedPosition;
    private LinearMapping mapping;

    void Start()
    {
        closedPosition = transform.localPosition;
        mapping = GetComponent<LinearMapping>();
    }

    void Update()
    {
        if (mapping == null) return;
        transform.localPosition = closedPosition + Offset(mapping.value);
    }

    // смещение двери для положения t от 0 (закрыта) до 1 (открыта)
    Vector3 Offset(float t)
    {
        if (t < 0.15f) return pullOut * (t / 0.15f);
        return pullOut + slide * ((t - 0.15f) / 0.85f);
    }
}
