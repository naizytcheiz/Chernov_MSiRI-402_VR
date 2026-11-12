using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Valve.VR.InteractionSystem;

// Заготовка сцены 10.11: если её отпустили рядом с патроном, она закрепляется в нём.
// Захват и бросок делает стандартный Throwable из SteamVR, этот скрипт только проверяет место отпускания.
public class ChuckWorkpiece : MonoBehaviour
{
    [FormerlySerializedAs("tochka")] public Transform chuckPoint;             // точка крепления в патроне
    [FormerlySerializedAs("rasstoyanie")] public float snapDistance = 0.1f;   // насколько близко к патрону надо отпустить, м
    [FormerlySerializedAs("pikseli")] public float cursorPixels = 50f;        // без шлема: насколько близко курсор к патрону, пикселей
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // взяли из патрона: снова обычное физическое тело, чтобы рука могла его тянуть
    private void OnAttachedToHand(Hand hand)
    {
        transform.SetParent(null);
        rb.isKinematic = false;
    }

    private void OnDetachedFromHand(Hand hand)
    {
        StartCoroutine(CheckRelease(hand));
    }

    // проверка через кадр, когда Throwable закончил отпускание
    private IEnumerator CheckRelease(Hand hand)
    {
        yield return null;
        bool near = Vector3.Distance(transform.position, chuckPoint.position) < snapDistance;

        // без шлема: курсор мыши наведён на патрон
        if (hand.noSteamVRFallbackCamera != null)
        {
            Vector3 onScreen = hand.noSteamVRFallbackCamera.WorldToScreenPoint(chuckPoint.position);
            float toCursor = Vector2.Distance(onScreen, Input.mousePosition);
            if (onScreen.z > 0 && toCursor < cursorPixels) near = true;
        }

        if (near)
        {
            rb.isKinematic = true;
            transform.SetParent(chuckPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            print("Заготовка закреплена в патроне");
        }
        else
        {
            transform.SetParent(null);
            rb.isKinematic = false;
        }
    }
}
