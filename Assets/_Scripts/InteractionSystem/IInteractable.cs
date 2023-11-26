using UnityEngine.InputSystem;

namespace PlayerController.Interactable
{
    public interface IInteractable
    {
        InputActionReference InteractionAction { get; }
        InteractionType Type { get; }
        float MaxValue { get; }
        string Label { get; }
        bool IsMultiUse { get; }
        void Focus();
        void UnFocus();

        bool OnInteractionComplete();
        bool OnInteractionUpdate(float percentage);
        void OnInteractionCancel();
        bool CanInteract();
    }


    public enum InteractionType
    {
        SingleTap,
        Hold,
        RapidTaps,
        Pull
    }
}