using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;
using System.Linq;
using System.Text;

public class PlayerController : NetworkBehaviour
{
    public NetworkVariable<float> hp = new NetworkVariable<float>(999); 
    public NetworkVariable<float> score = new NetworkVariable<float>(999); 
    public NetworkVariable<float> combo = new NetworkVariable<float>(999); 

    private TMP_InputField inputField;
    private GameObject server;
    private GameObject spawner;
    private ulong clientId;

    public List<ulong> wordNetworkIdList = new List<ulong>();


    // Start is called before the first frame update
    void Awake() 
    {

        inputField = GameObject.Find("InputField").GetComponent<TMP_InputField>();
        server = GameObject.Find("Server");
        spawner = GameObject.Find("SpawnPositions");

    }
    void Start()
    {
        inputField.Select();
    }

    // Update is called once per frame
    void Update()
    {

        if(!IsOwner) return;
            if(!inputField.isFocused)
            {
                inputField.Select();
                inputField.ActivateInputField();
            }

            if(Input.anyKeyDown)
            {
                ColorInputText();
            }
            if(Input.GetKeyDown(KeyCode.Return))
            {
                SendWordToServerRpc(inputField.text, clientId);
                inputField.text = string.Empty;
            }

    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            var spawnPointManager = NetworkManager.gameObject.GetComponent<NetworkManagerSpawnPoint>();
            if(spawnPointManager != null)
            {
                this.transform.position = spawnPointManager.GetSpawnPoint();
                RegisterPlayerPosition();
            }
        }
        RegisterPlayer();
        hp.OnValueChanged += (float oldValue, float newValue) => {
            GetComponent<PlayerUI>().SetHP(newValue);
        };
        score.OnValueChanged += (float oldValue, float newValue) => {
            GetComponent<PlayerUI>().SetScore(newValue);
        };
        combo.OnValueChanged += (float oldValue, float newValue) => {
            GetComponent<PlayerUI>().SetCombo(newValue);
        };

    }

    private void RegisterPlayer()
    {
        server.GetComponent<ServerController>().AddPlayer(this.gameObject);
    }

    private void RegisterPlayerPosition()
    {
        server.GetComponent<ServerController>().RegisterPlayerLocation(this.gameObject);
        Debug.Log($"RegisterPlayerPosition {this.transform.position}");
    }

    [ServerRpc]
    private void SendWordToServerRpc(string input, ulong clientID)
    {
        server.GetComponent<ServerController>().CheckWord(input, clientID);
    }

    public ulong GetClientId()
    {
        return clientId;
    }

    public void RegisterWordNetworkId(ulong networkId)
    {
        if(!IsOwner) return;
        wordNetworkIdList.Add(networkId);
    }
    public void RemoveWordNetworkId(ulong networkId)
    {
        if(!IsOwner) return;
        wordNetworkIdList.Remove(networkId);
    }
    public void ColorInputText()
    {
        
            foreach(ulong networkId in wordNetworkIdList)
            {
                NetworkObject go = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkId];
                string phrase = go.GetComponent<WordController>().word.Value.phrase;
                Debug.Log($"text: {phrase} length: {phrase.Length}");
                Debug.Log($"input length: {inputField.text.Length}");
                int wordCount = FindSmaller(phrase.Length, inputField.text.Length);
                Debug.Log($"Smallest word: {wordCount}");
                
                int i = 0;
                while(wordCount > i && phrase[i] == inputField.text[i] )
                {
                    i++;
                }
                string displayText = "<color=#FF0000>"+phrase.Insert(i,"</color>");
                go.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = displayText;
            }
    }
    public int FindSmaller(int word, int wordtwo)
    {
        return word > wordtwo ? wordtwo : word;
    }
}
