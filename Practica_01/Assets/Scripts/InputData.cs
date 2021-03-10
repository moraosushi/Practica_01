using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]

public struct InputData
{
    //basic movement
    public float hMovement;
    public float vMovement;

    //Mouse rotation
    public float verticalMouse;
    public float horizontalMouse;

    //extra movement
    public bool dash;
    public bool jump;


   public void getInput()
    {
        //basic movement
        hMovement = Input.GetAxis("Horizontal");
        vMovement = Input.GetAxis("Vertical");

        //mouse/joystick rotation
        verticalMouse = Input.GetAxis("Mouse Y");
        horizontalMouse = Input.GetAxis("Mouse X");


        //extra movement
        dash = Input.GetButton("Dash");
        jump = Input.GetButton("Jump");
    }
}
