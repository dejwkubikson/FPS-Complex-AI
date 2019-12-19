using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class DecisionMakingScript : MonoBehaviour
{
    CoverFinderScript coverFinder;
    MovementScript movementScript;
    AgentScript agentScript;
    PlayerScript playerScript;
    EmotionScript emotion;
    public GameObject player;

    public List<GameObject> allyAgents;

    int noOfAgents = 0;
    public int decisionSpeed = 7;

    // ALARMS
    public bool underAttack;
    public bool coverUnderAttack;
    public bool outOfAmmo;
    private bool alarm;
    private float alarmCoolDown = 5.0f;
    
    public bool moveToCover;
    public bool returnToCover;
    public bool hide;
    public bool attackFromCover;

    public bool attackPlayerCover;
    public bool attackPlayer;
    public bool flankPlayer;
    public bool playerVisible;

    public bool attack;
    public bool defend;
    public bool move;

    public bool addCover = false;
    public bool previousDecisionFinished = false;

    private Vector3 coverOffset = new Vector3(3.5f, -4, 0);
    public float decisionTimer = 7.0f;

    // Introduced this boolean as the agent could be changing his mind all the time when alarm on.
    public bool alarmActionTaken = false;
    public int firstTreeDecision;
    public int secondTreeDecision;

    public void MoveToCover()
    {
        agentScript.ChangeActionText("Moving to cover");
        //coverFinder.movingToCover = true;
        // If there are no more covers seperating the agent from the player
        if (coverFinder.GetClosestCoverToHide() != null)
            movementScript.AddPointToList(coverFinder.GetClosestCoverToHide().transform.position - coverOffset);
        else
            if(agentScript.playerVisible == false)
                movementScript.AddPointToList(gameObject.transform.position + new Vector3(5, 0, 0));
    }
    
    public void ReturnToCover()
    {
        agentScript.ChangeActionText("Returning to previous cover");
        //Debug.Log("Returning to previous cover if exists - " + coverFinder.currentCover.name);
        if(coverFinder.currentCover != null)
        {
            moveToCover = false;
            //coverFinder.movingToCover = true;
            UnityEngine.Debug.Log("ReturnToCover calling RemoveFirstPoint()");
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
            if (coverFinder.currentCover == cover || coverFinder.nextCover == cover)
                return true;
        }

        return false;
    }

    public bool CanIFlank()
    {
        // If no allies
        if (noOfAgents == 1)
            return false;

        Vector3 playerPos = player.transform.position;

        // Map size is from z = -50 to z = 50, area considered in front is -20 to 20 if player is at z = 0
        int zOffset = 20;

        if(gameObject.transform.position.z > player.transform.position.z + zOffset || gameObject.transform.position.z < player.transform.position.z - zOffset)
            return true;

        return false;
    }

    public void ClearActions()
    {
        decisionTimer = 0;

        underAttack = false;
        coverUnderAttack = false;
        outOfAmmo = false;
        hide = false;

        returnToCover = false;
        moveToCover = false;
        addCover = false;
        attackPlayer = false;
        attackPlayerCover = false;
        flankPlayer = false;

        attack = false;
        move = false;
        defend = false;
        attackFromCover = false;

        agentScript.attackPlayer = false;
        agentScript.attackCover = false;
        agentScript.shoot = false;

        agentScript.ChangeActionText("");
    }

    // Start is called before the first frame update
    void Start()
    {
        // Getting all allied AI units.
        allyAgents = new List<GameObject>();
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < agents.Length; i++)
            allyAgents.Add(agents[i]);

        noOfAgents = allyAgents.Count;

        coverFinder = gameObject.GetComponent<CoverFinderScript>();
        movementScript = gameObject.GetComponent<MovementScript>();
        agentScript = gameObject.GetComponent<AgentScript>();
        emotion = gameObject.GetComponent<EmotionScript>();
        
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<PlayerScript>();

        decisionTimer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        Stopwatch alarmWatch = new Stopwatch();
        alarmWatch.Start();
        if (agentScript.dead)
            return;

        decisionTimer += Time.deltaTime;

        if (playerScript.attackedAgent == this.gameObject)
        {
            underAttack = true;
            alarm = true;
            emotion.AddAdrenaline(2);
        }
        if (playerScript.attackedCover == coverFinder.currentCover && coverFinder.nearCover)
        {
            coverUnderAttack = true;
            alarm = true;
            emotion.AddFear(1);
            emotion.AddAdrenaline(2);
        }
        if (agentScript.ammo <= 0)
        {
            outOfAmmo = true;
            alarm = true;
        }

        if (alarm && alarmCoolDown >= 0)
            alarmCoolDown -= Time.deltaTime;
        else
        {
            alarm = false;
            alarmActionTaken = false;
            alarmCoolDown = 5;
        }

        if (agentScript.playerVisible)
        {
            attack = true;
            alarm = true;
        }

        if(agentScript.attackCover && playerScript.playerCover == null)
        {
            agentScript.attackCover = false;
            agentScript.shoot = false;
        }

        // Stoping the loop if alarm is not on or if the not enough time elapsed to make decision
        if (alarm == false && decisionSpeed > decisionTimer)
            return;
        else
        if (decisionSpeed <= decisionTimer)
            ClearActions();

        // ALARMS
        if(underAttack)
        {
            // Checking which cover is closer to agent, previous or the chosen one
            if (coverFinder.currentCover != null && coverFinder.nextCover != null)
            {
                if (coverFinder.nearCover == false)
                {
                    if (Vector3.Distance(gameObject.transform.position, coverFinder.currentCover.transform.position) < Vector3.Distance(gameObject.transform.position, coverFinder.nextCover.transform.position))
                        returnToCover = true;
                    else
                        moveToCover = true;
                }
            }
            else
            if (coverFinder.nextCover == null)
                moveToCover = true;

            hide = true;
        }

        if (coverUnderAttack)
        {
            agentScript.ChangeActionText("Cover under attack - Hiding");
            hide = true;
            attack = false;
            movementScript.crouch = true;
        }

        if(outOfAmmo)
        {
            agentScript.ChangeActionText("Reloading");
            agentScript.reload = true;
            attackPlayer = false;
            agentScript.attackPlayer = false;
            attackPlayerCover = false;
            agentScript.attackCover = false;
            attack = false;

            hide = true;
        }

        if(coverFinder.nearCover && hide && attack == false)
        {
            if (coverFinder.currentCover != null)
            {
                if (coverFinder.currentCover.CompareTag("Small Object"))
                    movementScript.crouch = true;
            }
        }

        alarmWatch.Stop();
        //UnityEngine.Debug.LogWarning("Agent " + this.name + ": Alarms in DecisionMakingScript took " + alarmWatch.Elapsed + "ms to check alarms");

        // END OF ALARMS

        Stopwatch decisionWatch = new Stopwatch();
        decisionWatch.Start();
        // Decisions tree 1
        if (alarmActionTaken == false)
            firstTreeDecision = Random.Range(1, 11);

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

        if (alarmActionTaken == false)
        {
            secondTreeDecision = Random.Range(1, 4); // 33%
            alarmActionTaken = true;
        }

        if (move)
        {
            // Attacking only if player is visible - moving and attacking
            if (agentScript.playerVisible)
                attack = true;

            if (secondTreeDecision == 1 && coverFinder.currentCover != null)
                returnToCover = true;
            else
                if(coverFinder.nextCover != null)
                    moveToCover = true;
        }

        if (attack)
        {
            // Will try to attack the player first
            if (agentScript.playerVisible || secondTreeDecision > 1)
            {
                // Checking if player is near cover
                if (agentScript.playerVisible || (playerScript.nearCover && playerScript.crouching == false && playerScript.playerCover.CompareTag("Small Object")))
                    attackPlayer = true;
                else if (playerScript.playerCover != null && agentScript.playerCoverVisible)
                    attackPlayerCover = true;
            }
            else
            if (playerScript.playerCover != null && agentScript.playerCoverVisible)
                attackPlayerCover = true;
            else
                UnityEngine.Debug.Log("Unabel to attack");
        }

        if (move)
            defend = false;

        if (attack)
            hide = false;

        if(defend)
        {
            if (coverFinder.currentCover != null)
            {
                if (coverFinder.currentCover.CompareTag("Small Object") && coverFinder.nearCover && secondTreeDecision == 1)
                { 
                    attackFromCover = true;
                    movementScript.crouch = false;
                    hide = false;
                }
            }

            if (secondTreeDecision > 1)
                hide = true;
        }

        if (movementScript.inAir)
            hide = false;

        decisionWatch.Stop();

        //UnityEngine.Debug.LogWarning("Agent " + this.name + ": Decision making in DecisionMakingScript took " + decisionWatch.Elapsed + "ms to make decision");
        // END OF DECISIONS

        // Assigning decisions to scripts
        // AgentScript
        if (attackPlayer)
            agentScript.attackPlayer = true;

        if (attackPlayerCover)
            agentScript.attackCover = true;

        if(attackFromCover)
        {
            movementScript.crouch = false;

            if (agentScript.playerVisible)
                agentScript.attackPlayer = true;
            else 
                if(playerScript.playerCover != null)
                    agentScript.attackCover = true;
        }

        // MovementScript and CoverFinderScript
        if (hide == false)
            movementScript.crouch = false;
        else
        {
            if (coverFinder.nearCover == false)
                moveToCover = true;
            else
            { 
                movementScript.crouch = true;
                agentScript.shoot = false;
                attackPlayer = false;
                attackPlayerCover = false;
                agentScript.attackCover = false;
                agentScript.attackPlayer = false;
            }
        }

        // Don't stop moving to cover until it is reached
        if (coverFinder.nextCover != null)
            moveToCover = true;

        if (moveToCover)
        {
            if(CanIFlank())
            {
                int flankRand = Random.Range(1, 4); // 33%

                if (flankRand == 1)
                    flankPlayer = true;
                else
                    flankPlayer = false;
            }
            else
                flankPlayer = false;

            if (coverFinder.nextCover == null)
            {
                addCover = false;
                MoveToCover();
            }
        }
        else
        if (returnToCover)
        {
            returnToCover = false;
            ReturnToCover();
        }
        // END OF ASSIGNING
     }
}
