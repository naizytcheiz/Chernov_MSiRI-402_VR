using UnityEngine;
using Valve.VR.InteractionSystem;

public class Dver : MonoBehaviour
{
    public Vector3 vynos = new Vector3(0f, 0f, 0.003f);
    public Vector3 sdvig = new Vector3(-0.6f, 0f, 0f);
    private Vector3 nachalo;
    private LinearMapping mapping;

    void Start()
    {
        nachalo = transform.localPosition;
        mapping = GetComponent<LinearMapping>();
    }

    void Update()
    {
        if (mapping == null) return;
        transform.localPosition = nachalo + Smeshenie(mapping.value);
    }

    Vector3 Smeshenie(float t)
    {
        if (t < 0.15f) return vynos * (t / 0.15f);
        return vynos + sdvig * ((t - 0.15f) / 0.85f);
    }
}
