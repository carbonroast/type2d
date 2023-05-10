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
    [SerializeField] private Slider slider;
    [SerializeField] private Transform scoreTransform;
    [SerializeField] private Transform defeatTransform;

    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        if(!IsOwner) return;
        canvas.SetActive(true);
        slider = canvas.GetComponentInChildren<Slider>();
        scoreTransform = canvas.transform.Find("Score");
        defeatTransform = canvas.transform.Find("Defeat");


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
        scoreTransform.GetComponent<TextMeshProUGUI>().text = "Score: " + score.ToString();
    }

    public void SetDefeat()
    {
        defeatTransform.gameObject.SetActive(true);
        Debug.Log($"Defeated!");
        defeatTransform.GetComponent<TextMeshProUGUI>().text = "Defeat!";
    }
}
