using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Linq;

public class ServerController : NetworkBehaviour
{

    private Dictionary<ulong, Dictionary<ulong,string>> wordDict = new Dictionary<ulong, Dictionary<ulong, string>>();

    private Dictionary<ulong, PlayerStats> playerStatsDict = new Dictionary<ulong, PlayerStats>();

    private Dictionary<ulong, Vector3> playerSpawnPosition = new Dictionary<ulong, Vector3>();
    
    [SerializeField]private GameObject spawnGameObjectPrefab;

    private List<string> wordList = new List<string>(){"test","network","why"};

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsServer) return;
        if(Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SpawnWords();
        }
        if(Input.GetKeyDown(KeyCode.Keypad6))
        {
            foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                Debug.Log("Connected Clients are : " + clientId.ToString());
            }
        }
        if(Input.GetKeyDown(KeyCode.Keypad9))
        {
            foreach(var kvp in wordDict)
            {
                Debug.Log("Client " + kvp.Key.ToString() + " has " + kvp.Value.Count + " words.");
            }
        }
    }

    public override void OnNetworkSpawn()
    {

    }
   
    private void SpawnWords()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
        StartCoroutine(SpawnWord(clientId));
        }
    }

    IEnumerator SpawnWord( ulong clientId)
    {
        Debug.Log($"Player {clientId} is at {playerSpawnPosition[clientId]}");
        foreach (string word in wordList)
        {
            Vector3 randomPosition = new Vector3(Random.Range(-5,5),Random.Range(6,7),Random.Range(-2,2)) + playerSpawnPosition[clientId];
            GameObject wordGO = Instantiate(spawnGameObjectPrefab, randomPosition, Quaternion.identity);
            wordGO.GetComponent<NetworkObject>().Spawn();
            wordGO.GetComponent<WordController>().word.Value = new Word {
                phrase = word,
                speed = 1,
                clientId = clientId,
                pointValue = 1,
                damage = -1,
            };
                            
            AddToDict(clientId, wordGO.GetComponent<NetworkObject>().NetworkObjectId, word);
            yield return new WaitForSeconds(1.5f);
        }

    }
    private void AddToDict(ulong clientId, ulong networkId, string word)
    {
        // Words exist for client already, just add to list
        if (wordDict.ContainsKey(clientId))
        {
            wordDict[clientId].Add(networkId, word);
        }
        // Create Dictionary entry
        else
        {
            wordDict.Add(clientId,new Dictionary<ulong, string>(){{networkId, word}});
        }
    }

    public void RemoveFromDict(ulong clientId, ulong networkId)
    {
        Debug.Log("Removed NetworkObjectID: " + networkId);
        wordDict[clientId].Remove(networkId);
    }
    public Dictionary<ulong, Dictionary<ulong,string>> GetWordDictionary()
    {
        return wordDict;
    }

    public void AddPlayer(GameObject go)
    {
        ulong clientId = go.GetComponent<NetworkObject>().OwnerClientId;
        PlayerStats playerstat = new PlayerStats() {
            hp = 10,
            score = 0,
            comboMultiplier = 1,
            comboNumber = 0,
            go = go,
            clientId = clientId,
        };
        Debug.Log("Added player " + clientId);
        playerStatsDict[clientId] = playerstat;
        
    }

    public void RegisterPlayerLocation(GameObject go)
    {
        ulong clientId = go.GetComponent<NetworkObject>().OwnerClientId;
        playerSpawnPosition[clientId] = go.transform.position;
    }
    public void CheckWord(string clientInput, ulong clientId)
    {
        Debug.Log("Client: " + clientId + "\n" + " Word: " + clientInput);


        if(wordDict.ContainsKey(clientId) && wordDict[clientId].ContainsValue(clientInput))
        {
            ulong wordNetworkId = wordDict[clientId].FirstOrDefault(x => x.Value == clientInput).Key;
            GameObject wordGo = NetworkManager.Singleton.SpawnManager.SpawnedObjects[wordNetworkId].gameObject;
            CalculateScore(playerStatsDict[clientId], wordGo);
            RemoveFromDict(clientId, wordNetworkId);
            wordGo.GetComponent<WordController>().Destroy();
            
            
        }
    }

    public void CalculateScore(PlayerStats playerStats, GameObject wordGo)
    {
        float pointValue = wordGo.GetComponent<WordController>().word.Value.pointValue;
        float total = pointValue * playerStats.comboMultiplier;
        playerStats.score += total;
        playerStats.go.GetComponent<PlayerController>().score.Value = playerStats.score;
        Debug.Log("Player: " + playerStats.clientId + " Added " + total.ToString() + " for a totat Score of: " + playerStats.score.ToString());
    }


    private void ChangeHP(PlayerStats playerStats, float damage)
    {
        playerStats.hp += damage;
        playerStats.go.GetComponent<PlayerController>().hp.Value = playerStats.hp;
        Debug.Log("Healing: " + damage.ToString() + " to a total of " + playerStats.hp.ToString() + " hp.");

    }

    public void Damage(ulong clientId, float hp)
    {
        ChangeHP(playerStatsDict[clientId], hp);
    }
}
