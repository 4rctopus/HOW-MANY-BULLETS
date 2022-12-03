using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public GameObject ballEnemy;
    public GameObject player;

    public float spawnTime = 2f;
    private float cd = 0f;

    Transform min, max;
    // Start is called before the first frame update
    void Start() {
        min = transform.Find("min");
        max = transform.Find("max");
    }

    // Update is called once per frame
    void Update() {
        if (cd <= 0f) {
            // Spawn
            cd = spawnTime;

            // Generate positions until it's far enough from player
            while(true){
                Vector3 randPos =
                    new Vector3(Random.Range(min.position.x, max.position.x), 0.3f, Random.Range(min.position.z, max.position.z));
                float dist = Vector3.Distance(randPos, player.transform.position);
                if( dist > 15 ){
                    // Spawn enemy
                    GameObject go = Instantiate(ballEnemy, randPos, Quaternion.identity);
                    go.GetComponent<BallEnemy>().player = player;
                    
                    break;
                }
            }
        }
        else {
            cd -= Time.deltaTime;
        }
    }
}
