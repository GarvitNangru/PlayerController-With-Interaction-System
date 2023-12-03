using CustomTimers;
using PlayerController.Interactable;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using IInteractable = PlayerController.Interactable.IInteractable;

namespace PlayerController
{
    public class InteractionHandler : MonoBehaviour
    {


        [Header("Interaction Settings")]
        [SerializeField] private float radius = .3f;
        [SerializeField] private float distance = 5f;

        [SerializeField] private TMP_Text text;

        [Header("Interaction Input Action")]
        [SerializeField] private InputAction InteractionAction;


        #region Private Variables
        CustomUpdater CustomUpdate = null;

        bool currentlyInteracting = false;

        private Camera maincam;
        private IInteractable currentInteractingObject;
        #endregion
        private void Awake()
        {
            maincam = Camera.main; //TODO - Change this to GamePlayStats GetActiveCamera();
        }
        private void OnEnable()
        {
            InteractionAction.started += InteractWithCurrentObject;
            InteractionAction.canceled += InteractWithCurrentObject;
        }
        private void OnDisable()
        {
            InteractionAction.started -= InteractWithCurrentObject;
            InteractionAction.canceled -= InteractWithCurrentObject;
        }
        void FixedUpdate()
        {
            CheckForInteraction();
        }
        void CheckForInteraction()
        {
            if (!CanInteract())
            {
                if (currentlyInteracting)
                    StopInteracting();

                return;
            }

            if (Physics.SphereCast(maincam.transform.position, radius, maincam.transform.forward, out RaycastHit hit, distance))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject.TryGetComponent(out IInteractable interactable))
                {
                    if (!interactable.CanInteract())
                    {
                        return;
                    }

                    if (currentInteractingObject == null)   // If not interacting with any object;
                    {
                        OnInteractObjectEnter(interactable);
                    }
                    else if (currentInteractingObject != interactable) // If interacting Object Changed.
                    {
                        OnInteractObjectExit();
                        OnInteractObjectEnter(interactable);
                    }
                }
                else
                {
                    if (currentInteractingObject != null)
                        OnInteractObjectExit();
                }
            }
            else
            {
                if (currentInteractingObject != null)
                    OnInteractObjectExit();
            }
        }
        void OnInteractObjectEnter(IInteractable interactable)
        {
            if (currentlyInteracting)
                return;

            currentInteractingObject = interactable;

            if (currentInteractingObject != null)
            {
                interactable.Focus();
                text.text = interactable.Label;

                SetActionInteraction(true);
            }
        }
        private void StopInteracting()
        {
            currentlyInteracting = false;
            if (CustomUpdate != null) CustomTimerManager.Instance.StopUpdate(CustomUpdate);

            currentInteractingObject?.UnFocus();

            currentInteractingObject = null;
            text.text = "";

        }

        void SetActionInteraction(bool Enter)
        {
            InteractionAction.Disable();
            if (Enter)
            {
                switch (currentInteractingObject.Type)
                {
                    case InteractionType.SingleTap:
                        InteractionAction.ChangeBinding(0).WithPath("<Keyboard>/E").WithInteraction("tap(duration=0.2)");
                        break;

                    case InteractionType.Hold:
                        InteractionAction.ChangeBinding(0).WithPath("<Keyboard>/E").WithInteraction("hold(duration=2)");
                        break;

                    case InteractionType.RapidTaps:
                        InteractionAction.ChangeBinding(0).WithPath("<Keyboard>/E").WithInteraction("multiTap(tapCount=3)");
                        break;

                    case InteractionType.Pull:
                        InteractionAction.ChangeBinding(0).WithPath("<Mouse>/leftButton").WithInteraction("tap(duration=0.2)");
                        break;
                }
                InteractionAction.Enable();
            }
        }

        void InteractWithCurrentObject(InputAction.CallbackContext ctx)
        {
            if (currentInteractingObject == null) return;

            switch (currentInteractingObject.Type)
            {
                case InteractionType.SingleTap:

                    CustomUpdate = CustomTimerManager.Instance.CreateUpdate(Time.fixedDeltaTime, () =>
                    {
                        float per = InteractionAction.GetTimeoutCompletionPercentage();
                        Debug.Log(per);
                        if (currentInteractingObject.OnInteractionUpdate(per))
                        {
                            StopInteracting();
                        }
                    });
                    break;
                case InteractionType.RapidTaps:
                case InteractionType.Hold:
                    if (ctx.started)
                    {
                        currentlyInteracting = true;

                        CustomUpdate = CustomTimerManager.Instance.CreateUpdate(Time.fixedDeltaTime, () =>
                        {
                            float per = InteractionAction.GetTimeoutCompletionPercentage();
                            Debug.Log(per);
                            if (currentInteractingObject.OnInteractionUpdate(per))
                            {
                                StopInteracting();
                            }
                        });
                    }
                    else if (ctx.canceled)
                    {
                        currentInteractingObject.OnInteractionCancel();
                        StopInteracting();
                    }
                    break;


                case InteractionType.Pull:

                    if (ctx.started)
                    {
                        currentlyInteracting = true;
                        InputHandler.Instance.TogglePlayerCamInput(false);

                        CustomUpdate = CustomTimerManager.Instance.CreateUpdate(Time.fixedDeltaTime, () =>
                        {
                            if (currentInteractingObject.OnInteractionUpdate(InputHandler.Instance.GetMouse()))
                            {
                                InputHandler.Instance.TogglePlayerCamInput(true);
                                StopInteracting();
                            }

                        }, true);
                    }
                    else if (ctx.canceled)
                    {
                        currentInteractingObject.OnInteractionCancel();
                        InputHandler.Instance.TogglePlayerCamInput(true);
                        StopInteracting();
                    }

                    break;
            }
        }
        void OnInteractObjectExit()
        {
            if (currentlyInteracting)
                return;

            StopInteracting();
        }

        bool CanInteract()
        {
            return true;
        }
    }
}