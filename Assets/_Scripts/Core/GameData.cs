// File: _Scripts/Core/GameData.cs

using System;

namespace DominoBash.Models
{
    [Serializable]
    public class Balance { public long amount; }

    // Represents the 'round' object from both play and authenticate responses
    [Serializable]
    public class RoundData
    {
        public bool active;
        public float payoutMultiplier;
        public string state;
    }
    
    // NEW: This holds the server-provided configuration
    [Serializable]
    public class GameConfig
    {
        public long[] betLevels;
    }

    // EXPANDED: The full response from the /wallet/authenticate endpoint
    [Serializable]
    public class WalletAuthenticateResponse
    {
        public Balance balance;
        public RoundData round;
        public GameConfig config;
    }

    // This can remain the same
    [Serializable]
    public class PlayResponse
    {
        public RoundData round;
        public Balance balance;
    }

    [Serializable]
    public class EndRoundResponse
    {
        public Balance balance;
    }
}