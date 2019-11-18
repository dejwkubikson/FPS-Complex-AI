using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//x -84.23, y 4.3917, z 5.2
public class AgentScript : MonoBehaviour
{
    int health = 100;
    int noOfAllies = 0;
    int fear = 50;
    int confidence = 50;
    int adrenaline = 0;
    int moveSpeed = 10;
    int decisionSpeed = 0;
    int accuracy = 0;
    int jumpSpeed = 12;

    public List<GameObject> bigObjList;
    public List<GameObject> mediumObjList;
    public List<GameObject> smallObjList;
    public List<GameObject> coversCombined;
    public List<GameObject> allyAgents;
    public GameObject player;

    public GameObject currentCover;
    public GameObject nextCover;
    public GameObject lastCollidedWith;

    public bool underAttack;
    public bool attackingPlayer;
    public bool playerVisible;
    public bool attackingPlayerCover;
    public bool coverAttack;
    public bool playerMovingTowards;
    public bool flankingPlayer;
    public bool moveToCover;
    public bool crouching;
    public bool standing;
    public bool hiding;
    public bool collidedNxtCover;
    public bool jump;

    public bool move;
    public bool hold;

    public ParticleSystem bloodParticle;
    public ParticleSystem shootingParticle;
    public TextMeshPro actionText;

    public Vector3 currentPos;

    List<Vector3> movePathList;
    

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

        // Getting all needed child objects.
        actionText = transform.GetChild(2).gameObject.GetComponent<TextMeshPro>();
        bloodParticle = transform.GetChild(3).gameObject.GetComponent<ParticleSystem>();
        shootingParticle = transform.GetChild(4).gameObject.GetComponent<ParticleSystem>();

        // Getting all allied AI units.
        allyAgents = new List<GameObject>();
        objs = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < objs.Length; i++)
            allyAgents.Add(objs[i]);

        // Getting player object.
        player = GameObject.FindGameObjectWithTag("Player");

        movePathList = new List<Vector3>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(this.name + " collided with: " + collision.collider.name);

        // If collided with the cover that the agent was supposed to hide behind.
        if (collision.gameObject == nextCover)
        {
            moveToCover = false;
            currentCover = nextCover;
            nextCover = null;
            return;
        }

        // Jump on small object
        /*if (collision.gameObject.CompareTag("Small Object"))
        {
            JumpOnObject(collision.gameObject);
        }*/
    }

    private void OnCollisionExit(Collision collision)
    {
        //Debug.Log(this.name + " no more colliding with: " + collision.collider.name);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.name != "Floor")
        {
            Debug.Log(this.name + " staying in collision with: " + collision.collider.name);

            if (collision.gameObject == lastCollidedWith)
            {
                if (moveToCover)
                {
                    AvoidObject(collision.gameObject);
                }
            }
            else
                lastCollidedWith = collision.gameObject;
        }
    }

    private GameObject GetClosestCoverToHide()
    {
        GameObject closestCover = null;
        float distance = Mathf.Infinity;
        //float distanceToPlayer = Mathf.Infinity;

        for (int i = 0; i < coversCombined.Count; i++)
        {
            // Checking if this is not the current cover
            if (coversCombined[i] == currentCover)
                continue;

            // Getting distance to cover
            float distToObj = Vector3.Distance(gameObject.transform.position, coversCombined[i].transform.position);
            // Getting cover's distance to player
            float coverDistToPlayer = Vector3.Distance(coversCombined[i].transform.position, player.transform.position);
            
            // Checking if this is the closest cover to agent
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
        
        return closestCover;
    }

    private void MoveToObject(GameObject objectToMoveTo)
    {
        actionText.text = "Moving to cover";

        Vector3 objectPos = objectToMoveTo.transform.position;
        
        //transform.position = Vector3.Lerp(currPos, objectPos, Time.deltaTime);
        transform.position = Vector3.MoveTowards(currentPos, objectPos, Time.deltaTime * moveSpeed);
    }

    private void AvoidObject(GameObject objectToAvoid)
    {
        actionText.text = "Avoiding object";
        Debug.Log("avoiding " + objectToAvoid);

        bool objOnRight = false;

        int pathPick = 1;
        if (objectToAvoid.CompareTag("Small Object"))
            pathPick = Random.Range(1, 3);
        else
            pathPick = 1;
            
        switch(pathPick)
        {
            case 1:
                Debug.Log("case1");
                // Avoid by going left or right. Checking if there's an object near.
                for(int i = 0; i < coversCombined.Count; i++)
                {
                    if (Vector3.Distance(objectToAvoid.transform.position, coversCombined[i].transform.position) <= 10 && objectToAvoid != coversCombined[i])
                    {
                        //Debug.Log("Object next to " + objectToAvoid.name  + " is " + coversCombined[i].name);

                        if (objectToAvoid.transform.position.z > coversCombined[i].transform.position.z)
                            objOnRight = true;
                    }
                }

                if (objOnRight)
                    MoveLeft();
                else
                    MoveRight();

                break;

            case 2:
                // Avoid by jumping over
                break;

            default:
                break;
        }
    }

    private void MoveForward()
    {
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.forward * moveSpeed;
    }

    private void MoveRight()
    {
        Debug.Log("Move right");
        //transform.position = Vector3.Lerp(currentPos, new Vector3(currentPos.x, currentPos.y, currentPos.z - 5), Time.deltaTime * 3);
        movePathList.Insert(0, new Vector3(currentPos.x, currentPos.y, currentPos.z - 5));
    }

    private void MoveLeft()
    {
        Debug.Log("Move left");
        Vector3.Lerp(currentPos, new Vector3(currentPos.x, currentPos.y, currentPos.z + 5), Time.deltaTime);
    }

    private void JumpOnObject(GameObject objectToJumpOn)
    {
        Vector3 desiredPos = new Vector3(currentPos.x, currentPos.y + objectToJumpOn.GetComponent<BoxCollider>().bounds.size.y, currentPos.z);

        transform.position = Vector3.Lerp(currentPos, desiredPos, 1);
    }

    private void MoveToNextPoint()
    {
        if(movePathList.Count > 0)
            transform.position = Vector3.MoveTowards(currentPos, movePathList[0], Time.deltaTime * moveSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        currentPos = gameObject.transform.position;

        Debug.Log(GetClosestCoverToHide().name);
        if(lastCollidedWith != null)
        Debug.Log(lastCollidedWith.name);

        if(jump)
        {
            //JumpOnObject();
        }

        if (move)
        {
            if (moveToCover)
            {
                nextCover = GetClosestCoverToHide();
                movePathList.Add(nextCover.transform.position);
                moveToCover = false;
                //MoveToObject(nextCover);
                //MoveLeft();
            }

            MoveToNextPoint();
        }

    }

}
