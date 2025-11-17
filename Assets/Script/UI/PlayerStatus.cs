using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI attackSpeedText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI scoreText;

    public PlayerController player;

    public DavidDie davidDie;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

    }

    // Update is called once per frame
    void Update()
    {
        healthText.text = "血糖:" + player.Health.ToString();
        attackText.text = "血脂:" + player.Attack.ToString();
        attackSpeedText.text = "血压:" + player.AttackSpeed.ToString();
        speedText.text = "半月板强度:" + player.currentSpeed.ToString();
        scoreText.text = "得分:" + davidDie.currentExp.ToString();
    }
}