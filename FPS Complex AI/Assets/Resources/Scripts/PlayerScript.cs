using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    int health = 100;
    int damage = 10;
    public float shootCoolDown = 0.6f;

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

    IEnumerator Shoot()
    {
        shooting = true;
        shootingParticle.Play();
        audioSource.PlayOneShot(shotSound);
        ammo--;
        ammoText.text = ammo + " / 30";

        yield return new WaitForSeconds(shootCoolDown);
        shooting = false;
        /*int random = Random.Range(0, 101);
        if(random <= accuracy)
        {
            player.GetComponent<PlayerScript>().ReceiveDamage(damage);
        }*/
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

    public void ReceiveDamage(int amount)
    {
        //bloodParticle.Play();
        health -= amount;
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        shotSound = Resources.Load<AudioClip>("Sounds/gunshot_sound");
        reloadSound = Resources.Load<AudioClip>("Sounds/reload_sound");
        hitSound = Resources.Load<AudioClip>("hit_sound");

        // Getting all needed child objects.
        ammoText = GameObject.Find("AmmoText").GetComponent<Text>();
        bloodParticle = transform.GetChild(3).gameObject.GetComponent<ParticleSystem>();
        shootingParticle = transform.GetChild(4).gameObject.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            if (ammo > 0 && reloading == false && shooting == false)
            {
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

                            agentScript.ReceiveDamage(damage);
                        }
                        else
                        if (hitObject.CompareTag("Big Object") || hitObject.CompareTag("Small Object"))
                        {
                            //
                        }
                    }
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R pressed, ammo " + ammo + " reloading " + reloading);
            if (reloading == false && ammo < clipAmount)
                StartCoroutine(Reload());
        }
        

    }
}
