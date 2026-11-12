using UnityEngine;

// Обработчик столкновений (занятие 04.11): пишет в консоль, с чем столкнулся объект и через какой триггер прошёл.
public class CollisionReporter : MonoBehaviour
{
    void Start()
    {

    }
    void Update()
    {

    }

    // столкновение - объекты не проходят друг через друга
    void OnCollisionEnter(Collision collision) {
        print(gameObject.name + " столкнулся с объектом " + collision.gameObject.name);
    }

    // триггер - объект проходит насквозь через зону
    private void OnTriggerEnter(Collider other)
    {
        print(gameObject.name + " прошёл через " + other.gameObject.name);
    }
}
