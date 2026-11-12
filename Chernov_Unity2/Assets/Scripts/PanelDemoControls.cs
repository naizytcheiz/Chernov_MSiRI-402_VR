using UnityEngine;

// Кнопки Пуск и Стоп панели станка (занятие 11.11): меняют состояние на панели и пишут команду.
public class PanelDemoControls : MonoBehaviour
{
    public MachinePanel panel;

    private void Start()
    {
        panel.ShowDemoState(false);
        panel.ShowCommand("Нажмите «Пуск»");
    }

    public void StartMachine()
    {
        Debug.Log("Пуск");
        panel.ShowDemoState(true);
        panel.ShowCommand("Станок запущен");
    }

    public void StopMachine()
    {
        Debug.Log("Стоп");
        panel.ShowDemoState(false);
        panel.ShowCommand("Станок остановлен");
    }
}
