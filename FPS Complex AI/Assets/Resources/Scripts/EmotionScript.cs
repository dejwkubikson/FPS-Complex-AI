using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionScript : MonoBehaviour
{
    public int fear = 50;
    public int confidence = 50;
    public int adrenaline = 0;

    private bool emotional = false;
    private bool confident = false;
    private bool fearful = false;
    private bool onAdrenaline = false;

    private bool fearCool = false;
    private bool adrenalineCool = false;

    private bool addingFear = false;
    private bool addingAdrenaline = false;

    private int emotionCoolDown = 5;

    DecisionMakingScript decisionMaking;
    MovementScript movementScript;
    AgentScript agentScript;

    public States currentState;

    public enum States
    {
        confidentState,
        fearfulState,
        rageState,
        determinedState,
        normalState
    }

    // High confidence - 80%+
    public void Confident()
    {
        currentState = States.confidentState;
        decisionMaking.decisionSpeed = 5;
        movementScript.movementSpeed = 15;
        agentScript.accuracy = 65;
        agentScript.shootCoolDown = 0.8f;
    }

    // High fear - 80%+
    public void Fearful()
    {
        currentState = States.fearfulState;
        decisionMaking.decisionSpeed = 15;
        movementScript.movementSpeed = 5;
        agentScript.accuracy = 5;
        agentScript.shootCoolDown = 0.2f; // full auto due to fear
    }

    // High fear and adrenaline - 80%+
    public void Rage()
    {
        currentState = States.rageState;
        decisionMaking.decisionSpeed = 3;
        movementScript.movementSpeed = 20;
        agentScript.accuracy = 3;
        agentScript.shootCoolDown = 0.2f; // full auto due to fear
    }

    // High confidence and adrenaline - 80%+
    public void Determinated()
    {
        currentState = States.determinedState;
        decisionMaking.decisionSpeed = 4;
        movementScript.movementSpeed = 15;
        agentScript.accuracy = 50;
        agentScript.shootCoolDown = 0.4f;
    }

    public void Normal()
    {
        currentState = States.normalState;
        decisionMaking.decisionSpeed = 7;
        movementScript.movementSpeed = 10;
        agentScript.accuracy = 30;
        agentScript.shootCoolDown = 1;
    }

    public void AddFear(int amount)
    {
        if (addingFear == false)
        {
            fear += amount;
            StartCoroutine(AddFearOnce());
        }
    }

    public void AddAdrenaline(int amount)
    {
        if (addingAdrenaline == false)
        {
            adrenaline += amount;
            StartCoroutine(AddAdrenalineOnce());
        }
    }

    IEnumerator AddFearOnce()
    {
        addingFear = true;
        yield return new WaitForSeconds(3);
        addingFear = false;
    }

    IEnumerator AddAdrenalineOnce()
    {
        addingAdrenaline = true;
        yield return new WaitForSeconds(3);
        addingAdrenaline = false;
    }

    IEnumerator FearCoolDown()
    {
        fearCool = true;
        yield return new WaitForSeconds(emotionCoolDown);
        fear -= 1;
        confidence = 100 - fear;
        fearCool = false;
    }

    IEnumerator AdrenalineCoolDown()
    {
        adrenalineCool = true;
        yield return new WaitForSeconds(emotionCoolDown);
        adrenaline -= 1;
        adrenalineCool = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        decisionMaking = gameObject.GetComponent<DecisionMakingScript>();
        movementScript = gameObject.GetComponent<MovementScript>();
        agentScript = gameObject.GetComponent<AgentScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fearCool == false && fear > 0)
            StartCoroutine(FearCoolDown());

        if (adrenalineCool == false && adrenaline > 0)
            StartCoroutine(AdrenalineCoolDown());

        if (confidence > 80)
            confident = true;
        else
            confident = false;

        if (fear > 80)
            fearful = true;
        else
            fearful = false;

        if (adrenaline > 80)
            onAdrenaline = true;
        else
            onAdrenaline = false;

        if(fearful)
        {
            if (fearful && onAdrenaline)
                Rage();
            else
                Fearful();

            emotional = true;
        }

        if(confident)
        {
            if (confident && onAdrenaline)
                Determinated();
            else
                Confident();

            emotional = true;
        }

        if (confident == false && fearful == false)
            emotional = false;

        if (emotional == false)
            Normal();
    }
}
