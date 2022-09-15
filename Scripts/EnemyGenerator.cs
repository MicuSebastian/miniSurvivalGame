using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    public GameObject mob;
    public float timer;
    public float interval;
    public GameObject toSpawn;
    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        mob = null;
        Terrain terrain = (Terrain)FindObjectOfType(typeof(Terrain));
        transform.position = new Vector3(transform.position.x, terrain.SampleHeight(transform.position), transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (mob == null)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                timer = interval;
                mob = Instantiate(toSpawn, transform.position, transform.rotation);
            }
        }
    }
}
