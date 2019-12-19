using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;

//x -84.23, y 4.3917, z 5.2
public class AgentScript : MonoBehaviour
{
    public bool playerVisible;
    public bool playerCoverVisible;
    public int health = 100;
    public int damage = 5;
    public int accuracy = 30; // percentage of bullets that may hit the player - 30% at normal behaviour

    public bool dead;

    public GameObject player;
    public PlayerScript playerScript;

    public ParticleSystem bloodParticle;
    public ParticleSystem shootingParticle;
    public TextMeshPro actionText;
    EmotionScript emotion;

    public Vector3 currentPos;

    public bool shoot;
    public bool reload;
    public float shootCoolDown = 1;
    private float currentCoolDown = 0;
    private bool healthCoolDown = false;

    AudioClip shotSound;
    AudioClip reloadSound;
    AudioClip hitSound;

    AudioSource audioSource;

    private int clipAmount = 30;
    public int ammo = 30;
    private bool reloading = false;
    public bool attackCover;
    public bool attackPlayer;

    // Returns true if player is visible or false when not
    private bool PlayerVisible()
    {
        // Getting the planes from player's camera
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(GameObject.Find("FirstPersonCharacter").GetComponent<Camera>());
        // Checking if the agent's collider is within the frustum
        if (GeometryUtility.TestPlanesAABB(planes, gameObject.GetComponent<Collider>().bounds))
        {
            RaycastHit hit;
            // Checking if a Raycast will reach the player
            if (Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit))
            {
                if (hit.transform.gameObject == player)
                    return true;
                else
                {
                    if (hit.transform.gameObject.CompareTag("Small Object") && playerScript.playerCover == hit.transform.gameObject)
                    {
                        playerCoverVisible = true;

                        if (playerScript.crouching == false)
                            return true;
                        
                        else
                            return false;
                        
                    }
                    else
                    {
                        // Debug.Log("Another object seperating me from player: " + hit.transform.gameObject.name);
                        if (hit.transform.gameObject == playerScript.playerCover)
                            playerCoverVisible = true;
                        else
                            playerCoverVisible = false;

                        return false;
                    }
                }
            }
            else
                return false;
        }
        else
            return false;
    }

    public void ChangeActionText(string text)
    {
        actionText.text = text;
    }

    // When hit by player receiving damage.
    public void ReceiveDamage(int amount)
    {
        emotion.AddFear(2);

        bloodParticle.Play();
        if (dead == false)
        {
            audioSource.PlayOneShot(hitSound);
            health -= amount;
        }
    }

    // Stopping the agent when dead.
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

    // Shooting
    public void Shoot()
    {
        shootingParticle.Play();
        audioSource.PlayOneShot(shotSound);
        currentCoolDown = 0.0f;
        ammo--;

        if(attackCover)
        {
            if (playerScript.playerCover != null)
            {
                if (playerScript.playerCover.transform.parent.name == "Crates")
                {
                    playerScript.playerCover.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
                    playerScript.playerCover.GetComponent<AudioSource>().Play();
                }
            }
        }
        else
        if(attackPlayer && playerVisible)
        {
            int random = Random.Range(0, 101);
            if (random <= accuracy)
            {
                emotion.AddFear(-1);
                player.GetComponent<PlayerScript>().ReceiveDamage(damage);
            }
        }
    }

    // Reloading
    IEnumerator Reload()
    {
        reloading = true;

        audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(reloadSound.length);

        ammo = clipAmount;
        reload = false;
        reloading = false;
    }

    // Agent slowly healing himself

    IEnumerator Heal()
    {
        healthCoolDown = false;
        health += 1;
        yield return new WaitForSeconds(3);
        healthCoolDown = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Getting all needed child objects.
        actionText = transform.GetChild(2).gameObject.GetComponent<TextMeshPro>();
        bloodParticle = transform.GetChild(3).gameObject.GetComponent<ParticleSystem>();
        shootingParticle = transform.GetChild(4).gameObject.GetComponent<ParticleSystem>();

        // Getting player object.
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<PlayerScript>();

        audioSource = gameObject.GetComponent<AudioSource>();
        shotSound = Resources.Load<AudioClip>("Sounds/gunshot_sound");
        reloadSound = Resources.Load<AudioClip>("Sounds/reload_sound");
        hitSound = Resources.Load<AudioClip>("Sounds/hit_sound");

        emotion = gameObject.GetComponent<EmotionScript>();

        shoot = false;
        currentCoolDown = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!(dead))
            currentPos = gameObject.transform.position;
        else
            return;

        UnityEngine.Debug.DrawRay(gameObject.transform.position, gameObject.transform.forward, Color.red, 0.5f);

        //Stopwatch watch = new Stopwatch();
        //watch.Start();
        if (PlayerVisible())
        {
            playerVisible = true;

            UnityEngine.Debug.Log(this.name + ": player visible");
        }
        else
        {
            playerVisible = false;
            attackPlayer = false;
            if (playerCoverVisible == false)
                shoot = false;
        }
        //watch.Stop();

        //UnityEngine.Debug.LogWarning("Agent " + this.name + ": PlayerVisible() function took " + watch.Elapsed + "ms to return a value.");


        if (health <= 0)
        {
            if (dead == false)
                Die();
        }

        if (attackPlayer || attackCover)
            shoot = true;

        currentCoolDown += Time.deltaTime;
        if (shoot && shootCoolDown < currentCoolDown)
        {
            if (ammo > 0)
                Shoot();
            else
                reload = true;
        }

        if(health < 100 && healthCoolDown)
            StartCoroutine(Heal());

        if(reload)
        {
            if(reloading == false)
                StartCoroutine(Reload());
        }
    }

}
