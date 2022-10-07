using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
    [Header("Info")]
    public int id;
    private int curAttackerId;

    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;
    public int curHp;
    public int maxHp;
    public int kills;
    public bool dead;

    private bool damaging;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;
    public PlayerWeapon weapon;
    public MeshRenderer mr;
    public GameObject viewmodelPistol;
    public GameObject otherPistol;
    public GameObject viewmodelRifle;
    public GameObject otherRifle;
    public GameObject viewmodelSniper;
    public GameObject otherSniper;
    public ParticleSystem impactParticleSystem;

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            viewmodelPistol.SetActive(false);
            rig.isKinematic = true;
        }
        else
        {
            GameUI.instance.Initialize(this);
            otherPistol.SetActive(false);
        }
    }

    void Update()
    {
        if (!photonView.IsMine || dead)
            return;

        Move();

        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();

        if (Input.GetMouseButtonDown(0))
            weapon.TryShoot();
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.velocity.y;

        rig.velocity = dir;
    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 1.5f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    [PunRPC]
    public void TakeDamage(int attackerId, int damage)
    {
        if (dead)
            return;

        curHp -= damage;
        curAttackerId = attackerId;

        photonView.RPC("Damage", RpcTarget.Others);

        GameUI.instance.UpdateHealthBar();

        if (curHp <= 0)
            photonView.RPC("Die", RpcTarget.All);
    }

    [PunRPC]
    void Die()
    {
        curHp = 0;
        dead = true;

        GameManager.instance.alivePlayers--;

        if (PhotonNetwork.IsMasterClient)
            GameManager.instance.CheckWinCondition();

        if (photonView.IsMine)
        {
            if (curAttackerId != 0)
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);

            GetComponentInChildren<CameraController>().SetSpectator();

            //change to function later?
            viewmodelPistol.SetActive(false);
            viewmodelRifle.SetActive(false);
            viewmodelSniper.SetActive(false);

            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);
        }
    }

    [PunRPC]
    public void AddKill()
    {
        kills++;

        GameUI.instance.UpdatePlayerInfoText();
    }

    [PunRPC]
    void Damage()
    {
        if (damaging)
            return;

        StartCoroutine(DamageCoRoutine());

        IEnumerator DamageCoRoutine()
        {
            damaging = true;

            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;
            impactParticleSystem.Play();

            yield return new WaitForSeconds(0.05f);

            damaging = false;

            impactParticleSystem.Clear();
            mr.material.color = defaultColor;
        }
    }

    [PunRPC]
    public void Heal(int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);

        GameUI.instance.UpdateHealthBar();
    }

    [PunRPC]
    public void GunModelSwitch(int gunActive)
    {
        switch (gunActive)
        {
            case 0:
                if (!photonView.IsMine)
                {
                    viewmodelPistol.SetActive(false);
                    viewmodelRifle.SetActive(false);
                    viewmodelSniper.SetActive(false);
                    otherPistol.SetActive(true);
                    otherRifle.SetActive(false);
                    otherSniper.SetActive(false);
                }
                else
                {
                    viewmodelPistol.SetActive(true);
                    viewmodelRifle.SetActive(false);
                    viewmodelSniper.SetActive(false);
                    otherPistol.SetActive(false);
                    otherRifle.SetActive(false);
                    otherSniper.SetActive(false);
                }
                break;

            case 1:
                if (!photonView.IsMine)
                {
                    viewmodelPistol.SetActive(false);
                    viewmodelRifle.SetActive(false);
                    viewmodelSniper.SetActive(false);
                    otherPistol.SetActive(false);
                    otherRifle.SetActive(true);
                    otherSniper.SetActive(false);
                }
                else
                {
                    viewmodelPistol.SetActive(false);
                    viewmodelRifle.SetActive(true);
                    viewmodelSniper.SetActive(false);
                    otherPistol.SetActive(false);
                    otherRifle.SetActive(false);
                    otherSniper.SetActive(false);
                }
                break;

            case 2:
                if (!photonView.IsMine)
                {
                    viewmodelPistol.SetActive(false);
                    viewmodelRifle.SetActive(false);
                    viewmodelSniper.SetActive(false);
                    otherPistol.SetActive(false);
                    otherRifle.SetActive(false);
                    otherSniper.SetActive(true);
                }
                else
                {
                    viewmodelPistol.SetActive(false);
                    viewmodelRifle.SetActive(false);
                    viewmodelSniper.SetActive(true);
                    otherPistol.SetActive(false);
                    otherRifle.SetActive(false);
                    otherSniper.SetActive(false);
                }
                break;
        }
    }
}
