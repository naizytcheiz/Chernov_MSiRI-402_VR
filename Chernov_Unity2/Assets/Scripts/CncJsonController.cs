using System;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

// Клиент сервера станка (занятие 09.11): по GET-запросу получает JSON, находит координаты осей X, Y, Z, двигает узлы станка и передаёт данные на панель.
public class CncJsonController : MonoBehaviour
{
    public string url = "https://cnc.kovalev.team/get/3";
    public int channel = 1;
    public float interval = 0.5f;
    public Transform nodeX;
    public Transform nodeZ;
    public MachinePanel panel;
    public bool applyMovement = true;
    private Vector3 startX;
    private Vector3 startZ;

    private IEnumerator Start()
    {
        if (nodeX == null || nodeZ == null)
        {
            Debug.LogError("Назначьте узлы X и Z станка в Inspector.");
            yield break;
        }
        startX = nodeX.localPosition;
        startZ = nodeZ.localPosition;

        while (true)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = 10;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        MoveNodes(request.downloadHandler.text);
                    }
                    catch (Exception error)
                    {
                        Debug.LogWarning("Ошибка чтения JSON: " + error.Message);
                        if (panel != null)
                            panel.ShowReadError("Ошибка формата данных");
                    }
                }
                else
                {
                    Debug.LogWarning("Ошибка запроса: " + request.error);
                    if (panel != null)
                        panel.ShowReadError("Нет связи с API");
                }
            }
            yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, interval));
        }
    }

    private float GetCoordinate(JObject data, string axis)
    {
        foreach (JToken section in data["data"])
        {
            if ((string)section[0] != "Оси Канала " + channel)
                continue;

            foreach (JToken row in section[1])
            {
                if ((string)row[0] == "Перемещение по оси " + axis)
                {
                    string text = ((string)row[1]).Replace("мм", "").Trim();
                    text = text.Replace(',', '.');
                    float value = float.Parse(text, CultureInfo.InvariantCulture);
                    if (float.IsNaN(value) || float.IsInfinity(value))
                        throw new FormatException("Некорректная координата " + axis);
                    return value;
                }
            }
        }
        throw new FormatException("Не найдена координата " + axis);
    }

    // раздел «Канал 1», строка «Статус канала»
    private string GetStatus(JObject data)
    {
        foreach (JToken section in data["data"])
        {
            if ((string)section[0] != "Канал " + channel)
                continue;

            foreach (JToken row in section[1])
            {
                if ((string)row[0] == "Статус канала")
                    return (string)row[1];
            }
        }
        return "";
    }

    private void MoveNodes(string json)
    {
        JObject data = JObject.Parse(json);
        // токарный станок: X - поперечные салазки к детали, Z - каретка вдоль станка
        float x = GetCoordinate(data, "X") * 0.001f;
        float y = GetCoordinate(data, "Y") * 0.001f;
        float z = GetCoordinate(data, "Z") * 0.001f;
        if (panel != null)
        {
            panel.ShowCoordinatesMeters(x, y, z, channel);
            string status = GetStatus(data);
            if (status != "") panel.ShowStatus(status);
        }
        if (!applyMovement) return;
        nodeX.localPosition = startX + Vector3.forward * x;
        nodeZ.localPosition = startZ + Vector3.left * z;
    }
}
