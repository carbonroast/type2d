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

    public void RemoveFromDictAndDestroy(ulong clientId, ulong networkId)
    {
        //Debug.Log("Removed NetworkObjectID: " + networkId);
        wordDict[clientId].Remove(networkId);
        GameObject wordGo = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkId].gameObject;
        wordGo.GetComponent<WordController>().Destroy();

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
            combo = 0,
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
            RemoveFromDictAndDestroy(clientId, wordNetworkId);   
        }
    }

    public void CalculateScore(PlayerStats playerStats, GameObject wordGo)
    {
        float pointValue = wordGo.GetComponent<WordController>().word.Value.pointValue;
        float total = pointValue * playerStats.comboMultiplier;
        AddCombo(playerStats);
        playerStats.score += total;
        playerStats.go.GetComponent<PlayerController>().score.Value = playerStats.score;
        UpdateOpponentsClientRpc(playerStats.clientId);
        Debug.Log("Player: " + playerStats.clientId + " Added " + total.ToString() + " for a total Score of: " + playerStats.score.ToString());
    }

    public void AddCombo(PlayerStats playerStats)
    {
        playerStats.combo += 1;
        playerStats.go.GetComponent<PlayerController>().combo.Value = playerStats.combo;
        if(playerStats.combo < 10) 
        {
            playerStats.comboMultiplier = 1;
        }
        else
        {
            playerStats.comboMultiplier = Mathf.FloorToInt(playerStats.combo / 10);
        }
        
    }

    public void ClearCombo(PlayerStats playerStats)
    {
        playerStats.go.GetComponent<PlayerController>().combo.Value = 0;
        playerStats.combo = 0;
    }

    private void ChangeHP(PlayerStats playerStats, float damage)
    {
        playerStats.hp += damage;
        playerStats.go.GetComponent<PlayerController>().hp.Value = playerStats.hp;
        ClearCombo(playerStats);
        Debug.Log("Healing: " + damage.ToString() + " to a total of " + playerStats.hp.ToString() + " hp.");
        UpdateOpponentsClientRpc(playerStats.clientId);
        if (playerStats.hp <= 0)
        {
            SetDefeat(playerStats);
        }

    }

    public void Damage(ulong clientId, float hp)
    {
        ChangeHP(playerStatsDict[clientId], hp);
    }

    public void SetDefeat(PlayerStats playerStats)
    {
        playerStats.go.GetComponent<PlayerUI>().SetDefeat();
    }


    [ClientRpc]
    public void UpdateOpponentsClientRpc(ulong clientID)
    {

        float score = playerStatsDict[clientID].score;
        float hp = playerStatsDict[clientID].hp;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerUI>().SetOpponentScore(score);
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerUI>().SetOpponentHp(hp);
        }

    }

}
