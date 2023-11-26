using PlayerController;
using UnityEngine;

public class animatorTEst : MonoBehaviour
{
    FirstPersonController firstPersonController;
    public MovementInputData movementInputData;

    [Range(-6.0f, 6.0f)] public float horizontal;
    [Range(-6.0f, 6.0f)] public float vertical;

    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        firstPersonController = GetComponentInParent<FirstPersonController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //var speed = firstPersonController.smoothCurrentSpeed;
        //horizontal = movementInputData.InputVector.x * speed;
        //vertical = movementInputData.InputVector.y * speed;

        //animator.SetFloat("Horizontal", horizontal);
        //animator.SetFloat("Vertical", vertical);
    }
}
