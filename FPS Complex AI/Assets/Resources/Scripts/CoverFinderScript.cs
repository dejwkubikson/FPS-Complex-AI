using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverFinderScript : MonoBehaviour
{
    DecisionMakingScript decisionScript;
    
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
    public bool movingToCover = false;

    MovementScript movementScript;

    private void OnCollisionEnter(Collision collision)
    {
        // If collided with the cover that the agent was supposed to hide behind.
        if(collision.gameObject == nextCover)
        {
            movementScript.RemoveFirstPoint();
            currentCover = nextCover;
            nextCover = null;
            visitedCovers.Add(currentCover);
            movingToCover = false;
            decisionScript.moveToCover = false;
            return;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.collider.name != "Floor")
        {
            //Debug.Log(this.name + " staying in collision with " + collision.collider.name);

            if (collision.gameObject == lastCollidedWith && movementScript.inAir == false)
            {
                inCollisionTime += Time.deltaTime;

                if (movingToCover)
                {
                    if (inCollisionTime > 0.3f)
                    {
                        inCollisionTime = 0.0f;
                        AvoidObject(collision.gameObject);
                    }
                }
                else
                {
                    if(inCollisionTime > 0.3f)
                    {
                        inCollisionTime = 0.0f;
                        movementScript.MoveBack(3);
                    }
                }
            }
            else
            {
                inCollisionTime = 0.0f;
                lastCollidedWith = collision.gameObject;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //Debug.Log(this.name + " no more colliding with " + collision.collider.name);
        inCollisionTime = 0.0f;
    }

    // When colliding with an object constantly the agent will try to avoid it by gettin aroud it or if possible, jumping over.
    public void AvoidObject(GameObject objectToAvoid)
    {
        // @@@ actionText.text = "Avoiding object";
        //Debug.Log("Avoiding " + objectToAvoid);

        bool objOnRight = false;

        int pathPick = 1;
        if (objectToAvoid.CompareTag("Small Object"))
            pathPick = Random.Range(1, 3);
        
        switch(pathPick)
        {
            case 1:
                // Avoid by going left or right. Checking if there's an object near.
                //Debug.Log("Avoid by going left or right.");
                for(int i = 0; i < coversCombined.Count; i++)
                {
                    if(Vector3.Distance(objectToAvoid.transform.position, coversCombined[i].transform.position) <= 10 && objectToAvoid != coversCombined[i])
                    {
                        //Debug.Log("Object next to " + objectToAvoid.name  + " is " + coversCombined[i].name);
                        if (objectToAvoid.transform.position.z > coversCombined[i].transform.position.z)
                            objOnRight = true;
                    }
                }

                if (objOnRight)
                    movementScript.MoveLeft(7);
                else
                    movementScript.MoveRight(7);

                break;

            case 2:
                // Avoid by jumping over.
                //Debug.Log("Avoiding by jumping.");
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

                if(agentDistToPlayer > coverDistToPlayer)
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

        decisionScript = gameObject.GetComponent<DecisionMakingScript>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(GetClosestCoverToHide().name);
    }
}
