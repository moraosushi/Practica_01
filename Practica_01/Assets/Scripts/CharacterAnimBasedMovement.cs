using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class CharacterAnimBasedMovement : MonoBehaviour
{
    public float rotationSpeed = 4f;
    public float rotationThreshold = 0.3f;
    [Range(0, 180f)]
    public float degreesToTurn = 160f;

    [Header("Animator Parameters")]
    public string motionParam = "motion";
    public string mirrorIdleParam = "mirrorIdle";
    public string turn180Param = "turn180";

    [Header("Animation Smoothing")]
    [Range(0, 1f)]
    public float StartAnimTime = 0.3f;
    [Range(0, 1f)]
    public float StopAnimTime = 0.15f;

    private Ray wallRay = new Ray();
    private float Speed;
    private Vector3 desiredMoveDirection;
    private CharacterController characterController;
    private Animator animator;
    private bool mirrorIdle;
    private bool turn180;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    public void moveCharacter(float hInput, float vInput, Camera cam, bool jump, bool dash)
    {
        //calculate the input magnitude
        Speed = new Vector2(hInput, vInput).normalized.sqrMagnitude;

        //dash only if character has reached maxSpeed (animator parameter value)
        if (Speed >= Speed - rotationThreshold && dash)
        {
            Speed = 1.5f;
        }
        //physically move player
        if(Speed >= rotationThreshold)
        {
            animator.SetFloat(motionParam, Speed, StartAnimTime, Time.deltaTime);
            Vector3 forward = cam.transform.forward;
            Vector3 right = cam.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            //rotate the character towards desired move direction based on player input and camera position
            desiredMoveDirection = forward * vInput + right * hInput;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), rotationSpeed * Time.deltaTime);

        }
        else if (Speed < rotationThreshold)
        {
            animator.SetBool(mirrorIdleParam, mirrorIdle);
            animator.SetFloat(motionParam, Speed, StopAnimTime, Time.deltaTime);
        }
    }
    private void OnAnimatorIK(int layerIndex)
    {
        if (Speed < rotationThreshold) return;
        float distanceToLeftFoot = Vector3.Distance(transform.position, animator.GetIKPosition(AvatarIKGoal.LeftFoot));
        float distanceToRightFoot = Vector3.Distance(transform.position, animator.GetIKPosition(AvatarIKGoal.RightFoot));

        //right foot in front
        if (distanceToRightFoot > distanceToLeftFoot)
        {
            mirrorIdle = true;
        }
        //right foot behind
        else
        {
            mirrorIdle = false;
        }
    }
}
