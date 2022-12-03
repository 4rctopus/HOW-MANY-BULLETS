using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BallEnemy : MonoBehaviour {
    public GameObject player;

    public Color color;
    public float moveStrength = 5f;
    public float timeToForgetPlayer = 4f;

    private Rigidbody rb;
    private Renderer ballRenderer;
    private NavMeshAgent navMeshAgent;
    public float movementSoundCooldown = 3f;
    public AudioSource audioSourceMovement;
    public AudioSource audioSourceContact;

    private Vector3 startPosition;    

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
        ballRenderer = GetComponent<Renderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
    }

    public Vector3 debugVec;
    // Update is called once per frame
    void Update() {
        navMeshAgent.destination = player.transform.position;
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        navMeshAgent.velocity = rb.velocity;

        transform.GetChild(0).transform.rotation = Quaternion.Euler(90,0,0); // Don't let the ball rotation affect the circle rotation
    }


    void OnDrawGizmos() {
        Gizmos.color = Color.green;
        if(navMeshAgent != null){
            Gizmos.DrawRay(transform.position, navMeshAgent.desiredVelocity);
            Gizmos.DrawSphere(navMeshAgent.nextPosition, 0.8f);
        }
    }

    float followTimer = -1f;
    void FixedUpdate() {
        rb.AddForceAtPosition(-rb.velocity, transform.position);
        if(player) {
            Vector3 toPlayer = player.transform.position - transform.position;
            int layerMask = 1 << 6; layerMask = ~layerMask;
            if(Physics.Raycast(transform.position, toPlayer.normalized, out RaycastHit raycastHit, 50f, layerMask)) {
                if(raycastHit.collider.tag == "Player") {
                    // Player is in line of sight => start following
                    followTimer = timeToForgetPlayer;
                }
            }
        }
        
        bool followingPlayer = followTimer > 0f;
        if(followTimer > 0f && rb.velocity.magnitude < 15f) {
            rb.AddForceAtPosition(navMeshAgent.desiredVelocity.normalized * moveStrength, transform.position + Vector3.up * 0.2f);            
            PlayMovementSound();
            navMeshAgent.nextPosition = rb.position;
        }

        if(followTimer > 0f) followTimer -= Time.deltaTime;        
    }

    bool movementSoundPlayable = true;
    void PlayMovementSound() {
        if(!movementSoundPlayable) return;

        audioSourceMovement.pitch = Random.Range(0.8f, 1.2f);
        audioSourceMovement.Play();

        StartCoroutine(ColorEffect());

        movementSoundPlayable = false;
        StartCoroutine(MovementSoundCooldown());
    }

    IEnumerator ColorEffect() {
        float t = 0f;
        float audioTime = audioSourceMovement.clip.length;
        while(t < audioTime) {
            float x = t / audioTime;

            x = 1 - Mathf.Pow(Mathf.Abs(((x - 1) * (x - 1)) * 2 - 1), 3f);
            ballRenderer.material.SetColor("_EmissionColor", color * x * 15);
            t += Time.deltaTime;
            yield return null;
        }
        ballRenderer.material.SetColor("_EmissionColor", color * 0);
    }


    IEnumerator MovementSoundCooldown() {
        yield return new WaitForSeconds(movementSoundCooldown + Random.Range(0f, 1f));
        movementSoundPlayable = true;
    }

    private void OnCollisionEnter(Collision collision) {
        if(collision.collider.tag == "Wall" || collision.collider.tag == "Enemy") {
            audioSourceContact.pitch = Random.Range(0.8f, 1.2f);
            audioSourceContact.Play();
        }
    }
}
