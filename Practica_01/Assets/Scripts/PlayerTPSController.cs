using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTPSController : MonoBehaviour
{
    public Camera cam;
    private InputData input;
    private CharacterAnimBasedMovement characterMovement;


    void Start()
    {
        characterMovement = GetComponent<CharacterAnimBasedMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        //get input from player
        input.getInput();

        //apply input to character
        characterMovement.moveCharacter(input.hMovement, input.vMovement, cam, input.jump, input.dash);
    }
}
