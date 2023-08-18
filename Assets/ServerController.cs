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
    public void CheckWord(string clientInput, ulong clientObjId)
    {
        Debug.Log("Client: " + clientObjId + "\n" + " Word: " + clientInput);


        if(wordDict.ContainsKey(clientObjId) && wordDict[clientObjId].ContainsValue(clientInput))
        {
            ulong wordNetworkId = wordDict[clientObjId].FirstOrDefault(x => x.Value == clientInput).Key;
            GameObject wordGo = NetworkManager.Singleton.SpawnManager.SpawnedObjects[wordNetworkId].gameObject;
            CalculateScore(clientObjId, wordGo);
            RemoveFromDictAndDestroy(clientObjId, wordNetworkId);   
        }
    }

    public void CalculateScore(ulong clientObjId, GameObject wordGo)
    {
        float pointValue = wordGo.GetComponent<WordController>().word.Value.pointValue;
        float total = pointValue * playerStatsDict[clientObjId].comboMultiplier;
        AddCombo(clientObjId);
        playerStatsDict[clientObjId].score += total;
        playerStatsDict[clientObjId].go.GetComponent<PlayerController>().score.Value = playerStatsDict[clientObjId].score;
        UpdateOpponentsValues(clientObjId);
        Debug.Log("Player: " + clientObjId + " Added " + total.ToString() + " for a total Score of: " + playerStatsDict[clientObjId].score.ToString());
    }

    public void AddCombo(ulong clientObjId)
    {
        playerStatsDict[clientObjId].combo += 1;
        playerStatsDict[clientObjId].go.GetComponent<PlayerController>().combo.Value = playerStatsDict[clientObjId].combo;
        if(playerStatsDict[clientObjId].combo < 10) 
        {
            playerStatsDict[clientObjId].comboMultiplier = 1;
        }
        else
        {
            playerStatsDict[clientObjId].comboMultiplier = Mathf.FloorToInt(playerStatsDict[clientObjId].combo / 10);
        }
        
    }

    public void ClearCombo(ulong clientObjId)
    {
        playerStatsDict[clientObjId].go.GetComponent<PlayerController>().combo.Value = 0;
        playerStatsDict[clientObjId].combo = 0;
    }

    private void ChangeHP(ulong clientObjId, float damage)
    {
        playerStatsDict[clientObjId].hp += damage;
        playerStatsDict[clientObjId].go.GetComponent<PlayerController>().hp.Value = playerStatsDict[clientObjId].hp;
        ClearCombo(clientObjId);
        Debug.Log("Healing: " + damage.ToString() + " to a total of " + playerStatsDict[clientObjId].hp.ToString() + " hp.");
        UpdateOpponentsValues(clientObjId);
        if (playerStatsDict[clientObjId].hp <= 0)
        {
            SetDefeat(clientObjId);
        }

    }

    public void Damage(ulong clientObjId, float hp)
    {
        ChangeHP(clientObjId, hp);
    }

    public void SetDefeat( ulong clientObjId)
    {
        playerStatsDict[clientObjId].go.GetComponent<PlayerUI>().SetDefeat();
    }



    public void UpdateOpponentsValues(ulong clientObjId)
    {
        float score = playerStatsDict[clientObjId].score;
        float hp = playerStatsDict[clientObjId].hp;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if( clientId != clientObjId)
            {
                UpdateOpponentsValuesClientRpc(clientObjId, clientId, score, hp);
            }

        }

    }

    [ClientRpc]
    public void UpdateOpponentsValuesClientRpc(ulong senderId, ulong clientId,float score, float hp)
    {
        playerStatsDict[clientId].go.GetComponent<PlayerUI>().SetOpponentInfo(senderId, score, hp);
    }
}
