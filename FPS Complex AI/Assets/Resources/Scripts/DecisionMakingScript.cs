using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecisionMakingScript : MonoBehaviour
{
    CoverFinderScript coverFinder;
    MovementScript movementScript;
    AgentScript agentScript;
    PlayerScript playerScript;
    public GameObject player;

    public List<GameObject> allyAgents;

    int noOfAllies = 0;
    public int decisionSpeed = 0;

    // ALARMS
    public bool underAttack;
    public bool coverUnderAttack;
    public bool outOfAmmo;
    
    public bool flankPlayer;

    public bool moveToCover;
    public bool returnToCover;
    public bool hide;
    public bool attackFromCover;

    public bool attackPlayerCover;
    public bool attackPlayer;
    public bool playerVisible;
    public bool playerMovingTowards;
    public bool moveToPlayer;

    public bool addCover = false;
    public bool previousDecisionFinished = false;

    private Vector3 coverOffset = new Vector3(3, 0, 0);
    private float decisionTimer;


    public void MoveToCover()
    {
        agentScript.ChangeActionText("Moving to cover");
        Debug.Log("Adding cover " + coverFinder.GetClosestCoverToHide().name);
        coverFinder.movingToCover = true;
        movementScript.AddPointToList(coverFinder.GetClosestCoverToHide().transform.position - coverOffset);
    }
    
    public void ReturnToCover()
    {
        agentScript.ChangeActionText("Returning to previous cover");
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
        agentScript.ChangeActionText("Attacking Player's Cover");
        agentScript.shoot = true;
    }

    public void AttackPlayer()
    {
        agentScript.ChangeActionText("Attacking Player");
        agentScript.shoot = true;
    }
    
    public bool IsCoverInUse(GameObject cover)
    {
        for (int i = 0; i < allyAgents.Count; i++)
        {
            CoverFinderScript coverFinder = allyAgents[i].GetComponent<CoverFinderScript>();
            if (coverFinder.currentCover == cover)
                return true;
        }

        return false;
    }

    // MAKE SMTHG LIKE WHILE LOOP TO PERFORM DECISIONS (IN COROUTINE?) AND HAVE ALERTS BREAK IT.


    // Start is called before the first frame update
    void Start()
    {
        // Getting all allied AI units.
        allyAgents = new List<GameObject>();
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < agents.Length; i++)
            allyAgents.Add(agents[i]);

        coverFinder = gameObject.GetComponent<CoverFinderScript>();
        movementScript = gameObject.GetComponent<MovementScript>();
        agentScript = gameObject.GetComponent<AgentScript>();

        decisionTimer = 0.0f;

        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        // DEALING WITH ALARMS FIRST 
        /*if()
        
        underAttack;
        coverUnderAttack;
        outOfAmmo;
    */
        


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
