using System.Collections;
using TMPro;
using UnityEngine;
using Valve.VR.InteractionSystem;

// Учебный сценарий работы со станком: этапы, проверки, цикл, результат и сброс.
public class MachineTrainer : MonoBehaviour
{
    public enum Stage
    {
        Intro = 1,      // ознакомление
        Check = 2,      // исходное состояние
        Grab = 3,       // захват
        Place = 4,      // установка
        Guard = 5,      // ограждение
        Ready = 6,      // запуск
        Processing = 7, // ожидание
        Output = 8,     // готовая продукция
        Done = 9        // результат
    }

    public TrainingBlank blank;
    public Transform blankHome;
    public Transform workPoint;
    public Transform outputPoint;
    public BoxCollider workZone;
    public BoxCollider outputZone;
    public Transform movingNode;
    public TMP_Text stepText;
    public TMP_Text messageText;
    public TMP_Text resultText;
    public Renderer blankRenderer;
    public Color rawColor = new Color(0.55f, 0.55f, 0.58f);
    public Color finishedColor = new Color(0.20f, 0.75f, 0.30f);
    public float duration = 5f;
    public float travel = 0.12f;
    public bool requireGuard = true;
    public Transform guard;
    public Vector3 guardOpenOffset = new Vector3(0.5f, 0f, 0f);
    // ограждение = двери станка: их положение хранит LinearMapping (0 закрыта, 1 открыта),
    // двигать можно рукой (LinearDrive) или кнопкой «Ограждение»
    public LinearMapping[] guardDoors;
    public float spindleSpeed = 720f;   // градусов в секунду: вращение заготовки во время обработки

    public Stage CurrentStage { get; private set; }

    private Vector3 nodeHome;
    private Quaternion visualRotation;
    private Vector3 guardClosed;
    private bool guardOpen;
    private bool placed;
    private bool processed;
    private bool collected;
    private int handledRelease;
    private int errors;
    private int errStart, errPlace, errOutput, errGuard;   // ошибки по видам
    private float startTime;
    private Coroutine cycle;
    private float lastPress = -1f;

    // одно нажатие = один обработчик, даже если кнопку нажали двумя способами сразу
    private bool Pressed()
    {
        if (Time.unscaledTime - lastPress < 0.3f) return false;
        lastPress = Time.unscaledTime;
        return true;
    }

    private void Start()
    {
        if (blank == null || blankHome == null || workPoint == null || outputPoint == null ||
            workZone == null || outputZone == null || movingNode == null || stepText == null ||
            messageText == null || resultText == null || blankRenderer == null ||
            (requireGuard && guard == null && !HasDoors))
        {
            Debug.LogError("Заполните ссылки MachineTrainer");
            enabled = false;
            return;
        }

        blank.trainer = this;
        visualRotation = blankRenderer.transform.localRotation;
        nodeHome = movingNode.localPosition;
        if (guard != null) guardClosed = guard.localPosition;
        RestartTraining();
    }

    private void Update()
    {
        // отпускание обрабатывается один раз и чуть позже, когда рука закончила действие
        if (blank.ReleaseNumber != handledRelease && !blank.IsHeld &&
            Time.time - blank.ReleaseTime >= 0.15f)
        {
            handledRelease = blank.ReleaseNumber;
            HandleRelease();
        }
        if (requireGuard && HasDoors) WatchDoors();
    }

    // ---------- двери станка как ограждение ----------

    private bool HasDoors => guardDoors != null && guardDoors.Length > 0;

    // аварийная остановка: цикл прерывается, каретка в нуле, деталь остаётся необработанной
    private void StopByDoor()
    {
        if (cycle != null) { StopCoroutine(cycle); cycle = null; }
        movingNode.localPosition = nodeHome;
        blankRenderer.transform.localRotation = visualRotation;
        guardOpen = true;
        SetStage(Stage.Guard);
        Error("Станок остановлен: дверь открыта во время обработки. Закройте дверь и нажмите «Пуск» заново.", ref errGuard);
    }

    // закрыто, только если все двери почти в нуле
    private bool DoorsClosed()
    {
        foreach (LinearMapping d in guardDoors)
            if (d != null && d.value > 0.03f) return false;
        return true;
    }

    private void SetDoors(float value)
    {
        foreach (LinearMapping d in guardDoors)
            if (d != null) d.value = value;
    }

