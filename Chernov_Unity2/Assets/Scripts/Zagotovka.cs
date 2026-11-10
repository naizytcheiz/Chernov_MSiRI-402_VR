using System.Collections;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Zagotovka : MonoBehaviour
{
    public Transform tochka;
    public float rasstoyanie = 0.1f;
    public float pikseli = 50f;
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
        StartCoroutine(Proverka(hand));
    }

    private IEnumerator Proverka(Hand hand)
    {
        yield return null;
        bool ryadom = Vector3.Distance(transform.position, tochka.position) < rasstoyanie;

        // без шлема: курсор мыши наведён на патрон
        if (hand.noSteamVRFallbackCamera != null)
        {
            Vector3 naEkrane = hand.noSteamVRFallbackCamera.WorldToScreenPoint(tochka.position);
            float doKursora = Vector2.Distance(naEkrane, Input.mousePosition);
            if (naEkrane.z > 0 && doKursora < pikseli) ryadom = true;
        }

        if (ryadom)
        {
            rb.isKinematic = true;
            transform.SetParent(tochka);
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
