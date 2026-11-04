using UnityEngine;

public class Kamera : MonoBehaviour
{
    // поля - состояние
    public float Speed = 5;
    public float MouseSpeed = 3;

    void Start()
    {

    }

    void Update()
    {
        float s = Speed;
        if(Input.GetKey(KeyCode.LeftShift)){
            s = Speed * 3;          // с Shift летим быстрее
        }

        // движение - клавиши W A S D, E вверх, Q вниз
        if(Input.GetKey(KeyCode.W)){
            transform.position += transform.forward * s * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.S)){
            transform.position -= transform.forward * s * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.D)){
            transform.position += transform.right * s * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.A)){
            transform.position -= transform.right * s * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.E)){
            transform.position += Vector3.up * s * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.Q)){
            transform.position -= Vector3.up * s * Time.deltaTime;
        }

        // поворот - зажатая правая кнопка мыши
        if(Input.GetMouseButton(1)){
            float x = Input.GetAxis("Mouse X") * MouseSpeed;
            float y = Input.GetAxis("Mouse Y") * MouseSpeed;
            transform.eulerAngles += new Vector3(-y, x, 0);
        }
    }
}
