using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class NetworkManagerSpawnPoint : MonoBehaviour
{
    public List<SpawnPoint> spawnPositions = new List<SpawnPoint>();
    private NetworkManager networkManager;
    // Start is called before the first frame update
    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        spawnPositions = FindObjectsOfType<SpawnPoint>().ToList();
    }

    public Vector3 GetSpawnPoint()
    {
        if (networkManager.IsServer)
        {
            SpawnPoint spawnPoint = spawnPositions.Find(sp => sp.isAvailable);

            if (spawnPoint != null){
                spawnPoint.isAvailable = false;
                return spawnPoint.transform.position;
            }
            
        }
        return Vector3.zero;
    }
}
