using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//x -84.23, y 4.3917, z 5.2
public class AgentScript : MonoBehaviour
{
    public int health = 100;
    int noOfAllies = 0;
    int accuracy = 0;

    public bool test = false;
    public bool dead;

    public List<GameObject> allyAgents;
    public GameObject player;

    public ParticleSystem bloodParticle;
    public ParticleSystem shootingParticle;
    public TextMeshPro actionText;

    public Vector3 currentPos;
   
    public bool IsCoverInUse(GameObject cover)
    {
        for(int i = 0; i < allyAgents.Count; i++)
        {
            CoverFinderScript coverFinder = allyAgents[i].GetComponent<CoverFinderScript>();
            if (coverFinder.currentCover == cover)
                return true;
        }
        
        return false;
    }

    public void ReceiveDamage(int amount)
    {
        bloodParticle.Play();
        health -= amount;
    }

    private void Die()
    {
        // Disabling all scripts
        gameObject.GetComponent<DecisionMakingScript>().enabled = false;
        gameObject.GetComponent<MovementScript>().enabled = false;
        gameObject.GetComponent<CoverFinderScript>().enabled = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        gameObject.transform.position = new Vector3(currentPos.x, 0.88f, currentPos.z);
        gameObject.transform.eulerAngles = new Vector3(180, 90, 0);

        dead = true;
        // @@@ Need to think if want to keep the 'dead' body or not
        //StartCoroutine(WaitAndDestroy());
    }

    IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(4);
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Getting all needed child objects.
        actionText = transform.GetChild(2).gameObject.GetComponent<TextMeshPro>();
        bloodParticle = transform.GetChild(3).gameObject.GetComponent<ParticleSystem>();
        shootingParticle = transform.GetChild(4).gameObject.GetComponent<ParticleSystem>();

        // Getting all allied AI units.
        allyAgents = new List<GameObject>();
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < agents.Length; i++)
            allyAgents.Add(agents[i]);

        // Getting player object.
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if(!(dead))
            currentPos = gameObject.transform.position;

        if(test)
        {
            ReceiveDamage(50);
            test = false;
        }

        if (health <= 0)
        {
            if (dead == false)
                Die();
        }
    }

}
