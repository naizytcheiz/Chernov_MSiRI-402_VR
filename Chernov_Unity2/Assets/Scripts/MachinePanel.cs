using TMPro;
using UnityEngine;

public class MachinePanel : MonoBehaviour
{
    public TMP_Text stateText, axesText, messageText;

    public void ShowState(MachineState data)
    {
        stateText.text = data.running ? "Работает" : "Остановлен";
        axesText.text = $"X: {data.x:F1} мм\n" +
            $"Y: {data.y:F1} мм\nZ: {data.z:F1} мм";
        axesText.alpha = 1f;
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
    }

    // нет связи: последние координаты неактуальны
    public void ShowNoConnection(string error)
    {
        messageText.text = "Нет связи: " + error;
        axesText.alpha = 0.4f;
    }
}
