using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public int gold;
    public int curHP;
    public int maxHP;
    public bool dead;

    [Header("Attack")]
    public int damage;
    public float attackRange;
    public float attackRate;
    private float lastAttackTime;

    [Header("Components")]
    public Rigidbody2D rig;
    public Player photonPlayer;
    public SpriteRenderer sr;
    public Animator weaponAnim;

    //Local player
    public static PlayerController thisPlayer;
    // Start is called before the first frame update

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        //TO DO: Initialize health bar
        if (player.IsLocal)
            thisPlayer = this;
        else
            rig.isKinematic = true;

        GameManager.instance.players[id - 1] = this; 
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
            return;
        Move();
        if (Input.GetMouseButtonDown(0) && Time.time - lastAttackTime > attackRate)
            Attack();

        // flip the player sprite if the mouse crosses half way point on screen;
        float mouseX = (Screen.width / 2 - Input.mousePosition.x);
        if (mouseX > 0)
            weaponAnim.transform.parent.localScale = new Vector3(-1, 1, 1);
        else
            weaponAnim.transform.parent.localScale = new Vector3(1, 1, 1);
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        rig.velocity = new Vector2(x, y) * moveSpeed;
    } 
    void Attack()
    {
        lastAttackTime = Time.time;
        // Get a direction vector based on mouse position
        Vector3 dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position + dir, dir, attackRange);
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Enemy"))
        {
            //TO DO: Reference enemy and damage them
        }

        //play animaton
        weaponAnim.SetTrigger("Attack");
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        curHP -= damage;
        // TO DO: Update Health UI
        if (curHP <= 0)
            Die();
        else
        {
            // Make the player flash red when taking damage;
            StartCoroutine(DamageFlash());
            IEnumerator DamageFlash()
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.05f);
                sr.color = Color.white;
            }
        }
    }
    void Die()
    {
        dead = true;
        rig.isKinematic = true;
        transform.position = new Vector3(0, 99, 0);
        Vector3 spawnPos = GameManager.instance.spawnPoints[Random.Range(0, GameManager.instance.spawnPoints.Length)].position;
        StartCoroutine(Spawn(spawnPos, GameManager.instance.respawnTime));
    }

    IEnumerator Spawn(Vector3 spawnPos, float timeToSpawn)
    {
        yield return new WaitForSeconds(timeToSpawn);
    }

    [PunRPC]
    void Heal (int amountToHeal)
    {
        curHP = Mathf.Clamp(curHP + amountToHeal, 0, maxHP);
        //TO DO: Update health bar
    }

    [PunRPC]
    void GiveGold (int goldToGive)
    {
        gold += goldToGive;
        // TO DO: update UI
    }
}
