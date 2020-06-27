﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class MovementManager : MonoBehaviour
{
    public float moveSpeed, jumpHeight;
    public bool onGround = false;
    public Animator animator;
    float horizontalMove = 0f;

    // determine which way player is facing
    private bool faceRight = true; // determine which way player is facing

    // for attack
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 40;
    public LayerMask enemyLayers;
    public float attackRate = 2f;
    float nextAttackTime = 0f;

    // for health bar
    public int maxHealth = 100;
    int currentHealth;
    public HealthBar healthBar;

    public bool isAlive = true;

    // Start is called before the first frame update
    void Start()
    {
        // set health bar
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        // to move
        horizontalMove = CrossPlatformInputManager.GetAxis("Horizontal") * moveSpeed;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));
        
        // face direction of movement
        if (horizontalMove > 0)
        {            
            if (!faceRight)
            {
                Flip();
            }
            transform.Translate(Vector2.right * Time.deltaTime * moveSpeed);
        }
        else if (horizontalMove < 0)
        {
            if (faceRight)
            {
                Flip();
            }
            transform.Translate(Vector2.left * Time.deltaTime * moveSpeed);
        }

        // to jump
        if (CrossPlatformInputManager.GetButtonDown("Jump") && onGround) 
        {
            animator.SetBool("isJumping", true);
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
            onGround = false; // solved inconsistent jump height
        }

        // delay attack rate
        if (Time.time >= nextAttackTime)
        {
            if (CrossPlatformInputManager.GetButtonDown("Attack"))
            {
                Attack();
                FindObjectOfType<AudioManager>().Play("Sword");
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void Flip() 
    {
        faceRight = !faceRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    // to disable double jumping
    void OnCollisionEnter2D (Collision2D obj)
    {
        if (obj.gameObject.tag == "ground")
        {
            animator.SetBool("isJumping", false); // stop jumping animation
            onGround = true;
        }
    }

    void OnCollisionExit2D (Collision2D obj)
    {
        if (obj.gameObject.tag == "ground")
        {
            onGround = false; // disallow jumping in air
        }
    }

    void Attack() 
    {
        animator.SetTrigger("Attack"); // attack animation

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers); // detect enemies

        foreach(Collider2D enemy in hitEnemies)
        {
            // deal damage
            enemy.GetComponent<Enemy>().takeDamage(attackDamage);
        }
    }

    public void takeDamage(int damage)
    {
        currentHealth -= damage;
        // update health bar
        healthBar.SetHealth(currentHealth);

        //play hurt animation
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // player died
        animator.SetBool("isDead", true);
        isAlive = false;

        FindObjectOfType<AudioManager>().Play("PlayerDeath");
        
        this.enabled = false;

        GameObject.Find("gameCanvas").GetComponent<GameManager>().LoseGame();
    }
    
    void OnDrawGizmosSelected() 
    {
        if (attackPoint == null) 
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}