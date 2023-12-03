using CustomTimers;
using UnityEngine;
using Utils.Extensions;

namespace PlayerController.Interactable
{
    public class InteractableExample : MonoBehaviour, IInteractable
    {
        [SerializeField] private InteractionType interactionType;
        [SerializeField] private float maxValue;
        [SerializeField] private string label;
        [SerializeField] private bool multiUse = false;
        public InteractionType Type => interactionType;
        public float MaxValue => maxValue;
        public float Value { get; private set; }
        public string Label => label;
        public bool IsInteractionCompleted { get; private set; }
        public bool IsOnceInteracted { get; private set; }
        public bool IsMultiUse => multiUse;

        public void Focus()
        {
            Debug.Log($"Focusing with {gameObject.name.ToColor("black")}");
        }

        public void UnFocus()
        {
            Debug.Log($"UnFocused {gameObject.name.ToColor("black")}");
        }

        public bool OnInteractionUpdate(float percentage)
        {
            switch (interactionType)
            {
                case InteractionType.RapidTaps:
                    transform.localScale = Vector3.one + new Vector3(percentage / 10, percentage / 10, percentage / 10);
                    Value += percentage;
                    break;


                case InteractionType.Hold:
                    Value += percentage;
                    break;

                case InteractionType.SingleTap:

                    Value = percentage;

                    break;
            }


            return OnInteractionComplete();
        }
        public bool OnInteractionUpdate(Vector2 mouseInput)
        {
            switch (interactionType)
            {
                case InteractionType.Pull:

                    float rotationAmount = mouseInput.sqrMagnitude * 1;
                    transform.Rotate(0f, rotationAmount, 0f);

                    Value += mouseInput.sqrMagnitude;
                    break;
            }

            return OnInteractionComplete();
        }

        public void OnInteractionCancel()
        {
            switch (interactionType)
            {
                case InteractionType.RapidTaps:

                    transform.localScale = Vector3.one;

                    break;
            }
        }


        public bool CanInteract()
        {
            return multiUse || (!IsOnceInteracted);
        }

        public bool OnInteractionComplete()
        {
            if (Value >= MaxValue)
            {
                IsOnceInteracted = true;
                IsInteractionCompleted = true;
                if (IsMultiUse)
                {
                    ResetInteraction();
                }
                Debug.LogError("Interaction Completed");

                return true;
            }
            return false;
        }

        public void ResetInteraction()
        {
            CustomTimerManager.Instance.CreateTimer(2.0f, () =>
            {
                IsInteractionCompleted = false;
                Value = 0f;
            });
        }
    }
}
