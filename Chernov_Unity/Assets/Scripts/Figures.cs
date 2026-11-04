using UnityEngine;

public class Figures : MonoBehaviour
{
    // поля - состояние
    public string[] Names = { "Куб", "Сфера", "Цилиндр" };
    public int[] Counts = { 3, 2, 4 };

    void Start()
    {
        for(int i = 0; i < Names.Length; i++){
            // находим фигуру-образец на сцене по названию из массива
            GameObject figure = GameObject.Find(Names[i]);
            print("Создаём " + Names[i] + ": " + Counts[i] + " шт.");

            // создаём столько копий, сколько указано в массиве
            for(int j = 1; j <= Counts[i]; j++){
                Vector3 position = figure.transform.position + new Vector3(0, 0, j * 1.5f);
                GameObject c = Instantiate(figure, position, figure.transform.rotation);
                c.name = Names[i] + " №" + j;
            }

            // прячем образец, чтобы на сцене было ровно столько фигур, сколько в массиве
            figure.SetActive(false);
        }
    }
    void Update()
    {

    }
}
