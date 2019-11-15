using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentScript : MonoBehaviour
{
    int health = 100;
    int noOfAllies = 0;
    int fear = 50;
    int confidence = 50;
    int adrenaline = 0;
    int moveSpeed = 0;
    int decisionSpeed = 0;
    int accuracy = 0;

    Vector3 playerPos;
    List<Vector3> allyPosList;
    List<GameObject> bigObjList;
    List<GameObject> mediumObjList;
    List<GameObject> smallObjList;

    GameObject currentCover;
    GameObject nextCover;

    bool underAttack;
    bool attackingPlayer;
    bool attackingPlayerCover;
    bool coverAttack;
    bool flankingPlayer;
    bool movingToCover;
    bool crouching;
    bool standing;
    bool hiding;
         
    // Start is called before the first frame update
    void Start()
    {
        // Big objects - objects that the agent can hide behind standing.
        bigObjList = new List<GameObject>();
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Big Object");
        for (int i = 0; i < objs.Length; i++)
            bigObjList.Add(objs[i]);

        // Medium objects - objects that the agent can hide behind crouching.
        mediumObjList = new List<GameObject>();
        objs = GameObject.FindGameObjectsWithTag("Medium Object");
        for (int i = 0; i < objs.Length; i++)
            mediumObjList.Add(objs[i]);

        // Small objects - objects that the agent can't cover behind but can jump on.
        smallObjList = new List<GameObject>();
        objs = GameObject.FindGameObjectsWithTag("Small Object");
        for (int i = 0; i < objs.Length; i++)
            smallObjList.Add(objs[i]);

        // Getting all needed child objects.

        // Getting all allied AI units.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
