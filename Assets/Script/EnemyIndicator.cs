using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIndicator : MonoBehaviour {
    Camera cam;
    BoxCollider boxCollider;

    public GameObject enemies;
    // Start is called before the first frame update
    void Start() {
        cam = GetComponent<Camera>();
        boxCollider = GetComponent<BoxCollider>();
    }


    // Update is called once per frame
    void Update() {
        var cs = new Vector3(
            cam.orthographicSize * cam.aspect,
            0,
            cam.orthographicSize
        );

        Vector3[] addp = new Vector3[] { new Vector3( cs.x, 0, cs.z), new Vector3( cs.x, 0, -cs.z), new Vector3(cs.x, 0,  cs.z), new Vector3(-cs.x, 0,  cs.z) };
        Vector3[] addn = new Vector3[] { new Vector3(-cs.x, 0, cs.z), new Vector3(-cs.x, 0, -cs.z), new Vector3(cs.x, 0, -cs.z), new Vector3(-cs.x, 0, -cs.z) };

        foreach(Transform enemy in enemies.transform) {
            var target = enemy.position; target.y = transform.position.y;
            
            var hitPoint = new Vector3(0,0,0);
            var hit = false;
            for(int i = 0; i < 4; ++i){
                if( Geometry.SegmentSegmentIntersection(out var intersection, target, transform.position,
                    transform.position + addp[i], transform.position + addn[i]) ){
                        hitPoint = intersection;
                        hit = true;
                    }
            }

            var dir = (transform.position - target).normalized;
            var dist = Vector3.Distance(transform.position, target);
            var circle = enemy.transform.GetChild(0);
            if(hit) {
                circle.GetComponent<SpriteRenderer>().enabled = true;
                float size = 1f/dist * 10f;
                circle.transform.localScale = new Vector3(size, size, size);
                float radius = size * enemy.localScale.x / 2f;
                circle.transform.position = hitPoint + dir * (radius + 0.05f) + Vector3.down * 1;
            } else {
                circle.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }
}
