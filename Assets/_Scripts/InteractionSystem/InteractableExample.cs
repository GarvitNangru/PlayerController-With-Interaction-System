using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Extensions;

namespace PlayerController.Interactable
{
    public class InteractableExample : MonoBehaviour, IInteractable
    {

        [SerializeField] private InputActionReference interactionAction;
        [SerializeField] private InteractionType interactionType;
        [SerializeField] private float maxValue;
        [SerializeField] private string label;
        [SerializeField] private bool multiUse = false;
        public InteractionType Type => interactionType;
        public float MaxValue => maxValue;
        public string Label => label;

        public InputActionReference InteractionAction => interactionAction;

        public bool IsMultiUse => multiUse;

        public void Focus()
        {
            Debug.Log($"Focusing with {gameObject.name.ToColor("black")}");
        }

        public void UnFocus()
        {
            Debug.Log($"UnFocused {gameObject.name.ToColor("black")}");
        }

        public bool OnInteractionUpdate(float value)
        {
            switch (interactionType)
            {
                case InteractionType.RapidTaps:

                    transform.localScale = Vector3.one + new Vector3(value / 10, value / 10, value / 10);

                    break;

                case InteractionType.Pull:

                    // Adjust rotation based on the distance
                    float rotationAmount = value * 1;

                    // Apply the rotation to the object
                    transform.Rotate(0f, rotationAmount, 0f);

                    break;

                case InteractionType.Hold:

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
            return false;
        }

        public bool OnInteractionComplete()
        {
            Debug.Log("Interaction Complete");

            gameObject.SetActive(false);

            return true;
        }
    }
}
