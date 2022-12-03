using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
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

    private void OnCollisionEnter(Collision collision) {
        if(collision.collider.tag == "Enemy") {
            GameObject enemy = collision.collider.gameObject;
            Instantiate(enemyDeath, enemy.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
            Destroy(enemy);
        }
        if(collision.collider.tag != "Player" && collision.collider.tag != "Projectile") {
            transform.GetChild(0).parent = null;
            Instantiate(deathObject, transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));

            DisableComponentsExceptSound();
            StartCoroutine(PlayHitSoundAndDestroy());
        }
    }

    void DisableComponentsExceptSound() {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        Destroy(GetComponent<Rigidbody>());
    }

    IEnumerator PlayHitSoundAndDestroy() {
        audioSource.Play();
        yield return new WaitForSeconds(audioSource.clip.length + 0.1f);
        Destroy(gameObject);
    }
}
