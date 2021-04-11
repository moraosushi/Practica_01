using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[RequireComponent(typeof(Collider))]


public class TimelineActivator : MonoBehaviour
{
    public PlayableDirector playableDirector; //reference to the playable director component that contains the timeline
    public string playerTAG; //tag to check against when some object enters or exit the trigger
    public Transform interactionLocation; //reference to the transform where the player will be placed when the cinematic starts
    public bool autoActivate = false; //will the cinematic be activated when an object with a valic TAG enters the trigger.

    public bool interact {get; set;} //property activated from outside this object that behaves like an input button

    [Header("Activation Zone Events")]
    public UnityEvent OnPlayerEnter; //events to raise when an object with a valid tag enters the trigger
    public UnityEvent OnPlayerExit; // Events to raise when an object with a valid tag exits the trigger.

    [Header("Timeline Events")]
    public UnityEvent OnTimeLineStart; //events to raise when the timeline starts playing
    public UnityEvent OnTimeLineEnd;// Events to raise when the timeline stops playing

    private bool isPlaying; //determines if the timeline is currently playing
    private bool playerInside; // determines if the player is inside of the trigger or not
    private Transform playerTransform; //reference to the player transform

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag.Equals(playerTAG))
        {
            playerInside = true;
            playerTransform = other.transform;
            OnPlayerEnter.Invoke();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals(playerTAG))
        {
            playerInside = false;
            playerTransform = null;
            OnPlayerExit.Invoke();
        }
    }
    private void PlayTimeline()
    {
        //place the character at the correct interaction position
        if (playerTransform && interactionLocation)
            playerTransform.SetPositionAndRotation(interactionLocation.position, interactionLocation.rotation);

        //avoid infinite interaction loop
        if (autoActivate)
            playerInside = false;

        //play de timeline
        if (playableDirector)
            playableDirector.Play();

        //set variables
        isPlaying = true;
        interact = false;

        //wait for timeline to end
        StartCoroutine(waitForTimeLineToEnd());

    }
    private IEnumerator waitForTimeLineToEnd()
    {
        //invoke the methods linked to the beginning of the cinematic
        OnTimeLineStart.Invoke();

        //get the duration of the timeline from the playable director
        float timeLineDuration = (float)playableDirector.duration;

        //wait until the cinematic playing is over
        while (timeLineDuration > 0)
        {
            timeLineDuration -= Time.deltaTime;
            yield return null; 
        }
        //reset variable
        isPlaying = false;

        //invoke the methods linked to the end of the cinematic
        OnTimeLineEnd.Invoke();
    }
    private void Update()
    {
        if(playerInside && !isPlaying)
        {
            if (interact || autoActivate)
            {
                PlayTimeline();
            }
        }
    }
}
