using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementScript : MonoBehaviour
{
    AgentScript agentScript;

    public int movementSpeed = 10;

    public List<Vector3> movePathList;
    public Vector3 currentPos;
    public Quaternion currentRot;

    public bool move = true;
    public bool jump;
    public bool inAir = false;
    public bool crouch;

    private float inAirTime;

    public void JumpOnObject(GameObject objectToJumpOn)
    {
        agentScript.ChangeActionText("Jumping on object");
        Vector3 desiredPos = new Vector3(currentPos.x, currentPos.y + objectToJumpOn.GetComponent<BoxCollider>().bounds.size.y + 2, currentPos.z);

        transform.position = Vector3.Lerp(currentPos, desiredPos, 1);
        movePathList.Insert(0, transform.position + new Vector3(3, 0, 0));
    }

    public void MoveToPoint()
    {
        if (movePathList.Count == 0)
        {
            //Debug.LogWarning("Trying to move to first point but it doesn't exist!");
            return;
        }

        Vector3 moveToPoint = movePathList[0];

        //Debug.Log(Vector3.Distance(currentPos, moveToPoint) + " TO " + moveToPoint);

        if (Vector3.Distance(currentPos, moveToPoint) <= 0)
            RemoveFirstPoint();
        else
            if (Vector3.Distance(currentPos, moveToPoint) <= 6 && currentPos.y + 3 < moveToPoint.y)
            RemoveFirstPoint();
        else
            transform.position = Vector3.MoveTowards(currentPos, moveToPoint, Time.deltaTime * movementSpeed);
    }

    public void MoveLeft(int amount)
    {
        movePathList.Insert(0, new Vector3(currentPos.x, currentPos.y, currentPos.z + amount));
    }

    public void MoveRight(int amount)
    {
        movePathList.Insert(0, new Vector3(currentPos.x, currentPos.y, currentPos.z - amount));
    }

    public void MoveBack(int amount)
    {
        movePathList.Insert(0, new Vector3(currentPos.x - amount, currentPos.y, currentPos.z));
    }

    public void MoveForward(int amount)
    {
        movePathList.Insert(0, new Vector3(currentPos.x + amount, currentPos.y, currentPos.z));
    }

    public void RemoveFirstPoint()
    {
        if (movePathList.Count > 0)
            movePathList.RemoveAt(0);
        //else
         //   Debug.LogWarning("RemoveFirstPoint() tried to remove point from path but none exist.");
    }

    public void AddPointToList(Vector3 position)
    {
        bool found = false;
        // Adding only if the point doesn't already exist in the list
        for(int i = 0; i < movePathList.Count; i++)
        {
            if (movePathList[i] == position)
                found = true;
        }

        if (found == false)
            movePathList.Add(position);
    }

    // Start is called before the first frame update
    void Start()
    {
        movePathList = new List<Vector3>();

        agentScript = gameObject.GetComponent<AgentScript>();

        move = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (agentScript.dead)
            return;

        if (movePathList.Count > 3)
            movePathList.Clear();

        currentPos = gameObject.transform.position;
        currentRot = gameObject.transform.rotation;

        gameObject.transform.eulerAngles = new Vector3(-90, 90, 0);

        if (move)
            MoveToPoint();

        if (currentPos.y > 8)
            inAir = true;
        else
            inAir = false;

        if(inAir)
            inAirTime += Time.deltaTime;

        if (inAirTime > 3)
        {
            AddPointToList(currentPos + new Vector3(4, 0, 0));
            inAirTime = 0;
        }

        if (crouch)
            gameObject.transform.localScale = new Vector3(9, 9, 6);
        else
            gameObject.transform.localScale = new Vector3(9, 9, 9);
    }
}
