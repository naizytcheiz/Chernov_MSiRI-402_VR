using UnityEngine;
using Valve.VR.InteractionSystem;

// Захват и отпускание заготовки. Заменяет Throwable: перед захватом спрашивает MachineTrainer.
[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(Rigidbody))]
public class TrainingBlank : MonoBehaviour
{
    public MachineTrainer trainer;

    // VelocityMovement: рука тянет предмет через физику, поэтому он упирается в станок и не проходит сквозь него
    private Hand.AttachmentFlags attachmentFlags =
        Hand.AttachmentFlags.DetachFromOtherHand |
        Hand.AttachmentFlags.VelocityMovement |
        Hand.AttachmentFlags.TurnOffGravity;

    private Interactable interactable;
    private Rigidbody rb;

    public bool IsHeld { get; private set; }
    public float ReleaseTime { get; private set; }
    public int ReleaseNumber { get; private set; }

    private void Awake()
    {
        interactable = GetComponent<Interactable>();
        rb = GetComponent<Rigidbody>();
    }

    // рука навела на предмет и нажала захват
    private void HandHoverUpdate(Hand hand)
    {
        GrabTypes grab = hand.GetGrabStarting();
        if (grab == GrabTypes.None || IsHeld) return;
        if (trainer == null || !trainer.CanGrab()) return;

        hand.HoverLock(interactable);
        hand.AttachObject(gameObject, grab, attachmentFlags);
    }

    // предмет в руке: отпускаем, когда кнопка захвата отпущена
    private void HandAttachedUpdate(Hand hand)
    {
        if (hand.IsGrabEnding(gameObject))
        {
            hand.DetachObject(gameObject);
            hand.HoverUnlock(interactable);
        }
    }

    private void OnAttachedToHand(Hand hand)
    {
        // из патрона или со стола деталь берётся обычным физическим телом
        rb.isKinematic = false;
        IsHeld = true;
        trainer.NotifyGrab();
    }

    private void OnDetachedFromHand(Hand hand)
    {
        Release();
    }

    // общее отпускание: обычная физика без скорости броска
    private void Release()
    {
        IsHeld = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        ReleaseTime = Time.time;
        ReleaseNumber++;
    }

    // перенос предмета: и Transform, и Rigidbody, чтобы интерполяция не вернула старую позицию
    private void MoveTo(Transform point)
    {
        transform.SetPositionAndRotation(point.position, point.rotation);
        rb.position = point.position;
        rb.rotation = point.rotation;
    }

    // точная установка в точку
    public void Snap(Transform point)
    {
        rb.isKinematic = true;
        MoveTo(point);
    }

    // возврат на исходный стол при Повторить
    public void ResetTo(Transform home)
    {
        IsHeld = false;
        transform.SetParent(null);
        rb.isKinematic = true;
        MoveTo(home);
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
