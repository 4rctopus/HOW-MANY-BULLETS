using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Die : MonoBehaviour {
    public GameObject projectile;
    public Camera cam;
    public GameObject hpText;
    public GameObject deadText;
    public GameObject damageEffect;
    public float lookAhead = 100f;
    public float shootCd = 4f;
    public float moveStrength = 10f;
    public float pullDownStr = 5f;
    public float hitKnockBack = 50f;

    public AudioSource audioSourceShoot;
    public AudioSource audioSourceContact;
    public AudioSource audioSourceDamaged;
    public AudioSource audioSourceDeath;

    private int health = 10;
    public int maxHp = 5;

    private Vector3 centToMouse;
    private Rigidbody rb;

    GameObject[] sideGo = new GameObject[6];
    bool[] sideOnGround = new bool[6];
    float[] sideCd = new float[6];

    float immunity = 0f;
    float immunityTime = 1.5f;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();

        for(int i = 0; i < 6; ++i) {
            sideGo[i] = transform.GetChild(i).gameObject;
            sideOnGround[i] = false;
        }

        for(int i = 0; i < 6; ++i) sideOnGround[i] = true;
        health = maxHp;

        hpText.GetComponent<TextMeshProUGUI>().text = "health " + health.ToString();
    }

    void Update() {
        // Camera follows player
        cam.transform.position = new Vector3(transform.position.x, cam.transform.position.y, transform.position.z);

        // Camera moves towards mouse
        Vector3 mouse = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y);
        Vector3 scrCenter = new Vector3(Screen.width, 0, Screen.height) / 2f;
        centToMouse = mouse - scrCenter;
        cam.transform.position += centToMouse / lookAhead;

        for(int i = 0; i < 6; ++i) {
            if(sideCd[i] > 0) sideCd[i] -= Time.deltaTime;
            // Set material cooldown
            float visualCd = 0f;
            if(sideCd[i] > 0f) visualCd = sideCd[i] / shootCd;
            sideGo[i].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetFloat("_cooldown", visualCd);
        }

        // Immunity time:
        if(immunity > 0) {
            immunity -= Time.deltaTime;
            GetComponent<Renderer>().material.color = Color.black;

            for(int i = 0; i < 6; ++i)
                sideGo[i].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetInt("_immune", 1);
        } else {
            GetComponent<Renderer>().material.color = Color.white;
            for(int i = 0; i < 6; ++i)
                sideGo[i].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetInt("_immune", 0);
        }
    }

    void FixedUpdate() {
        Vector3 movement = new Vector3(0, 0, 0);
        movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        // Pull down player if not moving
        if(!(Mathf.Abs(movement.x) > 0.001 || Mathf.Abs(movement.z) > 0.001)) {
            rb.AddForceAtPosition(Vector3.down * pullDownStr, transform.position);
        }
        rb.AddForceAtPosition(movement * moveStrength, transform.position + Vector3.up * 0.5f);

        for(int i = 0; i < 6; ++i) {
            Vector3 pos = sideGo[i].transform.TransformPoint(new Vector3(0, 0, 0));
            if(pos.y < 0.02) {
                // On ground 
                if(!sideOnGround[i] && sideCd[i] <= 0f) { // Just landed => shoot                    
                    // Shoot as many bullets as the side number        
                    for(int j = 0; j <= i; ++j) {
                        // Random shoot spread, fewer bullets have less spread
                        float spread = i * 2f;
                        Vector3 shootDir = Quaternion.Euler(0, Random.Range(-spread, spread), 0) * centToMouse.normalized;
                        GameObject proj = Instantiate(projectile, transform.position, Quaternion.LookRotation(shootDir));
                        Rigidbody projRb = proj.GetComponent<Rigidbody>();
                        projRb.velocity = shootDir * (20f + Random.Range(0f, 2f)); // Random shoot speed                        
                    }
                    StartCoroutine(ShootSounds(i + 1));


                    sideCd[i] = shootCd;
                }

                sideOnGround[i] = true;
            } else {
                sideOnGround[i] = false;
            }
        }
    }

    
    IEnumerator ShootSounds(int num) {
        for(int i = 0; i < num; ++i) {
            audioSourceShoot.pitch = Random.Range(0.9f, 1.1f);
            audioSourceShoot.volume = 1f / num;
            audioSourceShoot.PlayOneShot(audioSourceShoot.clip);
            yield return new WaitForSeconds(0.01f);
        }
    }


    private void OnCollisionEnter(Collision collision) {
        if(collision.collider.tag == "Ground" || collision.collider.tag == "Wall") {
            audioSourceContact.pitch = Random.Range(0.8f, 1.2f);
            audioSourceContact.Play();
        }
        if(collision.collider.tag == "Enemy") {
            //Destroy(collision.collider.gameObject); // Destroy enemy that hit us (no)
            Rigidbody orb = collision.collider.gameObject.GetComponent<Rigidbody>();

            rb.AddForceAtPosition(orb.velocity.normalized * hitKnockBack, transform.position, ForceMode.Impulse);

            // Decrease our health
            if(immunity <= 0) {
                health -= 1;
                hpText.GetComponent<TextMeshProUGUI>().text = "health " + health.ToString();
                immunity = immunityTime;

                Instantiate(damageEffect, transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));

                if(health > 0)
                    audioSourceDamaged.Play();
            }

            if(health <= 0) {
                // You are dead
                //Destroy(gameObject);
                audioSourceDeath.Play();
                DestroyExceptSound();
                deadText.SetActive(true);
            }
        }
    }

    void DestroyExceptSound() {
        // Sides are in children and need to be destroyed
        foreach(Transform child in transform) Destroy(child.gameObject);

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        Destroy(GetComponent<Rigidbody>());
        GetComponent<Die>().enabled = false;
    }
}
