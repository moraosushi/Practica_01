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

   
    public int idleTime = 0;

    [Header("Animator Parameters")]
    public string motionParam = "motion";
    public string mirrorIdleParam = "mirrorIdle";
    public string turn180Param = "turn180";
    public string idleParam = "Idle_random";

    [Header("Animation Smoothing")]
    [Range(0, 1f)]
    public float StartAnimTime = 0.3f;
    [Range(0, 1f)]
    public float StopAnimTime = 0.15f;

    [Header("Wall Detection")]
    public LayerMask obstacleLayers;
    public float wallStopThreshold = 30f;
    public float wallStopDistance = 0.9f;
    public float rayHeight = 0.5f;


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

      //jump input
        if (Input.GetButton("Jump"))
        {
            animator.SetTrigger("jump");

        }
        //animaciones idle random en periodo de tiempo
     
        if (!Input.anyKey)
        {
            idleTime += 1;
            print(idleTime);
        }
        else
        {
            idleTime = 0;
        }
        int randomIdle = Random.Range(1, 5);
        

        if (idleTime == Random.Range(165, 440))
        {
            animator.SetTrigger("Idle_random");
            animator.SetInteger("IdleType", randomIdle);
            print("idle random");
            idleTime = 0;                  
        }
        else if (idleTime >= 440)
        {
            idleTime = 0;
        }

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

                               
            if (Vector3.Angle(transform.forward, desiredMoveDirection) >= degreesToTurn)
            {
                turn180 = true;
            }
            else
            {
                turn180 = false;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), rotationSpeed * Time.deltaTime);
            }
          
            //180 turning
            animator.SetBool(turn180Param, turn180);
            //move character
            animator.SetFloat(motionParam, Speed, StartAnimTime, Time.deltaTime);

            //wall detection
            Vector3 rayOrigin = transform.position;
            rayOrigin.y += rayHeight;

            wallRay.origin = rayOrigin;
            wallRay.direction = transform.forward;
            bool wallDetected = Vector3.Angle(transform.forward, desiredMoveDirection) < wallStopThreshold && Physics.Raycast(wallRay, wallStopDistance, obstacleLayers);
            Debug.DrawRay(rayOrigin, transform.forward, Color.red);

            if (wallDetected)
            {
                //simple foot ik for idle animation
                animator.SetBool(mirrorIdleParam, mirrorIdle);
                //Stop the character
                animator.SetFloat(motionParam, 0f, StopAnimTime, Time.deltaTime);
                Debug.DrawRay(rayOrigin, transform.forward, Color.yellow);
            }
            else
                //Move character
                animator.SetFloat(motionParam, Speed, StartAnimTime, Time.deltaTime);
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
