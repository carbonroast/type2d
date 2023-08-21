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
    [SerializeField] private Slider hpTransform;
    [SerializeField] private Transform scoreTransform;
    [SerializeField] private Transform comboTransform;
    [SerializeField] private Transform defeatTransform;
    [SerializeField] private Transform oppScoreTransform;
    [SerializeField] private Slider oppHpTransform;
    [SerializeField] private Transform oppComboTransform;

    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        if(!IsOwner) return;
        canvas.SetActive(true);
        hpTransform = canvas.transform.Find("Hp").GetComponent<Slider>();
        scoreTransform = canvas.transform.Find("Score");
        comboTransform = canvas.transform.Find("Combo");
        defeatTransform = canvas.transform.Find("Defeat");
        oppScoreTransform = canvas.transform.Find("OpponentScore");
        oppHpTransform = canvas.transform.Find("OpponentHP").GetComponent<Slider>();
        oppComboTransform = canvas.transform.Find("OpponentCombo");


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
        Debug.Log("Player " + GetComponent<NetworkObject>().OwnerClientId.ToString() + " HP IS " + hp);
        if(!IsOwner) return;
        hpTransform.value = hp / maxHp;
            
    }

    public void SetScore(float score)
    {
        Debug.Log("Player " + GetComponent<NetworkObject>().OwnerClientId.ToString() +" SCORE IS " + score);
        if(!IsOwner) return;
        scoreTransform.GetComponent<TextMeshProUGUI>().text = "Score: " + score.ToString();
    }

    public void SetCombo(float num)
    {
        Debug.Log("Player " + GetComponent<NetworkObject>().OwnerClientId.ToString() +" COMBO IS " + num);
        if(!IsOwner) return;
        comboTransform.GetComponent<TextMeshProUGUI>().text = "Combo: " + num.ToString();
    }
    public void SetDefeat()
    {
        defeatTransform.gameObject.SetActive(true);
        Debug.Log("Player " + GetComponent<NetworkObject>().OwnerClientId.ToString() +$" is Defeated!");
        if(!IsOwner) return;
        defeatTransform.GetComponent<TextMeshProUGUI>().text = "Defeat!";
    }

    public void SetOpponentInfo (ulong clientId, float score, float combo, float hp)
    {
        Debug.Log("Opponent Player Score is: " + score);
        Debug.Log("Opponent Player hp is: " + hp);
        Debug.Log("Opponent Player Combo is: " + combo);
        if(!IsOwner) return;
        oppScoreTransform.GetComponent<TextMeshProUGUI>().text = "Opponent Score: " + score.ToString();
        oppComboTransform.GetComponent<TextMeshProUGUI>().text = "Opponent Combo: " + combo.ToString();
        oppHpTransform.value = hp / maxHp;
    }
}
