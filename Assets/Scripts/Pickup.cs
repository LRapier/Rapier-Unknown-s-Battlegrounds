using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum PickupType
{
    Health,
    Ammo,
    Weapon
}

public class Pickup : MonoBehaviour
{
    public PickupType type;
    public int value;

    void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if(other.CompareTag("Player"))
        {
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if (type == PickupType.Health)
                player.photonView.RPC("Heal", player.photonPlayer, value);
            else if (type == PickupType.Ammo)
                player.photonView.RPC("GiveAmmo", player.photonPlayer, value);
            else if (type == PickupType.Weapon)
                player.photonView.RPC("SwitchGun", player.photonPlayer, value);

            PhotonNetwork.Destroy(gameObject);
        }
    }
}
