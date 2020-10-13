using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Enemy : MonoBehaviourPun
{
    [Header("Info")]
    public string enemyName;
    public float moveSpeed;

    public int curHP;
    public int maxHP;

    public float chaseRange;
    public float attackRange;

    private PlayerController targetPlayer;

    public float playerDetectRate = 0.2f;
    private float lastPlayerDetectTime;

    public string objectToSpawnOnDeath;

    [Header("Attack")]
    public int damage;
    public float attackRate;
    private float lastAttackTime;

    [Header("Components")]
    public HeaderInfo healthBar;
    public SpriteRenderer sr;
    public Rigidbody2D rig;
    // Start is called before the first frame update
    void Start()
    {
        healthBar.Initialize(enemyName, maxHP); 
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if(targetPlayer != null)
        {
            float dist = Vector3.Distance(transform.position, targetPlayer.transform.position);
           // Debug.Log(dist);
            if (dist < attackRange && Time.time - lastAttackTime >= attackRange)
                Attack();
            else if (dist > attackRange)
            {
                Vector3 dir = targetPlayer.transform.position - transform.position;
                rig.velocity = dir.normalized * moveSpeed;
            } 
            else
            {
                rig.velocity = Vector2.zero;
            }
        }
        DetectPlayer();
        // Since enemies have a tendency to "twirl", go ahead and try to have them always rotate correctly
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, 0.5f);
    }

    void Attack()
    {
        //Debug.Log("Attacking");
        lastAttackTime = Time.time;
        targetPlayer.photonView.RPC("TakeDamage", targetPlayer.photonPlayer, damage);
    }

    void DetectPlayer()
    {
        if(Time.time - lastPlayerDetectTime > playerDetectRate)
        {
            lastPlayerDetectTime = Time.time;

            foreach(PlayerController player in GameManager.instance.players)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (player == targetPlayer)
                {
                    if (dist > chaseRange)
                        targetPlayer = null; 

                } else if (dist < chaseRange)
                {
                    if (targetPlayer == null)
                        targetPlayer = player;
                }
            }
        }
    }

    [PunRPC]
    public void TakeDamage (int damage)
    {
        curHP -= damage;
        healthBar.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHP);
        if (curHP <= 0)
            Die();
        else
        {
            photonView.RPC("FlashDamage", RpcTarget.All);
        }
    }

    [PunRPC]
    void FlashDamage()
    {
        StartCoroutine(DamageFlash());
        IEnumerator DamageFlash()
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.05f);
            sr.color = Color.white;
        }
    }

    void Die()
    {
        if (objectToSpawnOnDeath != string.Empty)
            PhotonNetwork.Instantiate(objectToSpawnOnDeath, transform.position, Quaternion.identity);
        photonView.RPC("DestroyPickup", RpcTarget.All);

    }
    [PunRPC]
    void DestroyPickup()
    {
        Destroy(this.gameObject);
    }
}