    // двери могут двигать руками, поэтому состояние читается каждый кадр
    private void WatchDoors()
    {
        bool closed = DoorsClosed();
        // дверь открыли во время обработки - станок останавливается, как блокировка двери на настоящем ЧПУ
        if (CurrentStage == Stage.Processing && !closed) { StopByDoor(); return; }
        if (guardOpen == !closed) return;
        guardOpen = !closed;
        if (CurrentStage == Stage.Guard && closed) SetStage(Stage.Ready);
        else if (CurrentStage == Stage.Ready && !closed) SetStage(Stage.Guard);
        else if (CurrentStage == Stage.Output) Message(Hint(Stage.Output));
    }

    // ---------- кнопки панели ----------

    public void NextStep()
    {
        if (!Pressed()) return;
        if (CurrentStage == Stage.Intro)
        {
            SetStage(Stage.Check);
        }
        else if (CurrentStage == Stage.Check)
        {
            if (blank.IsHeld) { Message("Сначала положите заготовку на стол."); return; }
            if (Vector3.Distance(blank.transform.position, blankHome.position) > 0.3f) { Message("Заготовка должна лежать на исходном столе."); return; }
            if (InZone(workZone, blank.transform.position)) { Message("Рабочая зона должна быть свободна."); return; }
            if (movingNode.localPosition != nodeHome) { Message("Узел станка должен быть в исходном положении."); return; }
            if (requireGuard && !guardOpen) { Message("Откройте ограждение (двери станка), чтобы установить заготовку."); return; }
            SetStage(Stage.Grab);
        }
        else
        {
            Message("Кнопка «Далее» нужна только в начале. Выполните действие: " + Hint(CurrentStage));
        }
    }

    public void ToggleGuard()
    {
        if (!Pressed()) return;
        if (!requireGuard) { Message("Ограждение в этом станке не используется."); return; }
        if (CurrentStage == Stage.Processing) { SetGuard(true); StopByDoor(); return; }

        SetGuard(!guardOpen);
        if (CurrentStage == Stage.Guard && !guardOpen) SetStage(Stage.Ready);
        else if (CurrentStage == Stage.Ready && guardOpen) SetStage(Stage.Guard);
        else if (CurrentStage == Stage.Output) Message(Hint(Stage.Output));
        else Message(guardOpen ? "Ограждение открыто." : "Ограждение закрыто.");
    }

    public void StartCycle()
    {
        if (!Pressed()) return;
        if (CurrentStage == Stage.Processing) { Error("Цикл уже идёт. Дождитесь окончания.", ref errStart); return; }
        if (requireGuard && placed && guardOpen) { Error("Пуск невозможен: закройте ограждение (двери станка).", ref errStart); return; }
        if (CurrentStage != Stage.Ready || !placed || blank.IsHeld || !InZone(workZone, blank.transform.position))
        {
            Error("Пуск невозможен: сначала установите заготовку в патрон.", ref errStart);
            return;
        }
        if (requireGuard && guardOpen) { Error("Пуск невозможен: закройте ограждение.", ref errStart); return; }

        SetStage(Stage.Processing);
        cycle = StartCoroutine(ProcessCycle());
    }

    public void StopCycle()
    {
        if (!Pressed()) return;
        if (CurrentStage != Stage.Processing) { Message("Цикл не запущен."); return; }

        StopCoroutine(cycle);
        cycle = null;
        movingNode.localPosition = nodeHome;
        blankRenderer.transform.localRotation = visualRotation;
        SetStage(Stage.Ready);
        Message("Цикл остановлен. Нажмите «Пуск», чтобы начать заново.");
    }

    public void RestartTraining()
    {
        if (!Pressed()) return;
        if (blank.IsHeld) { Message("Сначала отпустите заготовку."); return; }

        if (cycle != null) { StopCoroutine(cycle); cycle = null; }
        movingNode.localPosition = nodeHome;
        blankRenderer.transform.localRotation = visualRotation;
        placed = false;
        processed = false;
        collected = false;
        errors = 0;
        errStart = 0; errPlace = 0; errOutput = 0; errGuard = 0;
        blank.ResetTo(blankHome);
        handledRelease = blank.ReleaseNumber;
        blankRenderer.material.color = rawColor;
        if (guard != null || HasDoors) SetGuard(true);   // в начале сценария ограждение открыто
        startTime = Time.time;
        resultText.text = "";
        SetStage(Stage.Intro);
    }

    // ---------- вызовы от заготовки ----------

    public bool CanGrab()
    {
        if (CurrentStage == Stage.Grab || CurrentStage == Stage.Place) return true;
        if (CurrentStage == Stage.Output) return !requireGuard || guardOpen;
        return false;
    }

    public void NotifyGrab()
    {
        if (CurrentStage == Stage.Grab)
        {
            SetStage(Stage.Place);
        }
        else if (CurrentStage == Stage.Output)
        {
            collected = true;
            Message("Перенесите деталь на стол готовой продукции и отпустите.");
        }
    }

