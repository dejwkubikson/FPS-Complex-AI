using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    int health = 100;
    int damage = 10;
    public GameObject attackedCover;
    public float shootCoolDown = 0.1f;
    public float shootingTime = 0.0f;
    public int attackCoolDown = 3;
    public float accuracy = 30;
    public bool crouching;
    public bool nearCover;
    public GameObject playerCover;

    AudioClip shotSound;
    AudioClip reloadSound;
    AudioClip hitSound;

    AudioSource audioSource;

    private int clipAmount = 30;
    private int ammo = 30;
    private bool reloading = false;
    private bool shooting = false;

    public ParticleSystem bloodParticle;
    public ParticleSystem shootingParticle;
    public Text ammoText;

    public List<GameObject> coversCombined;
    IEnumerator Shoot()
    {
        shooting = true;
        shootingParticle.Play();
        audioSource.PlayOneShot(shotSound);
        ammo--;
        ammoText.text = ammo + " / 30";

        yield return new WaitForSeconds(shootCoolDown);
        shooting = false;
    }

    IEnumerator Reload()
    {
        Debug.Log("Player reloading.");
        reloading = true;

        audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(reloadSound.length);

        ammo = clipAmount;
 
        reloading = false;

        ammoText.text = ammo + " / 30";
    }

    // More efficient to make agents check player's script for attacked cover rather than
    // checking each agent's current cover
    IEnumerator AttackingCover (GameObject cover)
    {
        attackedCover = cover;
        yield return new WaitForSeconds(attackCoolDown);
        attackedCover = null;
    }

    public void CheckIfNearCover()
    {
        nearCover = false;
        for(int i = 0; i < coversCombined.Count; i++)
        {
            if (Vector3.Distance(transform.position, coversCombined[i].transform.position) <= 10)
            {
                nearCover = true;
                playerCover = coversCombined[i];
                //Debug.Log("Object next to " + objectToAvoid.name  + " is " + coversCombined[i].name);
            }
        }

        if (nearCover == false)
            playerCover = null;       
    }

    public void ReceiveDamage(int amount)
    {
        bloodParticle.Play();
        audioSource.PlayOneShot(hitSound);
        health -= amount;
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        shotSound = Resources.Load<AudioClip>("Sounds/gunshot_sound");
        reloadSound = Resources.Load<AudioClip>("Sounds/reload_sound");
        hitSound = Resources.Load<AudioClip>("Sounds/hit_sound");

        // Getting all needed child objects.
        ammoText = GameObject.Find("AmmoText").GetComponent<Text>();
        bloodParticle = transform.GetChild(3).gameObject.GetComponent<ParticleSystem>();
        shootingParticle = transform.GetChild(4).gameObject.GetComponent<ParticleSystem>();

        // Big objects
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Big Object");
        for (int i = 0; i < objs.Length; i++)
        {
            coversCombined.Add(objs[i]);
        }

        // Small objects
        objs = GameObject.FindGameObjectsWithTag("Small Object");
        for (int i = 0; i < objs.Length; i++)
        {
            coversCombined.Add(objs[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfNearCover();

        if (Input.GetMouseButton(0))
        {
            shootingTime += Time.deltaTime;
            if (ammo > 0 && reloading == false && shooting == false)
            {
                if (accuracy - (shootingTime / 2) > 5)
                    accuracy -= (shootingTime / 2);
                else
                    accuracy = 5;

                StartCoroutine(Shoot());
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        Debug.Log(hit.collider.gameObject.name);

                        GameObject hitObject = hit.collider.gameObject;
                        if (hitObject.CompareTag("Enemy"))
                        {
                            AgentScript agentScript = hitObject.GetComponent<AgentScript>();
                            if (agentScript == null)
                                return;

                            int random = Random.Range(0, 101);
                            if (random <= accuracy)
                                agentScript.ReceiveDamage(damage);
                        }
                        else
                        if (hitObject.CompareTag("Big Object") || hitObject.CompareTag("Small Object"))
                        {
                            //if(attackedCover != hitObject.gameObject)
                            //{
                            StartCoroutine(AttackingCover(hitObject.gameObject));
                            //}
                        }
                    }
                }
            }
        }
        else
        { 
            shootingTime = 0.0f;
            accuracy = 30;
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R pressed, ammo " + ammo + " reloading " + reloading);
            if (reloading == false && ammo < clipAmount)
                StartCoroutine(Reload());
        }

        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            if(crouching)
            {
                crouching = false;
                gameObject.transform.localScale = new Vector3(2.77f, 4.22f, 3.06f);
            }
            else
            {
                crouching = true;
                gameObject.transform.localScale = new Vector3(2.77f, 3.0f, 3.06f);
            }
        }
    }
}
