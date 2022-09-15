using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed;
    public float rotationSpeed;
    public float damage;
    public static PlayerController instance;


    private Transform cameraPivot;
    private Rigidbody rb;
    private Terrain terrain;
    private Animator animController;
    private bool isAttacking;
    private float health;
    private float points;
    private Text scoreText;
    private Image healthBar;
    private bool isAlive;

    private bool firstAttackOn;
    private bool secondAttackOn;
    private bool thirdAttackOn;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraPivot = Camera.main.transform.parent;
        terrain = (Terrain)FindObjectOfType(typeof(Terrain));
        animController = GetComponent<Animator>();
        scoreText = GameObject.Find("Score").GetComponent<Text>();
        healthBar = GameObject.Find("HealthBar").GetComponent<Image>();
        points = 0;
        health = 100;
        isAlive = true;
        damage = 30;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            isAlive = false;
        }
        scoreText.text = "Score: " + points;
        healthBar.transform.localScale = new Vector3(health / 100.0f, 1, 1);
        Attack();
        Animations();
    }

    void Animations()
    {
        if (!isAttacking && isAlive)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            if (x != 0 || y != 0)
            {
                if (y > 0)
                {
                    animController.Play("Run");
                }
                else
                {
                    animController.Play("Run Backward");
                }
            }
            else
            {
                animController.Play("Idle");
            }
        }
        if (isAttacking && isAlive)
        {
            animController.Play("Attack");
        }
        if (!isAlive)
        {
            animController.Play("Die");
        }
    }

    void Attack()
    {
        if (isAlive)
        {
            if (Input.GetMouseButton(0))
            {
                isAttacking = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                isAttacking = false;
            }

            if (isAttacking)
            {
                if (animController.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                {
                    float time = (animController.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1;
                    if (time > 0.15f && time < 0.25f)
                    {
                        firstAttackOn = true;
                    }
                    else firstAttackOn = false;

                    if (time > 0.35f && time < 0.45f)
                    {
                        secondAttackOn = true;
                    }
                    else secondAttackOn = false;

                    if (time > 0.55f && time < 0.65f)
                    {
                        thirdAttackOn = true;
                    }
                    else thirdAttackOn = false;

                }
            }
        }
    }

    void Move()
    {
        if (!isAttacking)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            rb.AddRelativeForce(new Vector3(x * movementSpeed * Time.deltaTime, 0, y * movementSpeed * Time.deltaTime), ForceMode.Impulse);
            //transform.Translate(new Vector3(x * movementSpeed * Time.deltaTime, 0, y * movementSpeed * Time.deltaTime));
            transform.position = new Vector3(transform.position.x, terrain.SampleHeight(transform.position) + 0.1f, transform.position.z);
        }
    }

    void RotateCamera()
    {
        float x = Input.GetAxisRaw("Mouse X");
        float y = Input.GetAxisRaw("Mouse Y");

        transform.Rotate(new Vector3(0, x * Time.deltaTime * rotationSpeed, 0));
        Vector3 cameraRotation = cameraPivot.transform.rotation.eulerAngles;
        cameraRotation.x -= y * Time.deltaTime * rotationSpeed;
        if (cameraRotation.x < 5)
        {
            cameraRotation.x = 5;
        }
        if (cameraRotation.x > 60)
        {
            cameraRotation.x = 60;
        }
        cameraPivot.transform.rotation = Quaternion.Euler(cameraRotation);
    }

    void FixedUpdate()
    {
        if (isAlive)
        {
            Move();
            RotateCamera();
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health < 0)
        {
            health = 0;
        }
    }

    public void Heal(float value)
    {
        health += value;
        if (health > 100)
        {
            health = 100;
        }
    }

    public bool IsAlive()
    {
        return isAlive;
    }

    public void EarnPoints(float value)
    {
        points += value;
    }

    void OnTriggerEnter(Collider col)
    {
        Gem gem = col.gameObject.GetComponent<Gem>();
        if (gem)
        {
            if (gem.type == "Health")
            {
                Heal(gem.value);
            }
            if (gem.type == "Points")
            {
                points += gem.value;
            }
            Destroy(gem.transform.parent.gameObject);
        }
    }

    void OnTriggerStay(Collider col)
    {

        AIController enemy = col.gameObject.GetComponent<AIController>();
        if (enemy && enemy.IsAlive())
        {
            if (firstAttackOn && !enemy.onceDamagedFirst)
            {
                enemy.TakeDamage(damage);
                enemy.onceDamagedFirst = true;
            }
            if (secondAttackOn && !enemy.onceDamagedSecond)
            {
                enemy.TakeDamage(damage);
                enemy.onceDamagedSecond = true;
            }
            if (thirdAttackOn && !enemy.onceDamagedThird)
            {
                enemy.TakeDamage(damage);
                enemy.onceDamagedThird = true;
            }
            if (!firstAttackOn)
            {
                enemy.onceDamagedFirst = false;
            }
            if (!secondAttackOn)
            {
                enemy.onceDamagedSecond = false;
            }
            if (!thirdAttackOn)
            {
                enemy.onceDamagedThird = false;
            }
        }
    }
}
