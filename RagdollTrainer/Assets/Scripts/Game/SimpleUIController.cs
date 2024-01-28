using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimpleUIController : MonoBehaviour
{
    public GameObject m_RewardAmount;

    TextMeshPro m_RewardAmountText;

    private void Start()
    {
        m_RewardAmountText = m_RewardAmount.GetComponent<TextMeshPro>();
    }

    public void SetRewardAmountText(string text)
    {
        m_RewardAmountText.text = text;
    }

    public void SetRewardAmountText(float value)
    {
        SetRewardAmountText(value.ToString("F2"));
    }
}
