using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;


[System.Serializable]
public class WordController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI textUI;
  
    public NetworkVariable<Word> word = new NetworkVariable<Word>(
        new Word {
            phrase = "init",
            speed = 0,
            pointValue = 999,
            clientId = 999,
            damage = -999,
        }
    );

    public override void OnNetworkSpawn()
    {  

        word.OnValueChanged += (Word oldValue, Word newValue) => {
            Debug.Log("Word: " + newValue.phrase + " Speed: " + newValue.speed);
            textUI.text = newValue.phrase;
        };

    }
    // Update is called once per frame
    void Update()
    {
        transform.position += (Vector3.down) * word.Value.speed * Time.deltaTime;
    }

    public void Destroy()
    {
        GetComponent<NetworkObject>().Despawn();
        //Destroy(this.gameObject);
    }


}
