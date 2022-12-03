using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class DieNet : NetworkBehaviour {
    public Color player1Color;
    public Color player2Color;
    public GameObject projectile;
    private Camera cam;    
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

    [HideInInspector]
    public float immunity = 0f;
    public float immunityTime = 1.5f;

    private GameObject hpText;
    private GameObject enemyText;
    private GameObject player1WinText;
    private GameObject player2WinText;
    private GameObject restartButton;

    public override void OnNetworkSpawn() {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        hpText = GameObject.Find("ui").transform.Find("HealthText").gameObject;
        enemyText = GameObject.Find("ui").transform.Find("EnemyText").gameObject;

        player1WinText = GameObject.Find("ui").transform.Find("Player1Win").gameObject;
        player2WinText = GameObject.Find("ui").transform.Find("Player2Win").gameObject;
        restartButton = GameObject.Find("ui").transform.Find("restartButton").gameObject;

        rb = GetComponent<Rigidbody>();

        for(int i = 0; i < 6; ++i) {
            sideGo[i] = transform.GetChild(i).gameObject;
            sideOnGround[i] = false;
        }

        for(int i = 0; i < 6; ++i) sideOnGround[i] = true;
        health = maxHp;
        hpText.gameObject.SetActive(true);
        enemyText.gameObject.SetActive(true);
        hpText.GetComponent<TextMeshProUGUI>().text = "health " + health.ToString();
        hpText.GetComponent<TextMeshProUGUI>().text = "enemy " + health.ToString(); // enemy will have the same health


        // Set different colors for different players
        Color color = Color.white;
        if(OwnerClientId >= 1){
            color = player2Color;            
        }else if(OwnerClientId == 0){
            Debug.Log("Player 1 color");
            color = player1Color;
        }
        for(int i = 0; i < 6; ++i) {
            sideGo[i].transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_color", color);
        }
        GetComponent<Renderer>().material.color = color; // TODO fix

        if(IsOwner){
            if(OwnerClientId == 0){
                transform.position = GameObject.Find("Spawn1").transform.position;
            }else if(OwnerClientId >= 1){
                transform.position = GameObject.Find("Spawn2").transform.position;
            }
        }

        // Set layer
        if(OwnerClientId >= 1){
            gameObject.layer = LayerMask.NameToLayer("Player2");
        }
    }

    void Update() {
        // === Common === (update graphics)
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

        if(!IsOwner) return;
        // === Owner ===

        // Camera follows player
        cam.transform.position = new Vector3(transform.position.x, cam.transform.position.y, transform.position.z);

        // Camera moves towards mouse
        Vector3 mouse = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y);
        Vector3 scrCenter = new Vector3(Screen.width, 0, Screen.height) / 2f;
        centToMouse = mouse - scrCenter;
        cam.transform.position += centToMouse / lookAhead;        
    }

    void FixedUpdate() {
        if(!IsOwner) return;

        Vector3 movement = new Vector3(0, 0, 0);
        movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        // Pull down player if not moving
        if(!(Mathf.Abs(movement.x) > 0.001 || Mathf.Abs(movement.z) > 0.001)) {
            rb.AddForceAtPosition(Vector3.down * pullDownStr, transform.position);
        }
        rb.AddForceAtPosition(movement * moveStrength, transform.position + Vector3.up * 0.5f);

        //*/
        for(int i = 0; i < 6; ++i) {
            Vector3 pos = sideGo[i].transform.TransformPoint(new Vector3(0, 0, 0));
            if(pos.y < 0.02) {
                // On ground 
                if(!sideOnGround[i] && sideCd[i] <= 0f) { // Just landed => shoot                    
                    
                    // Shoot as many bullets as the side number
                    ShootServerRpc(i, centToMouse);
                    //sideCd[i] = shootCd;
                }

                sideOnGround[i] = true;
            } else {
                sideOnGround[i] = false;
            }
        }
        //*/
    }

    [ClientRpc]
    void ShootClientRpc(int side){
        sideCd[side] = shootCd;
        StartCoroutine(ShootSounds(side));
    }

    [ServerRpc]
    public void ShootServerRpc(int side, Vector3 direction, ServerRpcParams serverRpcParams = default){
        ShootClientRpc(side);
        for(int j = 0; j <= side; ++j) {
            // Random shoot spread, fewer bullets have less spread
            float spread = side * 2f;
            Vector3 shootDir = Quaternion.Euler(0, Random.Range(-spread, spread), 0) * direction.normalized;
            GameObject proj = Instantiate(projectile, transform.position, Quaternion.LookRotation(shootDir));
            Rigidbody projRb = proj.GetComponent<Rigidbody>();
            projRb.velocity = shootDir * (20f + Random.Range(0f, 2f)); // Random shoot speed
            if(OwnerClientId >= 1){
                proj.layer = LayerMask.NameToLayer("Bullet2");
            }
            proj.GetComponent<ProjectileNet>().ownerId = OwnerClientId;
            proj.GetComponent<NetworkObject>().Spawn();
        }
    }

    [ClientRpc]
    public void DamagedClientRpc(){
        health -= 1;

        if(IsOwner)
            hpText.GetComponent<TextMeshProUGUI>().text = "health " + health.ToString();
        else
            enemyText.GetComponent<TextMeshProUGUI>().text = "enemy " + health.ToString();
            
        immunity = immunityTime;
        Instantiate(damageEffect, transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));

        if(health > 0)
            audioSourceDamaged.Play();
        else{
            // Dead
            audioSourceDeath.Play();
            DestroyExceptSound();

            if(OwnerClientId == 0 && !player1WinText.activeSelf){
                // Player 2 has won
                player2WinText.SetActive(true);
                restartButton.SetActive(true);
            }else if(OwnerClientId >= 1 && !player2WinText.activeSelf){
                player1WinText.SetActive(true);
                restartButton.SetActive(true);
            }
        }
    }


    IEnumerator ShootSounds(int side) {        
        for(int i = 0; i <= side; ++i) {
            audioSourceShoot.pitch = Random.Range(0.9f, 1.1f);
            audioSourceShoot.volume = 1f / side;
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
        //Destroy(GetComponent<Rigidbody>());
        GetComponent<DieNet>().enabled = false;
    }
}
