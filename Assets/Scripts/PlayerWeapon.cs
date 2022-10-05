using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Stats")]
    public int damage;
    public int curAmmo;
    public int maxAmmo;
    public float bulletSpeed;
    public float shootRate;
    public int gunActive;
    //0 = pistol, 1 = burst rifle, 2 = sniper

    private float lastShootTime;

    public GameObject bulletPrefab;
    public GameObject sniperPrefab;
    public Transform bulletSpawnPos;
    public Transform sniperSpawnPos;

    private PlayerController player;

    void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    public void TryShoot()
    {
        if (curAmmo <= 0 || Time.time - lastShootTime < shootRate)
            return;
        if (gunActive == 1)
            curAmmo -= 3;
        else
            curAmmo--;
        lastShootTime = Time.time;

        GameUI.instance.UpdateAmmoText();

        if(gunActive != 2)
            player.photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.position, Camera.main.transform.forward);
        else
            player.photonView.RPC("SpawnBullet", RpcTarget.All, sniperSpawnPos.position, Camera.main.transform.forward);
    }

    [PunRPC]
    void SpawnBullet(Vector3 pos, Vector3 dir)
    {
        switch (gunActive)
        {
            case 0:
                GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
                bulletObj.transform.forward = dir;

                Bullet bulletScript = bulletObj.GetComponent<Bullet>();

                bulletScript.Initialize(damage, player.id, player.photonView.IsMine);
                bulletScript.rig.velocity = dir * bulletSpeed;
                break;
            case 1:
                StartCoroutine(BurstCoroutine());
                
                IEnumerator BurstCoroutine()
                {
                    GameObject bulletObj1 = Instantiate(bulletPrefab, pos, Quaternion.identity);
                    bulletObj1.transform.forward = dir;

                    Bullet bulletScript1 = bulletObj1.GetComponent<Bullet>();

                    bulletScript1.Initialize(damage, player.id, player.photonView.IsMine);
                    bulletScript1.rig.velocity = dir * bulletSpeed;

                    yield return new WaitForSeconds(0.1f);

                    GameObject bulletObj2 = Instantiate(bulletPrefab, pos, Quaternion.identity);
                    bulletObj2.transform.forward = dir;

                    Bullet bulletScript2 = bulletObj2.GetComponent<Bullet>();

                    bulletScript2.Initialize(damage, player.id, player.photonView.IsMine);
                    bulletScript2.rig.velocity = dir * bulletSpeed;

                    yield return new WaitForSeconds(0.1f);

                    GameObject bulletObj3 = Instantiate(bulletPrefab, pos, Quaternion.identity);
                    bulletObj3.transform.forward = dir;

                    Bullet bulletScript3 = bulletObj3.GetComponent<Bullet>();

                    bulletScript3.Initialize(damage, player.id, player.photonView.IsMine);
                    bulletScript3.rig.velocity = dir * bulletSpeed;
                }
                break;
            case 2:
                GameObject sniperObj = Instantiate(sniperPrefab, pos, Quaternion.identity);
                sniperObj.transform.forward = dir;

                Bullet sniperScript = sniperObj.GetComponent<Bullet>();

                sniperScript.Initialize(damage, player.id, player.photonView.IsMine);
                sniperScript.rig.velocity = dir * bulletSpeed;
                break;
        }
    }

    [PunRPC]
    public void GiveAmmo(int ammoToGive)
    {
        curAmmo = Mathf.Clamp(curAmmo + ammoToGive, 0, maxAmmo);

        GameUI.instance.UpdateAmmoText();
    }

    public void SwitchGun(int gunSwitch)
    {
        switch (gunSwitch)
        {
            case 0:
                gunActive = 0;
                shootRate = 0.2f;
                damage = 13;
                break;
            case 1:
                gunActive = 1;
                shootRate = 0.75f;
                damage = 10;
                break;
            case 2:
                gunActive = 2;
                shootRate = 1.5f;
                damage = 70;
                bulletSpeed = 50;
                break;

        }
    }
}
