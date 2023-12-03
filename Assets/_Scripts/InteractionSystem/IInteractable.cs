using UnityEngine;

namespace PlayerController.Interactable
{
    public interface IInteractable
    {
        InteractionType Type { get; }
        float Value { get; }
        float MaxValue { get; }
        string Label { get; }
        bool IsMultiUse { get; }
        bool IsInteractionCompleted { get; }
        public bool IsOnceInteracted { get; }
        void Focus();
        void UnFocus();
        bool OnInteractionComplete();
        bool OnInteractionUpdate(float percentage);
        bool OnInteractionUpdate(Vector2 mouseInput);
        void OnInteractionCancel();
        bool CanInteract();
        void ResetInteraction();
    }


    public enum InteractionType
    {
        SingleTap,
        Hold,
        RapidTaps,
        Pull
    }
}