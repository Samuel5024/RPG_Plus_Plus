using UnityEngine;
using Photon.Pun;

public enum PickupType
{
    Gold,
    Health
}

public class Pickup : MonoBehaviourPun
{
    public PickupType type;
    public int value;

    // We'll check OnTriggerEnter@d function to detect if a player has picked it up
    //the master client will check this & send the respective RPC to the player who entered the trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
        if(collision.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (type == PickupType.Gold)
            {
                player.photonView.RPC("GiveGold", player.photonPlayer, value);
            }
            else if(type == PickupType.Health)
            {
                player.photonView.RPC("Heal", player.photonPlayer, value);
            }

            PhotonNetwork.Destroy(gameObject);
        }
    }
}
