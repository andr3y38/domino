// File: _Scripts/UI/UIManager.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Text Displays")]
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI betText;
    public TextMeshProUGUI winText;

    [Header("Buttons")]
    public Button betButton;
    public Button increaseBetButton;
    public Button decreaseBetButton;

    [Header("Toggles")]
    public Toggle turboToggle;
    public Toggle instantToggle;

    void OnEnable()
    {
        GameManager.OnBalanceChanged += UpdateBalanceText;
        GameManager.OnBetChanged += UpdateBetText;
    }

    void OnDisable()
    {
        GameManager.OnBalanceChanged -= UpdateBalanceText;
        GameManager.OnBetChanged -= UpdateBetText;
    }

    void Start()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.SetIdleState();
        }
        winText.gameObject.SetActive(false);
    }

    private void UpdateBalanceText(float newBalance) { balanceText.text = $"BALANCE: ${newBalance:N2}"; }
    private void UpdateBetText(float newBet) { betText.text = $"BET: ${newBet:N2}"; }

    public void OnBetButtonPressed() { GameManager.Instance.StartPlay(turboToggle.isOn, instantToggle.isOn); }
    public void OnIncreaseBetPressed() { GameManager.Instance.AdjustBet(true); }
    public void OnDecreaseBetPressed() { GameManager.Instance.AdjustBet(false); }

    public IEnumerator ShowWinResult(float winAmount)
    {
        winText.text = $"YOU WON\n${winAmount:N2}";
        winText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        winText.gameObject.SetActive(false);
    }
}