using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DamageZoneController : NetworkBehaviour
{
    private GameObject server;
    // Start is called before the first frame update
    void Awake()
    {
        server = GameObject.Find("Server");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Word")
        {
            //Debug.Log("Triggered Object NetworkID:" + other.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
            ulong clientId = other.GetComponent<WordController>().word.Value.clientId;
            float damage = other.GetComponent<WordController>().word.Value.damage;
            DamagePlayerServerRpc(clientId, damage);
            ulong networkId = other.GetComponent<NetworkObject>().NetworkObjectId;
            RemovefromServerDictServerRpc(clientId, networkId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DamagePlayerServerRpc(ulong clientId, float damage)
    {      
        server.GetComponent<ServerController>().Damage(clientId, damage);        
    }

    [ServerRpc(RequireOwnership = false)]
    void RemovefromServerDictServerRpc(ulong clientId, ulong networkId)
    {
        server.GetComponent<ServerController>().RemoveFromDictAndDestroy(clientId, networkId);
    }


}
