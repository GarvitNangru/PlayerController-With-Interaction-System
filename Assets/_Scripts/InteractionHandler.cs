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
        [SerializeField] private float radius = .3f;
        [SerializeField] private float distance = 5f;

        [SerializeField] private TMP_Text text;

        CustomTimer HoldTimer = null;
        CustomUpdater CustomUpdate = null;

        float lastTapTime = 0.0f;
        int rapidTapsCount = 0;
        float rapidTapCancelTime = 0.5f;
        bool currentlyInteracting = false;


        private Camera maincam;
        private IInteractable currentInteractingObject;
        private void Awake()
        {
            maincam = Camera.main; //TODO - Change this to GamePlayStats GetActiveCamera();
        }
        void FixedUpdate()
        {
            CheckForInteraction();
        }
        void CheckForInteraction()
        {
            if (!CanInteract()) return;

            if (Physics.SphereCast(maincam.transform.position, radius, maincam.transform.forward, out RaycastHit hit, distance))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject.TryGetComponent(out Interactable.IInteractable interactable))
                {
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
                interactable.InteractionAction.action.performed += InteractWithCurrentObject;
                interactable.InteractionAction.action.canceled += InteractWithCurrentObject;

                //        InputHandler.Instance.FocusedWithInteractable(interactable);

                //     CanvasItem.MoveToThisObject(currentInteractingObject.transform, interactable.ItemData.ItemName, "Good item");
            }
        }
        private void StopInteracting()
        {
            currentlyInteracting = false;
            if (HoldTimer != null) CustomTimerManager.Instance.StopTimer(HoldTimer);
            if (CustomUpdate != null) CustomTimerManager.Instance.StopUpdate(CustomUpdate);
            rapidTapsCount = 0;


            if (currentInteractingObject != null)
            {
                currentInteractingObject.InteractionAction.action.performed -= InteractWithCurrentObject;
                currentInteractingObject.InteractionAction.action.canceled -= InteractWithCurrentObject;
                currentInteractingObject.UnFocus();
            }
            currentInteractingObject = null;
            text.text = "";

        }

        void InteractWithCurrentObject(InputAction.CallbackContext ctx)
        {
            //if (currentInteractingObject != null)
            //    if (currentInteractingObject.TryGetComponent(out Interactable.IInteractable interactable))
            //    {
            //        interactable.Interact();
            //    }

            if (currentInteractingObject == null) return;

            switch (currentInteractingObject.Type)
            {
                case InteractionType.SingleTap:

                    currentInteractingObject.OnInteractionComplete();

                    break;

                case InteractionType.Hold:
                    if (ctx.performed)
                    {
                        currentlyInteracting = true;
                        HoldTimer = CustomTimerManager.Instance.CreateTimer(currentInteractingObject.MaxValue, () => currentInteractingObject.OnInteractionComplete());
                    }
                    else
                    {
                        StopInteracting();
                    }
                    break;

                case InteractionType.RapidTaps:

                    if (ctx.performed)
                    {
                        if (lastTapTime - Time.time > rapidTapCancelTime)
                        {
                            rapidTapsCount = 0;
                        }

                        lastTapTime = Time.time;
                        rapidTapsCount++;
                        currentInteractingObject.OnInteractionUpdate(rapidTapsCount);
                        if (rapidTapsCount >= currentInteractingObject.MaxValue)
                            currentInteractingObject.OnInteractionComplete();
                    }
                    break;

                case InteractionType.Pull:

                    if (ctx.performed)
                    {
                        currentlyInteracting = true;
                        InputHandler.Instance.TogglePlayerCamInput(false);

                        CustomUpdate = CustomTimerManager.Instance.CreateUpdate(.01f, () =>
                        {
                            var mouse = InputHandler.Instance.GetMouse();
                            var magnitude = mouse.magnitude;
                            Debug.Log(magnitude);
                            currentInteractingObject.OnInteractionUpdate(magnitude);

                            //if (Mathf.Abs(magnitude) >= currentInteractingObject.MaxValue)
                            //{
                            //    InputHandler.Instance.TogglePlayerCamInput(true);
                            //    StopInteracting();
                            //    currentInteractingObject.OnInteractionComplete();
                            //}

                        }, false);
                    }
                    else
                    {
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