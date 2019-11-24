using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecisionMakingScript : MonoBehaviour
{
    CoverFinderScript coverFinder;
    MovementScript movementScript;

    public int decisionSpeed = 0;
    public int fear = 50;
    public int confidence = 50;
    public int adrenaline = 0;

    public bool underAttack;
    public bool coverUnderAttack;
    public bool flankPlayer;

    public bool moveToCover;
    public bool returnToCover;
    public bool hide;
    public bool attackFromCover;

    public bool attackPlayerCover;
    public bool attackPlayer;
    public bool playerVisible;
    public bool playerMovingTowards;

    public bool addCover = false;

    private Vector3 coverOffset = new Vector3(3, 0, 0);
    public void MoveToCover()
    {
        Debug.Log("Adding cover " + coverFinder.GetClosestCoverToHide().name);
        coverFinder.movingToCover = true;
        movementScript.AddPointToList(coverFinder.GetClosestCoverToHide().transform.position - coverOffset);
    }
    
    public void ReturnToCover()
    {
        Debug.Log("Returning to previous cover if exists - " + coverFinder.currentCover.name);
        if(coverFinder.currentCover != null)
        {
            moveToCover = false;
            movementScript.RemoveFirstPoint();
            coverFinder.nextCover = coverFinder.currentCover;
            movementScript.AddPointToList(coverFinder.currentCover.transform.position - coverOffset);
        }
    }

    public void AttackPlayerCover()
    {

    }

    public void AttackPlayer()
    {
        // gun shot sound https://www.youtube.com/watch?v=gFGVCCg9Y44
    }

    // Start is called before the first frame update
    void Start()
    {
        coverFinder = gameObject.GetComponent<CoverFinderScript>();
        movementScript = gameObject.GetComponent<MovementScript>();
    }

    // MAKE SMTHG LIKE WHILE LOOP TO PERFORM DECISIONS (IN COROUTINE?) AND HAVE ALERTS BREAK IT.

    // Update is called once per frame
    void Update()
    {
        if (moveToCover)
        {
            if (addCover)
            {
                addCover = false;
                MoveToCover();
            }
        }

        if(returnToCover)
        {
            returnToCover = false;
            ReturnToCover();
        }
    }
}