    private void HandleRelease()
    {
        Vector3 p = blank.transform.position;

        if (CurrentStage == Stage.Place)
        {
            if (InZone(workZone, p))
            {
                blank.Snap(workPoint);
                placed = true;
                SetStage(requireGuard && guardOpen ? Stage.Guard : Stage.Ready);
            }
            else
            {
                Error("Заготовка не в рабочей зоне. Возьмите её и установите в патрон.", ref errPlace);
            }
        }
        else if (CurrentStage == Stage.Output && collected)
        {
            if (processed && InZone(outputZone, p))
            {
                blank.Snap(outputPoint);
                SetStage(Stage.Done);
                float time = Time.time - startTime;
                resultText.text = $"Обучение пройдено. Время: {time:F0} с. Ошибок: {errors}" + ErrorDetails();
            }
            else
            {
                Error("Деталь не на столе готовой продукции. Возьмите её и перенесите ещё раз.", ref errOutput);
            }
        }
    }

    // ---------- обработка ----------

    private IEnumerator ProcessCycle()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / duration));
            movingNode.localPosition = nodeHome + Vector3.right * travel * k;
            // заготовка вращается вокруг своей оси, как в патроне токарного станка
            blankRenderer.transform.Rotate(Vector3.up, spindleSpeed * Time.deltaTime, Space.Self);
            yield return null;
        }
        movingNode.localPosition = nodeHome;
        blankRenderer.transform.localRotation = visualRotation;
        cycle = null;
        processed = true;
        blankRenderer.material.color = finishedColor;
        SetStage(Stage.Output);
    }

    // ---------- вспомогательное ----------

    private bool InZone(BoxCollider zone, Vector3 point)
    {
        Vector3 local = zone.transform.InverseTransformPoint(point) - zone.center;
        Vector3 half = zone.size * 0.5f;
        return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
    }

    private void SetGuard(bool open)
    {
        guardOpen = open;
        if (HasDoors) SetDoors(open ? 1f : 0f);
        else if (guard != null) guard.localPosition = guardClosed + (open ? guardOpenOffset : Vector3.zero);
    }

    private void SetStage(Stage s)
    {
        CurrentStage = s;
        stepText.text = $"Этап {(int)s} из 9. {Title(s)}";
        messageText.text = Hint(s);
    }

    private void Message(string text)
    {
        messageText.text = text;
    }

    private void Error(string text, ref int kind)
    {
        errors++;
        kind++;
        messageText.text = text;
    }

    // расшифровка ошибок для результата
    private string ErrorDetails()
    {
        if (errors == 0) return "";
        string s = "";
        if (errStart > 0) s += $"\nПуск не вовремя: {errStart}";
        if (errPlace > 0) s += $"\nНеверная установка заготовки: {errPlace}";
        if (errOutput > 0) s += $"\nДеталь не на столе продукции: {errOutput}";
        if (errGuard > 0) s += $"\nДверь открыта во время обработки: {errGuard}";
        return s;
    }

    private string Title(Stage s)
    {
        switch (s)
        {
            case Stage.Intro: return "Ознакомление";
            case Stage.Check: return "Исходное состояние";
            case Stage.Grab: return "Захват заготовки";
            case Stage.Place: return "Установка";
            case Stage.Guard: return "Ограждение";
            case Stage.Ready: return "Запуск";
            case Stage.Processing: return "Обработка";
            case Stage.Output: return "Готовая продукция";
            default: return "Результат";
        }
    }

    private string Hint(Stage s)
    {
        switch (s)
        {
            case Stage.Intro: return "Осмотрите станок, стол с заготовкой и стол готовой продукции. Нажмите «Далее».";
            case Stage.Check: return "Проверьте: заготовка на столе, патрон свободен, каретка в исходном положении. Нажмите «Далее».";
            case Stage.Grab: return "Возьмите заготовку со стола.";
            case Stage.Place: return "Вставьте заготовку в патрон через открытую дверь и отпустите.";
            case Stage.Guard: return "Закройте ограждение: сдвиньте двери станка рукой или нажмите «Ограждение».";
            case Stage.Ready: return "Заготовка установлена. Нажмите «Пуск».";
            case Stage.Processing: return "Идёт обработка. Дождитесь остановки каретки, двери не открывайте.";
            case Stage.Output: return requireGuard && !guardOpen ? "Откройте ограждение (двери станка) и возьмите готовую деталь." : "Возьмите готовую деталь из патрона.";
            default: return "Нажмите «Повторить», чтобы пройти ещё раз.";
        }
    }
}
