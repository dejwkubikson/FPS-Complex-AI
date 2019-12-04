using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class firstTreeDecisionMakingScript : MonoBehaviour
{
    CoverFinderScript coverFinder;
    MovementScript movementScript;
    AgentScript agentScript;
    PlayerScript playerScript;
    EmotionScript emotion;
    public GameObject player;

    public List<GameObject> allyAgents;

    int noOfAllies = 0;
    public int firstTreeDecisionSpeed = 0;

    // ALARMS
    public bool underAttack;
    public bool coverUnderAttack;
    public bool outOfAmmo;
    private bool alarm;
    
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

    public bool attack;
    public bool defend;
    public bool move;

    public bool addCover = false;
    public bool previousfirstTreeDecisionFinished = false;

    private Vector3 coverOffset = new Vector3(3, 0, 0);
    private float firstTreeDecisionTimer = 3.0f;


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

    public bool CheckIfAllyMoving()
    {
        for(int i = 0; i < allyAgents.Count; i++)
        {
            CoverFinderScript script = allyAgents[i].GetComponent<CoverFinderScript>();
            if (script.movingToCover)
                return true;
        }

        return false;
    }

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
        emotion = gameObject.GetComponent<EmotionScript>();

        firstTreeDecisionTimer = 0.0f;

        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (agentScript.dead)
            return;

        firstTreeDecisionTimer += Time.deltaTime;

        if (playerScript.attackedAgent == this.gameObject)
        {
            underAttack = true;
            alarm = true;
        }
        if (playerScript.attackedCover == coverFinder.currentCover && coverFinder.nearCover)
        {
            coverUnderAttack = true;
            alarm = true;
        }
        if (agentScript.ammo <= 0)
        {
            outOfAmmo = true;
            alarm = true;
        }

        // Stoping the loop if alarm is not on or if the not enough time elapsed to make firstTreeDecision
        if (alarm == false)
            if (firstTreeDecisionSpeed > firstTreeDecisionTimer)
                return;

        // ALARMS
        if(underAttack)
        {
            // Checking which cover is closer to agent, previous or the chosen one
            if (Vector3.Distance(gameObject.transform.position, coverFinder.currentCover.transform.position) < Vector3.Distance(gameObject.transform.position, coverFinder.nextCover.transform.position))
                returnToCover = true;
            else
            if (coverFinder.nextCover == null)
            {
                moveToCover = true;
                addCover = true;
            }

            hide = true;
        }

        if(coverUnderAttack)
            hide = true;

        if(outOfAmmo)
        {
            agentScript.reload = true;
            attackPlayer = false;
            attackPlayerCover = false;

            hide = true;
        }

        if(coverFinder.nearCover && hide)
        {
            if (coverFinder.currentCover.CompareTag("Small Object"))
                movementScript.crouch = true;
        }
        // END OF ALARMS

        // firstTreeDecisionS
        int firstTreeDecision = Random.Range(1, 11);

        switch(emotion.currentState)
        {
            case EmotionScript.States.confidentState:
                if (firstTreeDecision < 5)
                    attack = true;
                else if (firstTreeDecision >= 5 && firstTreeDecision < 8)
                    move = true;
                else
                    defend = true;
                break;

            case EmotionScript.States.fearfulState:
                if (firstTreeDecision < 3)
                    attack = true;
                else if (firstTreeDecision >= 3 && firstTreeDecision < 5)
                    move = true;
                else
                    defend = true;
                break;

            case EmotionScript.States.rageState:
                if (firstTreeDecision < 7)
                    attack = true;
                else if (firstTreeDecision >= 7 && firstTreeDecision < 9)
                    move = true;
                else
                    defend = true;
                break;

            case EmotionScript.States.determinedState:
                if (firstTreeDecision < 5)
                    attack = true;
                else if (firstTreeDecision >= 5)
                    move = true;
                break;

            case EmotionScript.States.normalState:
                if (firstTreeDecision < 3)
                    attack = true;
                else if (firstTreeDecision >= 3 && firstTreeDecision < 7)
                    move = true;
                else
                    defend = true;
                break;

            default:
                if (firstTreeDecision < 4)
                    attack = true;
                else if (firstTreeDecision >= 4 && firstTreeDecision < 7)
                    move = true;
                else
                    defend = true;
                break;
        }

        int secondTreeDecision = Random.Range(1, 4); // 33%

        if (move)
        {
            // Attacking only if player is visible - moving and attacking
            if (agentScript.playerVisible)
                attack = true;

            if (secondTreeDecision == 1)
                returnToCover = true;

            if (secondTreeDecision > 1)
                moveToCover = true;
        }

        if (attack)
        {
            if(CheckIfAllyMoving() || secondTreeDecision < 2)
            {
                if (agentScript.playerVisible)
                    attackPlayer = true;
                else
                    attackPlayer = false;

                if (agentScript.playerVisible == false && agentScript.playerCoverVisible)
                    attackPlayerCover = true;
                else
                    attackPlayerCover = false;
            }
            else
            if(secondTreeDecision >= 2)
            {
                if(agentScript.playerVisible && agentScript.playerCoverVisible)
                {
                    if (secondTreeDecision == 2 || (playerScript.nearCover && playerScript.crouching == false))
                        attackPlayer = true;
                    else
                        attackPlayerCover = true;
                }
            }
        }

        if(defend)
        {
            if (coverFinder.currentCover.CompareTag("Small Object") && coverFinder.nearCover && secondTreeDecision == 1)
                attackFromCover = true;

            if (secondTreeDecision > 1)
                hide = true;
        }

        // END OF DECISIONS


        // Assigning decisions to scripts

        @@@

        if (moveToCover)
        {
            if (addCover)
            {
                addCover = false;
                MoveToCover();
            }
        }

        if (returnToCover)
        {
            returnToCover = false;
            ReturnToCover();
        }

        if (hide == false)
            movementScript.crouch = false;
    
        
    }
}
