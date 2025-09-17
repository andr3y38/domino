// File: _Scripts/Core/ApiService.cs

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using DominoBash.Models;

/// <summary>
/// A static class to handle all communication with the Stake RGS API.
/// </summary>
public static class ApiService
{
    public const long API_CURRENCY_MULTIPLIER = 1_000_000;

    public static void Authenticate(MonoBehaviour runner, string url, string sessionID, string language, Action<WalletAuthenticateResponse> onSuccess, Action<string> onError)
    {
        var body = new Dictionary<string, object>
        {
            { "sessionID", sessionID },
            { "language", string.IsNullOrEmpty(language) ? "en" : language }
        };
        runner.StartCoroutine(Post<WalletAuthenticateResponse>(url + "/wallet/authenticate", body, onSuccess, onError));
    }

    public static void Play(MonoBehaviour runner, string url, string sessionID, string currency, string mode, long amount, Action<PlayResponse> onSuccess, Action<string> onError)
    {
        var body = new Dictionary<string, object>
        {
            { "sessionID", sessionID },
            { "currency", currency },
            { "mode", string.IsNullOrEmpty(mode) ? "BASE" : mode },
            { "amount", amount }
        };
        runner.StartCoroutine(Post<PlayResponse>(url + "/wallet/play", body, onSuccess, onError));
    }

    public static void EndRound(MonoBehaviour runner, string url, string sessionID, Action<EndRoundResponse> onSuccess, Action<string> onError)
    {
        var body = new Dictionary<string, object> { { "sessionID", sessionID } };
        runner.StartCoroutine(Post<EndRoundResponse>(url + "/wallet/end-round", body, onSuccess, onError));
    }

    private static IEnumerator Post<T>(string url, Dictionary<string, object> body, Action<T> onSuccess, Action<string> onError)
    {
        string jsonBody = MiniJson.Serialize(body);
        byte[] rawBody = Encoding.UTF8.GetBytes(jsonBody);

        using (var request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"Sending POST request to: {url}\nBody: {jsonBody}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = $"API Error: {request.responseCode} - {request.error}\nResponse: {request.downloadHandler.text}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
                yield break;
            }

            string jsonResponse = request.downloadHandler.text;
            Debug.Log($"Received response: {jsonResponse}");

            try
            {
                T result = JsonUtility.FromJson<T>(jsonResponse);
                onSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                string errorMsg = $"JSON Deserialization Error: {e.Message}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
        }
    }
}

public static class MiniJson
{
    public static string Serialize(object obj) => SerializeValue(obj);
    private static string SerializeValue(object value) {
        if (value == null) return "null";
        if (value is string s) return $"\"{Escape(s)}\"";
        if (value is bool b) return b ? "true" : "false";
        if (value is IDictionary dict) return SerializeDict(dict);
        if (value is ICollection list) return SerializeList(list);
        if (value is ValueType && (value is int || value is long || value is float || value is double || value is decimal))
            return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
        return $"\"{Escape(value.ToString())}\"";
    }
    private static string SerializeDict(IDictionary dict) {
        var sb = new StringBuilder("{"); bool first = true;
        foreach (DictionaryEntry kv in dict) { if (!first) sb.Append(',');
            sb.Append('\"').Append(Escape(kv.Key.ToString())).Append("\":").Append(SerializeValue(kv.Value)); first = false; }
        return sb.Append('}').ToString();
    }
    private static string SerializeList(ICollection list) {
        var sb = new StringBuilder("["); bool first = true;
        foreach (var v in list) { if (!first) sb.Append(',');
            sb.Append(SerializeValue(v)); first = false; }
        return sb.Append(']').ToString();
    }
    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
}