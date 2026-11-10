using UnityEngine;

public class Car : MonoBehaviour
{
    // поля - состояние
    public string Brand = "Rolls-Royce";
    public string Model = "Phantom";
    public int Speed = 0;
    public int Fuel = 40;

    public int GetDistance(){
        return Fuel*10;
    }
    public string GetName(){
        return Brand+" "+Model;
    }

    // методы - поведение
    public void Drive(int addSpeed)
    {
        if (Fuel >= 5)
        {
            Speed += addSpeed;      // работает с полем своего объекта
            Fuel -= 5;
        }
    }

    public void Refuel(int liters)
    {
        if (liters > 0)
            Fuel += liters;
    }

    public void Stop()
    {
        Speed = 0;
    }

    void Start()
    {
        print("Информация об автомобиле:");
        print("Марка: "+Brand);
        print("Модель: "+Model);
        print("Скорость: "+Speed+" км/ч");
        print("Бензин: "+Fuel+" л");
        print("Запас хода: "+GetDistance()+" км");
    }
    void Update()
    {

    }
}
