using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerUI : NetworkBehaviour
{
    // WIP: remove network var, replace with just 2

    public float maxHp;

    [SerializeField] private GameObject canvas;

    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        if(!IsOwner) return;
        canvas.SetActive(true);


    }


    public override void OnNetworkSpawn()
    {

        Debug.Log("Player Connected: " + GetComponent<NetworkObject>().OwnerClientId.ToString());

    }

    void Update()
    {

        if(!IsOwner) return;
      
    }

    public void SetHP(float hp)
    {
        Debug.Log("HP IS " + hp);
        canvas.GetComponentInChildren<Slider>().value = hp / maxHp;
            
    }

    public void SetScore(float score)
    {
        Debug.Log("SCORE IS " + score);
        canvas.GetComponentInChildren<TextMeshProUGUI>().text = "Score: " + score.ToString();
    }

}
