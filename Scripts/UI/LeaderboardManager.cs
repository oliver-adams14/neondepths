using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using UnityEngine;

// Represents a single entry in the leaderboard
public struct LeaderboardEntry
{
    public string PlayerId;
    public string PlayerName;
    public int Rank;
    public double Score;
}

// Handles Unity Gaming Services leaderboard integration
public class LeaderboardManager : MonoBehaviour
{
    // Singleton pattern for global access
    public static LeaderboardManager Instance { get; private set; }

    private const string LeaderboardPrefix = "Level_";
    private string _playerId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _ = Initialize();
    }

    public async Task Initialize()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            // If already initialized, let's just ensure the name is updated.
            await UpdatePlayerName();
            return;
        }

        try
        {
            Debug.Log("Initializing Unity Services...");
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Signing in anonymously...");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                _playerId = AuthenticationService.Instance.PlayerId;
                Debug.Log($"Player signed in anonymously with ID: {_playerId}");
            }
            
            _playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log($"Player signed in with ID: {_playerId}");
            await UpdatePlayerName();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
        }
    }

    private async Task UpdatePlayerName()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "");
        if (!string.IsNullOrEmpty(playerName) && AuthenticationService.Instance.PlayerName != playerName)
        {
            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
                Debug.Log($"Player name updated to: {playerName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to update player name: {e.Message}");
            }
        }
    }

    public async Task<bool> SubmitScoreAsync(int levelNumber, double score)
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogError("Unity Services not initialized. Cannot submit score.");
            await Initialize();
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                return false;
            }
        }

        string leaderboardId = LeaderboardPrefix + levelNumber;
        try
        {
            var playerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
            Debug.Log($"Score {score} submitted to {leaderboardId}. New rank: {playerEntry.Rank}");
            return true;
        }
        catch (LeaderboardsException e)
        {
            Debug.LogError($"LeaderboardsException in SubmitScoreAsync: {e.Reason}, {e.Message}");
            return false;
        }
        catch (Exception e) // Catch any other exceptions
        {
            Debug.LogError($"Failed to submit score to {leaderboardId}: {e.Message}");
            return false;
        }
    }

    public async Task<List<LeaderboardEntry>> GetScoresAsync(int levelNumber)
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogError("Unity Services not initialized. Cannot get scores.");
            await Initialize();
             if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.LogError("Initialization failed. Returning empty score list.");
                return new List<LeaderboardEntry>();
            }
        }

        string leaderboardId = LeaderboardPrefix + levelNumber;
        Debug.Log($"Getting scores for leaderboard: {leaderboardId}");
        var entries = new List<LeaderboardEntry>();
        try
        {
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId);
            Debug.Log($"Found {scoresResponse.Results.Count} scores.");
            foreach (var unityScore in scoresResponse.Results)
            {
                entries.Add(new LeaderboardEntry
                {
                    PlayerId = unityScore.PlayerId,
                    PlayerName = unityScore.PlayerName,
                    Rank = unityScore.Rank + 1, // Rank is 0-based, so add 1 for display
                    Score = unityScore.Score
                });
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get scores from {leaderboardId}: {e.Message}");
        }
        return entries;
    }
}
