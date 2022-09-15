using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public float radiusDetection;
    public float guardRadius;
    public GameObject target;
    public float damage;
    public float health;
    public bool onceDamagedFirst;
    public bool onceDamagedSecond;
    public bool onceDamagedThird;
    public float pointsValue;


    private Terrain terrain;
    private Vector3 targetDestination;
    private Vector3 spawnPoint;
    private float nextPositionTimer;
    private NavMeshAgent agent;
    private Animator animController;
    private bool isAttacking;
    private bool isFollowing;
    private float attackCooldown;
    private bool isAlive;
    private float deathTimer;
    private bool pointsAddedOnce;
    // Start is called before the first frame update
    void Start()
    {
        terrain = (Terrain)FindObjectOfType(typeof(Terrain));
        transform.position = new Vector3(transform.position.x, terrain.SampleHeight(transform.position), transform.position.z);
        spawnPoint = transform.position;
        nextPositionTimer = 0;
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<Animator>();
        isAttacking = false;
        isFollowing = false;
        isAlive = true;
        deathTimer = 5;
        pointsAddedOnce = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            if (!pointsAddedOnce)
            {
                pointsAddedOnce = true;
                PlayerController.instance.EarnPoints(pointsValue);
            }
            isAlive = false;
            animController.Play("Die");
            agent.enabled = false;
            deathTimer -= Time.deltaTime;
            if (deathTimer < 0)
            {
                Destroy(this.gameObject);
            }
        }
        if (isAlive)
        {
            float distance = Vector3.Distance(PlayerController.instance.transform.position, transform.position);
            if (PlayerController.instance.IsAlive())
            {
                if (distance < radiusDetection && distance > 10)
                {
                    Vector3 direction = PlayerController.instance.transform.position - transform.position;
                    direction.Normalize();
                    Vector3 destination = PlayerController.instance.transform.position - 5 * direction;
                    destination.y = terrain.SampleHeight(destination);
                    agent.SetDestination(destination);
                    if (!isAttacking)
                    {
                        animController.Play("Run", 0);
                    }
                    isFollowing = true;
                    isAttacking = false;
                }
                else
                {
                    if (distance >= radiusDetection)
                    {
                        isFollowing = false;
                    }
                }
                if (isFollowing)
                {
                    float dist = agent.remainingDistance;
                    if (dist != Mathf.Infinity && agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance == 0)
                    {
                        isAttacking = true;
                        attackCooldown = 0.1f;
                        isFollowing = false;
                    }
                    else
                    {
                        isAttacking = false;
                    }
                }
                if (isAttacking)
                {
                    attackCooldown -= Time.deltaTime;
                    if (attackCooldown < 0)
                    {
                        attackCooldown = 2;
                        animController.Play("Attack", 0, 0f);
                        PlayerController.instance.TakeDamage(damage);
                    }
                }
            }
            else
            {
                isFollowing = false;
                isAttacking = false;
            }
            if (!isAttacking && !isFollowing)
            {
                float dist = agent.remainingDistance;
                if (dist != Mathf.Infinity && agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance == 0)
                {
                    nextPositionTimer += Time.deltaTime;
                    animController.Play("Idle", 0);
                }
                if (nextPositionTimer > 5)
                {
                    float angle = Mathf.Deg2Rad * Random.Range(0.0f, 359.9f);
                    float radius = Random.Range(0, guardRadius);
                    Vector3 position = radius * (new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle))) + spawnPoint;
                    position.y = terrain.SampleHeight(position);
                    agent.SetDestination(position);
                    animController.Play("Run", 0);
                    nextPositionTimer = 0;
                }
            }
        }
    }

    void FixedUpdate()
    {
        transform.position = new Vector3(transform.position.x, terrain.SampleHeight(transform.position), transform.position.z);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    public bool IsAlive()
    {
        return isAlive;
    }
}
