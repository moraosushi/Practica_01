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

   
   
    public float idleTime = 0.0f;
    public float randomTime;
    public int randomIdle;
 

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
        randomTime = Random.Range(15.0f, 40.0f);
        randomIdle = Random.Range(1, 7);

    }

    public void moveCharacter(float hInput, float vInput, Camera cam, bool jump, bool dash)
    {
        //calculate the input magnitude
        Speed = new Vector2(hInput, vInput).normalized.sqrMagnitude;
       
      //jump input
        if (jump)
        {
            animator.SetTrigger("jump");
           
        }
        
        //animaciones idle random en periodo de tiempo
        if (!Input.anyKey) 
        {
            idleTime += Time.deltaTime;
           // print(idleTime);
            
        }
        else
        {
            
            idleTime = 0.0f;
           
        }
       
        if (idleTime >= randomTime) //si el tiempo esta en x rango de tiempo aleatorio
        {
            animator.SetTrigger("Idle_random"); //se hace el cambio al submachine system de idles
            animator.SetInteger("IdleType", randomIdle); //usando el numero aleatorio anterior, se escoge una de las animaciones de idle
            print("idle random");
            print(idleTime);// para saber que si funciona
            randomTime = Random.Range(15.0f, 40.0f); //asignar nuevo valor en el que se dara el idle
            randomIdle = Random.Range(1, 7); //asignar nuevo valor de la animacion idle
            idleTime = 0.0f;                  //el tiempo se reinicia
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

            // Wall Detection
            Vector3 rayOriging = transform.position;
            rayOriging.y += rayHeight;

            wallRay.origin = rayOriging;
            wallRay.direction = transform.forward;

            bool wallDetected = Vector3.Angle(transform.forward, desiredMoveDirection) < wallStopThreshold && Physics.Raycast(wallRay, wallStopDistance, obstacleLayers);

            Debug.DrawRay(rayOriging, transform.forward, Color.red);

            if (wallDetected)
            {
                // Simple foot IK for idle animation
                animator.SetBool(mirrorIdleParam, mirrorIdle);
                // Stop the character
                animator.SetFloat(motionParam, 0f);
                animator.SetBool("turn180", true);
                Debug.DrawRay(rayOriging, transform.forward, Color.yellow);
            }
            else
            {
                // Move the character
                animator.SetFloat(motionParam, Speed, StartAnimTime, Time.deltaTime);
            }
                
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

