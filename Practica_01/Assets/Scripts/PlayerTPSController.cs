using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTPSController : MonoBehaviour
{
    public Camera cam;
    public UnityEvent onInteractionInput;
    private InputData input;
    private CharacterAnimBasedMovement characterMovement;

    public bool onInteractionZone {get;set;} 

    void Start()
    {
        characterMovement = GetComponent<CharacterAnimBasedMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        //get input from player
        input.getInput();
        //use jump as action button only if the character is inside a InteractionZone
        if (onInteractionZone && input.jump)
        {
            onInteractionInput.Invoke();
        }

        //apply input to character
        characterMovement.moveCharacter(input.hMovement, input.vMovement, cam, input.jump, input.dash);
    }
}
