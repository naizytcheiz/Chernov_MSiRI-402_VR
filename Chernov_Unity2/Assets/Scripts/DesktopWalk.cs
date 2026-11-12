using UnityEngine;
using UnityEngine.Serialization;
using Valve.VR.InteractionSystem;

// Ходьба по полу без шлема (проверка на ПК): WASD или стрелки, Shift - бег, правая кнопка мыши - обзор.
// Висит на запасной камере Player; при подключённом шлеме этот объект выключен и не мешает VR.
public class DesktopWalk : MonoBehaviour
{
    [FormerlySerializedAs("skorost")] public float walkSpeed = 2f;      // скорость шага, м/с
    [FormerlySerializedAs("skorostBega")] public float runSpeed = 4f;   // скорость бега с Shift, м/с
    [FormerlySerializedAs("vysota")] public float eyeHeight = 1.6f;     // высота глаз над полом, м
    [FormerlySerializedAs("granica")] public float floorLimit = 4.7f;   // граница пола по X и Z, м
    public float radius = 0.25f;                                         // радиус «тела» для проверки стен
    private Vector3 mouseStart;
    private Vector3 angleStart;

    void Start()
    {
        // встроенный полёт камеры SteamVR выключаем, чтобы ходить по полу
        FallbackCameraController flying = GetComponent<FallbackCameraController>();
        if (flying != null) flying.enabled = false;

        Vector3 p = transform.position;
        p.y = eyeHeight;
        transform.position = p;
    }

    void Update()
    {
        // направления взгляда без наклона вверх-вниз
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        Vector3 move = (forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal")).normalized * speed * Time.deltaTime;

        if (move != Vector3.zero)
        {
            // капсула от колен до глаз: если впереди стена или станок - не идём
            Vector3 bottom = new Vector3(transform.position.x, 0.3f, transform.position.z);
            Vector3 top = new Vector3(transform.position.x, eyeHeight, transform.position.z);
            bool wall = false;
            foreach (RaycastHit hit in Physics.CapsuleCastAll(bottom, top, radius, move.normalized, move.magnitude + 0.05f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.rigidbody == null) wall = true;   // заготовку в руке не считаем препятствием
            }
            if (!wall)
            {
                Vector3 p = transform.position + move;
                p.x = Mathf.Clamp(p.x, -floorLimit, floorLimit);
                p.z = Mathf.Clamp(p.z, -floorLimit, floorLimit);
                p.y = eyeHeight;
                transform.position = p;
            }
        }

        // обзор: зажать правую кнопку мыши
        if (Input.GetMouseButtonDown(1))
        {
            mouseStart = Input.mousePosition;
            angleStart = transform.localEulerAngles;
        }
        if (Input.GetMouseButton(1))
        {
            Vector3 shift = Input.mousePosition - mouseStart;
            transform.localEulerAngles = angleStart + new Vector3(-shift.y * 360f / Screen.height, shift.x * 360f / Screen.width, 0f);
        }
    }
}
