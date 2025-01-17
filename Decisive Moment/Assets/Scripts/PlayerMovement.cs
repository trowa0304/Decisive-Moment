﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public GameObject diePrefab;
    public GameObject attackPrefab;
    public GameObject slimeDiePrefab;
    public GameObject skillPrefab;

    private AnimatorStateInfo mStateInfo;

    public float moveSpeed = 5f;
    private float time_val = 0.4f;

    public Rigidbody2D rb;
    public Animator animator;

    //Jumping Variables
    private bool grounded = false;
    private float groundCheckRadius = 0.4f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float jumpForce = 8f;

    //HealthBar Variables
    public Image currentHealthBar;
    public Image currentManaBar;

    private float hitpoint = 100;
    private float maxhitpoint = 100;
    private float healthRegen = 1;

    private float mana = 100;
    private float maxMana = 100;
    private float manaRegen = 1;
    private float manaCost = 20;
    public Vector3 startPoint;
    private float moveInput;

    private System.Timers.Timer timer;
    public RespawnLogic respawnPlayer;

    //Represents the location that the player will respawn at when they die
    public Vector3 respawnPoint;
    //Represents the number of lives the player has before reaching a game over state
    public int livesRemaining = 3;
    //Creating an instance of LevelManager which controls respawns
    public LevelManager levelManager;

    private void Start()
    {
        respawnPlayer = FindObjectOfType<RespawnLogic>();
        UpdateHealthBar();
        UpdateManaBar();
        RegenTimer();
        startPoint = transform.position;
        //Set initial respawn point to where the player is first loaded into the game
        respawnPoint = transform.position;
        //Instantiate the LevelManager object
        levelManager = FindObjectOfType<LevelManager>();
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        mStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (grounded && Input.GetAxis("Jump") > 0)
        {
            grounded = false;
            animator.SetBool("isGrounded", grounded);
            rb.AddForce(new Vector2(0, jumpForce));
        }

        UpdateHealthBar();
        UpdateManaBar();
    }

    void FixedUpdate()
    {
        //Retrieve keyboard input and calculate run speed
        moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);


        //Running animation toggle check
        if (moveInput == 0)
        {
            animator.SetBool("isRunning", false);
        }
        else
        {
            animator.SetBool("isRunning", true);
        }

        //Flip character based on position
        if (moveInput < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
            //rightFlg = false;
        }
        else if (moveInput > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            //rightFlg = true;
        }

        //Check if grounded
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool("isGrounded", grounded);
        animator.SetFloat("verticalSpeed", rb.velocity.y);

        if (time_val >= 0.4f)
        {
            Attack();
            Casting();
        }
        else
        {
            animator.SetBool("isAttack", false);
            animator.SetBool("isCast", false);
            time_val += Time.deltaTime;
            //transform.tag = "Hero";
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.tag);
        switch (collision.tag)
        {
            case "Axe":
                TakeDamage(20);
                break;
            case "Monster":
                TakeDamage(5);
                break;
            case "Hero":
                break;
            case "Wall":
                break;
            case "Cliff":
                Die();
                break;
            case "Checkpoint":
                //If the player reaches a checkpoint, set the respawn point to the location of that checkpoint
                respawnPoint = collision.transform.position;
                break;
            default:
                break;
        }
    }

    ////For unit Test Hit Player
    //public void OnTriggerEnter2D(string collision)
    //{
    //    //Debug.Log(collision.tag);
    //    switch (collision)
    //    {
    //        case "Axe":
    //            TakeDamage(20);
    //            break;
    //        case "Monster":
    //            TakeDamage(5);
    //            break;
    //        case "Hero":
    //            break;
    //        case "Wall":
    //            break;
    //        case "Cliff":
    //            break;
    //        default:
    //            break;
    //    }
    //}

    //Unit Test for Attack method
    //Attack(); at line 103 must be commented out to run this method
    /*
    public char Attack(char userKey)
    {
        char keyPress = userKey;
        char actualKey = 'j';
        if (keyPress != actualKey)
        {
            return keyPress;
        }
        return actualKey;
    }
    */

    private void Attack()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            //transform.tag = "Attack";
            animator.SetBool("isAttack", true);

            attackPrefab.SetActive(true);
            Instantiate(attackPrefab, transform.position, transform.rotation);
            time_val = 0;
            //Vector2 newPosition;
            //if (rightFlg)
            //{
            //    newPosition = new Vector2(transform.position.x + 1, transform.position.y);
            //}
            //else
            //{
            //    newPosition = new Vector2(transform.position.x - 1, transform.position.y);
            //}

            //int layerMask = LayerMask.GetMask("Monster");
            //RaycastHit2D hit = Physics2D.Raycast(newPosition, rightFlg ? Vector2.right : Vector2.left, 1f, layerMask);
            //if (hit)
            //{
            //    Destroy(hit.collider.gameObject);
            //    slimeDiePrefab.SetActive(true);
            //    Instantiate(slimeDiePrefab, hit.collider.gameObject.transform.position, hit.collider.gameObject.transform.rotation);
            //}
        }
    }
    

    private void Casting() //Use special ability
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (mana > 0 && mana >= manaCost) //Must have enough mana to use special ability
            {
                animator.SetBool("isCast", true);
                skillPrefab.SetActive(true);
                Instantiate(skillPrefab, transform.position, transform.rotation);//Quaternion.Euler(transform.eulerAngles)
                time_val = 0;
                UseMana(manaCost);
            }
        }
    }

    private void Die()
    {
        diePrefab.SetActive(true);
        //Instantiate(diePrefab, transform.position, transform.rotation); 
        //Destroy(gameObject); Commenting for testing purpose (This destroys the object) 
    }

    //Health Bar Functions

    private void UpdateHealthBar()
    {
        float ratio = hitpoint / maxhitpoint;
        currentHealthBar.rectTransform.localScale = new Vector3(ratio, 1, 1);

    }

    /*Unit Test for TakeDamage method
     * 
    public float TakeDamage(float dmg)
    {
        hitpoint -= dmg;
        if(hitpoint<0)
        {
            hitpoint = 0;
            //Die();
        }
        //UpdateHealthBar();
        return hitpoint;
    }
     * */

    ////Unit Test for Hit Player
    //public void TakeDamage(float dmg)
    //{
    //    hitpoint -= dmg;
    //    if (hitpoint < 0)
    //    {
    //        hitpoint = 0;
    //    }
    //}

    //public float getHitpoint()
    //{
    //    return hitpoint;
    //}

    private void UpdateLives()
    {
        
    }

    private void TakeDamage(float dmg)
    {
        hitpoint -= dmg;

        if (hitpoint<0)
        {
            //checks that the player still has lives remaining
            if(livesRemaining > 0)
            {
                diePrefab.SetActive(true);
                Instantiate(diePrefab, transform.position, transform.rotation);
                //decrease the amount of lives left by 1
                livesRemaining--;
                //places the player wherever the last respawnPoint was set
                levelManager.Respawn();
                //reset hitpoints and mana to max
                hitpoint = maxhitpoint;
                mana = maxMana;
                //update the health and mana bars to maximum
                UpdateHealthBar();
                UpdateManaBar();
            }
            //if the player has no lives left it is game over
            else
            {
                diePrefab.SetActive(true);
                Instantiate(diePrefab, transform.position, transform.rotation);
                respawnPlayer.respawn();
                hitpoint = maxhitpoint;
                mana = maxMana;
                UpdateHealthBar();
                UpdateManaBar();
                //game over scene
                SceneManager.LoadScene(2);
            }
        }

        if (hitpoint > maxhitpoint)
        {
            hitpoint = maxhitpoint;
        }

        UpdateHealthBar();
    }

    public void HealDamage(float heal)
    {
        hitpoint += heal;
        //if hp > 100, set back to 100
        if (hitpoint > maxhitpoint)
        {
            hitpoint = maxhitpoint;
        }
        UpdateHealthBar();
    }

    /*UnitTest for HealDamage method
     * 
     * The HealDamage method is modified to return a float value.
     * 
     * "hitpoint" variable should be modified to a lower value
     
    private float HealDamage(float heal)
    {
        hitpoint += heal;
        if (hitpoint > maxhitpoint)
        {
            hitpoint = maxhitpoint;
        }
        //UpdateHealthBar();
        return hitpoint;
    }
    */

    //Mana Bar Functions
    private void UpdateManaBar()
    {
        if (mana > maxMana)
        {
            mana = maxMana;
        }
        float ratio = mana / maxMana;
        currentManaBar.rectTransform.localScale = new Vector3(ratio, 1, 1);
    }

    private void BarRegen()
    {
        float ratio = mana += manaRegen;
        currentManaBar.rectTransform.localScale = new Vector3(ratio, 1, 1);

        float ratio2 = hitpoint += healthRegen;
        currentHealthBar.rectTransform.localScale = new Vector3(ratio2, 1, 1);

        if (hitpoint > maxhitpoint)
        {
            hitpoint = maxhitpoint;
        }
        if (mana > maxMana)
        {
            mana = maxMana;
        }
    }

    private void UseMana(float useMana)
    {
        mana -= useMana;
        if (mana < 0)
        {
            mana = 0;
        }
        UpdateManaBar();
    }

    private void AddMana(float addMana)
    {
        mana += addMana;
        BarRegen();
        UpdateManaBar();
    }

    private void AddHealth(float addHealth)
    {
        hitpoint += addHealth;
        BarRegen();
        UpdateHealthBar();
    }

    private void RegenTimer()
    {
        //Create the timer with a two second interval
        timer = new System.Timers.Timer(2000);
        //Attach the elapsed event for the timer
        timer.Elapsed += SetTimer;
        timer.Start();
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    private void SetTimer(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (hitpoint < maxhitpoint)
        {
            AddHealth(healthRegen);
        }
        if (mana < maxMana)
        {
            AddMana(manaRegen);
        }
        throw new System.NotImplementedException();
    }
}
