using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private UIManager() { }

    private static UIManager instance = null;

    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UIManager();
            }
            return instance;
        }
    }

    public GameObject m_RewardAmount;

    TextMeshPro m_RewardAmountText;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        m_RewardAmountText = m_RewardAmount.GetComponent<TextMeshPro>();
    }

    public void SetRewardAmountText(string text)
    {
        m_RewardAmountText.text = "REWARD: " + text;
    }
}
