using System;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class CncJsonController : MonoBehaviour
{
    public string url = "https://cnc.kovalev.team/get/3";
    public string commandUrl = "";
    public int channel = 1;
    public float interval = 0.5f;
    public Transform nodeX;
    public Transform nodeZ;
    public MachinePanel panel;
    public bool vyvod = false;   // true - координаты получаются и выводятся
    private Vector3 startX;
    private Vector3 startZ;
    private bool netSvyazi = false;

    private IEnumerator Start()
    {
        if (nodeX == null || nodeZ == null)
        {
            Debug.LogError("Назначьте узлы X и Z станка в Inspector.");
            yield break;
        }
        startX = nodeX.localPosition;
        startZ = nodeZ.localPosition;
        if (panel != null) panel.ShowMessage("Нажмите «Пуск», чтобы получать координаты");

        while (true)
        {
            if (!vyvod)
            {
                yield return null;
                continue;
            }

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
                    }
                }
                else
                {
                    Debug.LogWarning("Ошибка запроса: " + request.error);
                    if (panel != null) panel.ShowNoConnection(request.error);
                    netSvyazi = true;
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

    // состояние канала: «Работа», «Готов», «Останов», «Ошибка»
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
        float z = GetCoordinate(data, "Z") * 0.001f;
        nodeX.localPosition = startX + Vector3.forward * x;
        nodeZ.localPosition = startZ + Vector3.left * z;

        if (panel != null)
        {
            MachineState state = new MachineState();
            state.running = GetStatus(data) == "Работа";
            state.x = GetCoordinate(data, "X");
            state.y = GetCoordinate(data, "Y");
            state.z = GetCoordinate(data, "Z");
            Debug.Log($"X={state.x}, Y={state.y}, Z={state.z}");
            panel.ShowState(state);
            if (netSvyazi)
            {
                panel.ShowMessage("Связь восстановлена");
                netSvyazi = false;
            }
        }
    }

    // Пуск: начать получать и выводить координаты
    public void StartMachine()
    {
        vyvod = true;
        panel.ShowMessage("Вывод координат запущен");
        if (!string.IsNullOrEmpty(commandUrl)) StartCoroutine(SendCommand("start"));
    }

    // Стоп: остановить вывод, координаты остаются последними
    public void StopMachine()
    {
        vyvod = false;
        panel.ShowMessage("Вывод координат остановлен");
        if (!string.IsNullOrEmpty(commandUrl)) StartCoroutine(SendCommand("stop"));
    }

    // команда на сервер, если в Inspector задан адрес commandUrl
    private IEnumerator SendCommand(string command)
    {
        string body = "{\"command\":\"" + command + "\"}";
        using (UnityWebRequest req = UnityWebRequest.Post(commandUrl, body, "application/json"))
        {
            req.timeout = 10;
            panel.ShowMessage("Отправка команды…");
            Debug.Log("POST " + commandUrl + " " + body);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                panel.ShowMessage("Ошибка команды: " + req.error);
                yield break;
            }
            panel.ShowMessage("Команда принята. Ожидание состояния…");
        }
    }
}
