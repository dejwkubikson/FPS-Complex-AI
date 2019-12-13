using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverFinderScript : MonoBehaviour
{
    DecisionMakingScript decisionMaking;
    AgentScript agentScript;
    
    public List<GameObject> bigObjList;
    public List<GameObject> smallObjList;
    public List<GameObject> coversCombined;
    public List<GameObject> visitedCovers;
    public GameObject currentCover;
    public GameObject nextCover;
    public GameObject lastCollidedWith;

    public GameObject player;
    public float inCollisionTime = 0.0f;

    public bool collidedNxtCover;
    public bool nearCover = false;

    MovementScript movementScript;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.name == "Floor")
            return;

        if (collision.gameObject.name == "Map border")
        {
            Debug.Log(gameObject.name + " map border collision calling RemoveFirstPoint()");
            movementScript.RemoveFirstPoint();
        }

        // Debug.Log("Collided with " + collision.gameObject.name);

        // If collided with the cover that the agent was supposed to hide behind.
        if (nextCover != null)
        {
            if (collision.gameObject == nextCover || Vector3.Distance(nextCover.transform.position, transform.position) < 5)
            {
                Debug.Log(gameObject.name + " collision calling RemoveFirstPoint()");
                movementScript.RemoveFirstPoint();
                currentCover = nextCover;
                nextCover = null;
                if (visitedCovers.Contains(currentCover) == false)
                    visitedCovers.Add(currentCover);
                decisionMaking.moveToCover = false;
                agentScript.ChangeActionText("Reached cover");
                return;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.collider.name == "Floor")
            return;

        //Debug.Log(this.name + " staying in collision with " + collision.collider.name);

        inCollisionTime += Time.deltaTime;

        if (nextCover != null)
        {
            if (collision.gameObject == nextCover)
            {
                Debug.Log(gameObject.name + " collision stay calling RemoveFirstPoint()");
                movementScript.RemoveFirstPoint();
                currentCover = nextCover;
                nextCover = null;
                if (visitedCovers.Contains(currentCover) == false)
                    visitedCovers.Add(currentCover);
                decisionMaking.moveToCover = false;
            }
        }

        if (movementScript.inAir == false && inCollisionTime >= 3)
        {
            inCollisionTime = 0;
            AvoidObject(collision.gameObject);
        } 
        /*
        if (collision.gameObject == lastCollidedWith && movementScript.inAir == false)
        {
            Debug.Log("1");
            inCollisionTime += Time.deltaTime;

            if (movingToCover)
            {
                Debug.Log("2");
                if (inCollisionTime > 0.3f)
                {
                    Debug.Log("3");
                    inCollisionTime = 0.0f;
                    if (nextCover != null)
                    {
                        Debug.Log("4");
                        if (collision.gameObject == nextCover)
                        {
                            Debug.Log("5");
                            movementScript.RemoveFirstPoint();
                            currentCover = nextCover;
                            nextCover = null;
                            if (visitedCovers.Contains(currentCover) == false)
                                visitedCovers.Add(currentCover);
                            movingToCover = false;
                            decisionMaking.moveToCover = false;
                            movementScript.MoveBack(3);
                        }
                    }
                    else
                    {
                        Debug.Log("6");
                        AvoidObject(collision.gameObject);
                    }
                }
            }
            else
            {
                Debug.Log("7");
                if (inCollisionTime > 0.3f)
                {
                    Debug.Log("8");
                    inCollisionTime = 0.0f;
                    movementScript.MoveBack(3);
                }
            }
        }
        else
        {
            movementScript.MoveBack(1);
            inCollisionTime = 0.0f;
            lastCollidedWith = collision.gameObject;
        }*/
        
    }

    private void OnCollisionExit(Collision collision)
    {
        //Debug.Log(this.name + " no more colliding with " + collision.collider.name);
        if(collision.gameObject.name != "Floor")
            inCollisionTime = 0.0f;
    }

    // When colliding with an object constantly the agent will try to avoid it by gettin aroud it or if possible, jumping over.
    public void AvoidObject(GameObject objectToAvoid)
    {
        agentScript.ChangeActionText("Avoiding object");
        Debug.Log("I, " + gameObject.name +" avoiding " + objectToAvoid);

        bool objOnRight = false;
        bool objOnLeft = false;

        int pathPick = 1;
        if (objectToAvoid.CompareTag("Small Object") && movementScript.crouch == false)
            pathPick = Random.Range(1, 3);
        
        switch(pathPick)
        {
            case 1:
                // Avoid by going left or right. Checking if there's an object near.
                // Debug.Log("Avoid by going left or right.");
                for(int i = 0; i < coversCombined.Count; i++)
                {
                    if (Vector3.Distance(objectToAvoid.transform.position, coversCombined[i].transform.position) <= 10 && objectToAvoid != coversCombined[i])
                    {
                        //Debug.Log("Object next to " + objectToAvoid.name  + " is " + coversCombined[i].name);
                        if (objectToAvoid.transform.position.z > coversCombined[i].transform.position.z)
                            objOnRight = true;
                    }

                    if (Vector3.Distance(objectToAvoid.transform.position, coversCombined[i].transform.position) <= 10 && objectToAvoid != coversCombined[i])
                    {
                        //Debug.Log("Object next to " + objectToAvoid.name  + " is " + coversCombined[i].name);
                        if (objectToAvoid.transform.position.z < coversCombined[i].transform.position.z)
                            objOnLeft = true;
                    }
                }

                if(objOnLeft == false && objOnRight == false)
                {
                    int decision = Random.Range(1, 3);
                    if (decision == 1)
                    {
                        // If object is behind the agent and the agent is trying to get past it to return to cover
                        if (objectToAvoid.transform.position.x < gameObject.transform.position.x && decisionMaking.returnToCover)
                        {
                            // Reverse order as the Move() functions inserts point at the begining of the movement list.
                            movementScript.MoveForward(-12);
                            movementScript.MoveLeft(10);
                        }
                        else
                        {
                            movementScript.MoveForward(12);
                            movementScript.MoveLeft(12);
                            movementScript.MoveForward(-3);
                        }
                    }
                    else
                    {
                        if (objectToAvoid.transform.position.x < gameObject.transform.position.x && decisionMaking.returnToCover)
                        {
                            // Reverse order as the Move() functions inserts point at the begining of the movement list.
                            movementScript.MoveForward(-12);
                            movementScript.MoveRight(12);
                        }
                        else 
                        { 
                            movementScript.MoveForward(12);
                            movementScript.MoveRight(12);
                            movementScript.MoveForward(-3);
                        }
                    }
                }
                else
                if (objOnRight)
                {
                    if (objectToAvoid.transform.position.x < gameObject.transform.position.x && decisionMaking.returnToCover)
                    {
                        movementScript.MoveRight(12);
                        movementScript.MoveForward(-12);
                    }
                    else
                    {
                        movementScript.MoveForward(12);
                        movementScript.MoveLeft(12);
                        movementScript.MoveForward(-3);
                    }
                }
                else if(objOnLeft)
                {
                    if (objectToAvoid.transform.position.x < gameObject.transform.position.x && decisionMaking.returnToCover)
                    {
                        movementScript.MoveRight(12);
                        movementScript.MoveForward(-12);
                    }
                    else
                    {
                        movementScript.MoveForward(-12);
                        movementScript.MoveLeft(12);
                        movementScript.MoveForward(-3);
                    }
                }

                break;

            case 2:
                // Avoid by jumping over.
                // Debug.Log("Avoiding by jumping.");
                movementScript.JumpOnObject(objectToAvoid);

                break;

            default:
                break;
        }
    }

    public GameObject GetClosestCoverToHide()
    {
        GameObject closestCover = null;
        float distance = Mathf.Infinity;

        for(int i = 0; i < coversCombined.Count; i++)
        {
            // Checking if this is not the current cover
            if (coversCombined[i] == currentCover)
                continue;

            if (decisionMaking.IsCoverInUse(coversCombined[i]))
                continue;

            bool breakLoop = false;

            // Don't go if previously visited
            for(int x = 0; x < visitedCovers.Count; x++)
            {
                if (visitedCovers[x] == coversCombined[i])
                    breakLoop = true;
            }

            if (breakLoop)
                continue;

            // Getting distance to cover
            float distToObj = Vector3.Distance(gameObject.transform.position, coversCombined[i].transform.position);
            // Getting cover's distance to player
            float coverDistToPlayer = Vector3.Distance(coversCombined[i].transform.position, player.transform.position);

            // Checking if this is the closest cover to the agent
            if(distToObj < distance)
            {
                // Checking if this cover will make the agent come closer to player
                // Getting current agent's distance to player
                float agentDistToPlayer = Vector3.Distance(gameObject.transform.position, player.transform.position);

                // If the agent is supposed to flank the player
                if(decisionMaking.flankPlayer)
                {
                    if(coversCombined[i].transform.position.z > player.transform.position.z + 20 || coversCombined[i].transform.position.z < player.transform.position.z - 20)
                    {
                        // If the cover will be closer to player on X axis
                        if(coversCombined[i].transform.position.x > gameObject.transform.position.x)
                        {
                            distance = distToObj;
                            closestCover = coversCombined[i];
                        }
                    }
                }
                else if(agentDistToPlayer > coverDistToPlayer)
                {
                    distance = distToObj;
                    closestCover = coversCombined[i];
                }
            }
        }

        // There's no cover closer to the player. Stay with the current one
        //if (closestCover == null)
        //    closestCover = currentCover;
        nextCover = closestCover;

        if (closestCover == null)
            closestCover = currentCover;

        return closestCover;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Big objects - objects that the agent can hide behind standing.
        bigObjList = new List<GameObject>();
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Big Object");
        for (int i = 0; i < objs.Length; i++)
        {
            bigObjList.Add(objs[i]);
            coversCombined.Add(objs[i]);
        }

        // Small objects - objects that the agent can cover behind crouching.
        smallObjList = new List<GameObject>();
        objs = GameObject.FindGameObjectsWithTag("Small Object");
        for (int i = 0; i < objs.Length; i++)
        {
            smallObjList.Add(objs[i]);
            coversCombined.Add(objs[i]);
        }

        visitedCovers = new List<GameObject>();

        movementScript = gameObject.GetComponent<MovementScript>();

        player = GameObject.FindGameObjectWithTag("Player");

        decisionMaking = gameObject.GetComponent<DecisionMakingScript>();

        agentScript = gameObject.GetComponent<AgentScript>();
    }

    private void Update()
    {
        if (nextCover != null)
        {
            /*if (Vector3.Distance(gameObject.transform.position, nextCover.transform.position) < 3)
            {
                nearCover = true;
                currentCover = nextCover;
                nextCover = null;
                decisionMaking.moveToCover = false;
                if (visitedCovers.Contains(currentCover) == false)
                    visitedCovers.Add(currentCover);

                agentScript.ChangeActionText("Reached cover");

                Debug.Log("Update calling RemoveFirstPoint()");
                movementScript.RemoveFirstPoint();
            }*/
        }

        if (currentCover != null)
        {
            if (Vector3.Distance(gameObject.transform.position, currentCover.transform.position) < 3)
                nearCover = true;
        }
        else
            nearCover = false;
    }
}
