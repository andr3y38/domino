// File: _Scripts/Core/GameManager.cs

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DominoBash.Models;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<float> OnBalanceChanged;
    public static event Action<float> OnBetChanged;
    public static event Action<string> OnAuthenticationFailed;
    public static event Action OnAuthenticationComplete;
    
    public enum GameState { Preloading, Authenticating, ResolvingRound, Menu, Idle, Playing, ShowingResult }
    public GameState CurrentState { get; private set; }

    private float _balance;
    public float Balance
    {
        get => _balance;
        private set { _balance = value; OnBalanceChanged?.Invoke(_balance); }
    }

    private long _currentBetInCents = 1000000; // Stored as integer cents
    public float CurrentBet
    {
        get => _currentBetInCents / (float)ApiService.API_CURRENCY_MULTIPLIER;
    }

    [SerializeField] private long[] betLevels = { 1000000 }; // Default fallback, will be replaced by server config
    private int currentBetIndex = 0;

    // API Config
    [SerializeField] private string fallbackRgsUrl = "https://api.example.com";
    [SerializeField] private string fallbackSessionID = "dev-session";
    [SerializeField] private string fallbackLanguage = "en";
    [SerializeField] private string fallbackCurrency = "USD";
    [SerializeField] private string fallbackMode = "BASE";
    private string rgsUrl, sessionID;
    
    // Scene-specific References
    private UIManager uiManager;
    private spawner _spawner;
    private PlayResponse lastPlayResponse;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
        CurrentState = GameState.Preloading;
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeConfig();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGameScene")
        {
            uiManager = FindObjectOfType<UIManager>();
            _spawner = FindObjectOfType<spawner>();

            if (CurrentState == GameState.ResolvingRound)
            {
                StartCoroutine(ResolveActiveRound());
            }
        }
    }
    
    public void StartAuthentication()
    {
        if (CurrentState != GameState.Preloading && CurrentState != GameState.Authenticating) return;
        CurrentState = GameState.Authenticating;

        ApiService.Authenticate(this, rgsUrl, sessionID, fallbackLanguage, 
        (response) => {
            Balance = response.balance.amount / (float)ApiService.API_CURRENCY_MULTIPLIER;

            // Use the server's bet levels instead of our hardcoded ones.
            if (response.config?.betLevels != null && response.config.betLevels.Length > 0)
            {
                this.betLevels = response.config.betLevels;
                // Find a sensible default bet index (e.g., the 6th one or 1.00)
                currentBetIndex = Mathf.Clamp(5, 0, betLevels.Length - 1);
                _currentBetInCents = betLevels[currentBetIndex];
            }

            if (response.round != null && response.round.active)
            {
                Debug.Log("Active round detected! Preparing to resolve.");
                lastPlayResponse = new PlayResponse { round = response.round, balance = response.balance };
                CurrentState = GameState.ResolvingRound;
            }
            else
            {
                CurrentState = GameState.Menu;
            }
            
            OnAuthenticationComplete?.Invoke();
        }, 
        (error) => {
            CurrentState = GameState.Preloading;
            OnAuthenticationFailed?.Invoke(error);
        });
    }

    private IEnumerator ResolveActiveRound()
    {
        Debug.Log("Resolving active round in MainGameScene...");
        yield return new WaitForSeconds(1f);
        
        // Show the presentation for the unresolved round
    }
    
    public void SetIdleState()
    {
        CurrentState = GameState.Idle;
        OnBalanceChanged?.Invoke(Balance);
        OnBetChanged?.Invoke(CurrentBet);
    }

    public void AdjustBet(bool increase)
    {
        if (CurrentState != GameState.Idle) return;
        if (increase)
        {
            if (currentBetIndex < betLevels.Length - 1) currentBetIndex++;
        }
        else
        {
            if (currentBetIndex > 0) currentBetIndex--;
        }
        _currentBetInCents = betLevels[currentBetIndex];
        OnBetChanged?.Invoke(CurrentBet);
    }

    public void StartPlay(bool isTurbo, bool isInstant)
    {
        if (CurrentState != GameState.Idle || Balance < CurrentBet) return;
        CurrentState = GameState.Playing;
        Balance -= CurrentBet;

        ApiService.Play(this, rgsUrl, sessionID, fallbackCurrency, fallbackMode, _currentBetInCents, 
        (response) => {
            lastPlayResponse = response;
        }, 
        (error) => {
            Balance += CurrentBet;
            CurrentState = GameState.Idle;
        });
    }

    private void HandlePresentationComplete()
    {
        float winAmount = CurrentBet * lastPlayResponse.round.payoutMultiplier;
        Balance += winAmount;
        StartCoroutine(FinalizeRoundSequence(winAmount));
    }
    
    private IEnumerator FinalizeRoundSequence(float winAmount)
    {
        if (winAmount > 0 && uiManager != null)
        {
            yield return StartCoroutine(uiManager.ShowWinResult(winAmount));
        }

        if (lastPlayResponse.round.payoutMultiplier > 0)
        {
            EndRoundAfterWin(lastPlayResponse.balance);
        }
        else
        {
            CurrentState = GameState.Idle;
        }
    }

    private void EndRoundAfterWin(Balance finalBalance)
    {
        ApiService.EndRound(this, rgsUrl, sessionID,
        (response) => {
            Balance = response.balance.amount / (float)ApiService.API_CURRENCY_MULTIPLIER;
            CurrentState = GameState.Idle;
        },
        (error) => { CurrentState = GameState.Idle; });
    }
    
    private void InitializeConfig()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            string absoluteUrl = Application.absoluteURL;
            rgsUrl = "https://" + GetQueryParam(absoluteUrl, "rgs_url");
            sessionID = GetQueryParam(absoluteUrl, "sessionID");
        #else
            rgsUrl = fallbackRgsUrl;
            sessionID = fallbackSessionID;
        #endif
    }
    
    private string GetQueryParam(string url, string key)
    {
        if (string.IsNullOrEmpty(url)) return null;
        try { var uri = new Uri(url); var query = uri.Query; if (string.IsNullOrEmpty(query)) return null;
            var pairs = query.TrimStart('?').Split('&');
            foreach (var p in pairs) { var kv = p.Split(new[]{'='}, 2);
                if (kv.Length == 2 && Uri.UnescapeDataString(kv[0]) == key)
                    return Uri.UnescapeDataString(kv[1]);
            }
        } catch {}
        return null;
    }
}