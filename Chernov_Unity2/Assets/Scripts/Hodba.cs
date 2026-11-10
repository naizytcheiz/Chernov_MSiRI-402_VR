using UnityEngine;
using Valve.VR.InteractionSystem;

public class Hodba : MonoBehaviour
{
    public float skorost = 2f;
    public float skorostBega = 4f;
    public float vysota = 1.6f;
    public float granica = 4.7f;
    public float radius = 0.25f;
    private Vector3 startMysh;
    private Vector3 startUgol;

    void Start()
    {
        FallbackCameraController polet = GetComponent<FallbackCameraController>();
        if (polet != null) polet.enabled = false;

        Vector3 p = transform.position;
        p.y = vysota;
        transform.position = p;
    }

    void Update()
    {
        // ходьба по полу: WASD или стрелки, Shift - бег
        Vector3 vpered = transform.forward;
        vpered.y = 0f;
        vpered.Normalize();
        Vector3 vpravo = transform.right;
        vpravo.y = 0f;
        vpravo.Normalize();

        float s = Input.GetKey(KeyCode.LeftShift) ? skorostBega : skorost;
        Vector3 dvizhenie = (vpered * Input.GetAxisRaw("Vertical") + vpravo * Input.GetAxisRaw("Horizontal")).normalized * s * Time.deltaTime;

        if (dvizhenie != Vector3.zero)
        {
            Vector3 niz = new Vector3(transform.position.x, 0.3f, transform.position.z);
            Vector3 verh = new Vector3(transform.position.x, vysota, transform.position.z);
            bool stena = false;
            foreach (RaycastHit hit in Physics.CapsuleCastAll(niz, verh, radius, dvizhenie.normalized, dvizhenie.magnitude + 0.05f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.rigidbody == null) stena = true;   // заготовку в руке не считаем
            }
            if (!stena)
            {
                Vector3 p = transform.position + dvizhenie;
                p.x = Mathf.Clamp(p.x, -granica, granica);
                p.z = Mathf.Clamp(p.z, -granica, granica);
                p.y = vysota;
                transform.position = p;
            }
        }

        // обзор: зажать правую кнопку мыши
        if (Input.GetMouseButtonDown(1))
        {
            startMysh = Input.mousePosition;
            startUgol = transform.localEulerAngles;
        }
        if (Input.GetMouseButton(1))
        {
            Vector3 sdvig = Input.mousePosition - startMysh;
            transform.localEulerAngles = startUgol + new Vector3(-sdvig.y * 360f / Screen.height, sdvig.x * 360f / Screen.width, 0f);
        }
    }
}
