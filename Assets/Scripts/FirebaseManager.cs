using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    private DatabaseReference db;

    public static bool IsReady { get; private set; } = false;

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Task Failed: " + task.Exception);
                return;
            }

            if (task.Result == DependencyStatus.Available)
            {
                var app = FirebaseApp.DefaultInstance;
                app.Options.DatabaseUrl = new System.Uri("https://the-last-feather-default-rtdb.europe-west1.firebasedatabase.app/");

                db = FirebaseDatabase.GetInstance(app).RootReference;
                IsReady = true;
            }
            else
            {
                Debug.LogError("Dependency Firebase Error: " + task.Result);
            }
        });
    }

    public void SaveScore(string username, long score)
    {
        if (db == null)
        {
            Debug.LogError("Firebase not connected");
            return;
        }

        string key = db.Child("scores").Push().Key;
        PlayerScore newScore = new(username, score);
        string json = JsonUtility.ToJson(newScore);

        db.Child("scores").Child(key).SetRawJsonValueAsync(json);
    }

    public void GetScores(System.Action<List<PlayerScore>> callback)
    {
        db.Child("scores").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error getting scores!");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                List<PlayerScore> scores = new List<PlayerScore>();

                foreach (DataSnapshot child in snapshot.Children)
                {
                    PlayerScore score = JsonUtility.FromJson<PlayerScore>(child.GetRawJsonValue());
                    scores.Add(score);
                }

                callback?.Invoke(scores);
            }
        });
    }
}
