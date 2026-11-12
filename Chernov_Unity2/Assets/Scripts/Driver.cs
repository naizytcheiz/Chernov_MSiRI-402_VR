using UnityEngine;

// Водитель (занятие 03.11): через ссылку car обращается к полям и методам другого объекта - автомобиля.
public class Driver : MonoBehaviour
{
    // поля - состояние
    public string Name = "Назар";
    public int Experience = 3;
    public Car car;

    public string GetName(){
        return "Водитель "+Name;
    }

    void Start()
    {
        print("Информация о водителе:");
        print(GetName());
        print("Стаж: "+Experience+" года");

        // вызов методов - через точку
        print("Обращение к автомобилю:");
        print("Автомобиль: "+car.GetName());
        print("Марка: "+car.Brand);
        print("Модель: "+car.Model);
        print("Бензин: "+car.Fuel+" л");
        print("Запас хода: "+car.GetDistance()+" км");
    }
    void Update()
    {

    }
}
