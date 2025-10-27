using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Enemy : MonoBehaviourPun
{
    [Header("Info")]
    public string enemyName;
    public float moveSpeed;

    public int curHp;
    public int maxHp;

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

    void Start()
    {
        // initialize the health bar
        healthBar.Initialize(enemyName, maxHp);
        
    }

    void Update()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
        if(targetPlayer != null)
        {
            // calculate the distance
            float dist = Vector3.Distance(transform.position, targetPlayer.transform.position);

            // if we're able to attack, do so
            if(dist < attackRange && Time.time - lastAttackTime >= attackRange)
            {
                Attack();
            }
            //otherwise, do we move after the player
            else if(dist > attackRange)
            {
                Vector3 dir = targetPlayer.transform.position - transform.position;
                rig.linearVelocity = dir.normalized * moveSpeed;
            }
            else
            {
                rig.linearVelocity = Vector2.zero;
            }

        }
        DetectPlayer();
    }

    // attacks the targeted player
    void Attack()
    {
        lastAttackTime = Time.time;
        targetPlayer.photonView.RPC("TakeDamage", targetPlayer.photonPlayer, damage);
    }

    // check if a player is within the chase range. If so, target them.
    void DetectPlayer()
    {
        if(Time.time - lastPlayerDetectTime > playerDetectRate)
        {
            lastPlayerDetectTime = Time.time;
        }

        // loop through all the players
        foreach(PlayerController player in GameManager.instance.players)
        {
            // calculate distance between us and the player
            float dist = Vector2.Distance(transform.position, player.transform.position);

            if(player == targetPlayer)
            {
                if(dist > chaseRange)
                {
                    targetPlayer = null;
                }
            }
            else if(dist < chaseRange)
            {
                if(targetPlayer == null)
                {
                    targetPlayer = player;
                }
            }
        }
    }

    // TakeDamage() is called when the enemy gets attacked by a player
    [PunRPC]
    public void TakeDamage(int damage)
    {
        curHp -= damage;

        // update the health bar
        healthBar.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);

        if(curHp <= 0)
        {
            Die();
        }
        else
        {
            photonView.RPC("FlashDamage", RpcTarget.All);
        }
    }

    // FlashDamage() is called when enemy is hit. It flashes the enemy across the network.
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

    // Die() called when the enemy's health reaches 0
    void Die()
    {
        if(objectToSpawnOnDeath != string.Empty)
        {
            PhotonNetwork.Instantiate(objectToSpawnOnDeath, transform.position, Quaternion.identity);
        }

        // destroy the object across the network
        PhotonNetwork.Destroy(gameObject);
    }
}
