using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ProjectileNet : NetworkBehaviour {
    public ulong ownerId;
    public GameObject deathObject;
    public GameObject enemyDeath;
    float life = 10f;

    AudioSource audioSource;

    void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update() {
        life -= Time.deltaTime;
        if(life < 0f) {
            Destroy(gameObject);
        }
    }

    // private void OnTriggerEnter(Collider other){
    //     Debug.Log("OnTriggerEnter");
    //     if(other.transform.gameObject.layer != LayerMask.NameToLayer("Player") && 
    //        other.transform.gameObject.layer != LayerMask.NameToLayer("Projectile") ){
    //         KillClientRpc();
    //         DestroyLater(audioSource.clip.length * 2 + 1f);
    //     }
    // }

    private void OnCollisionEnter(Collision collision) {
        // This will only get called on the server/host

        var layer = LayerMask.LayerToName(collision.collider.gameObject.layer);
        Debug.Log("OnCollissionEnter " + layer);
        if((ownerId == 0 && layer == "Player2") || (ownerId >= 1 && layer == "Player")) {
            // Shot enemy player
            var player = collision.collider.gameObject.GetComponent<DieNet>();
            if(player.immunity <= 0){
                player.DamagedClientRpc();
            }
            
            KillClientRpc();
            DestroyLater(audioSource.clip.length * 2 + 1f);
        } else if(collision.collider.tag != "Projectile") {
            KillClientRpc();
            DestroyLater(audioSource.clip.length * 2 + 1f);
        }
    }

    IEnumerator DestroyLater(float time) {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    [ClientRpc]
    void KillClientRpc() {
        transform.GetChild(0).parent = null;
        Instantiate(deathObject, transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
        DisableComponentsExceptSound();
        audioSource.Play();
    }


    void DisableComponentsExceptSound() {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        //Destroy(GetComponent<Rigidbody>());
    }

    // IEnumerator PlayHitSoundAndDestroy() {
    //     audioSource.Play();
    //     yield return new WaitForSeconds(audioSource.clip.length + 0.1f);
    //     Destroy(gameObject);
    // }
}
