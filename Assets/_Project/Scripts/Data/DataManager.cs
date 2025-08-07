
namespace LorePath.Stormsign
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    //using LorePath;
    //using DG.Tweening;

    /// <summary>
    /// Manages saving and loading storm data and general application data.
    /// As this app becomes less storm-focused and more generalized, it might split storm data out to a storm-focused script.
    /// </summary>    
    public class DataManager : MonoBehaviour
    {
        private StormDatabase _storms = new();
        private long _lastSaveTime;
        private long _sessionNumber;

#if UNITY_STANDALONE
        // desktop builds can save aggregate storm data to local file for later analysis.
        private string _path => Path.Combine(Application.streamingAssetsPath, $"stormdata_{_sessionNumber}.txt");
#endif

        private void Awake()
        {
            string version = PlayerPrefs.GetString("version", "0");
            if (Application.version != version) PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString("version", Application.version);

            _sessionNumber = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            EventBus.Subscribe<DatabaseRequestEvent>(OnDatabaseEvent);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DatabaseRequestEvent>(OnDatabaseEvent);
        }

        void OnDatabaseEvent(DatabaseRequestEvent evt)
        {
            switch (evt)
            {
                case DataSaveRequest saveRequest: SaveDatabaseToFile(); break;
                case DataChangeRequest changeRequest: AddOrRemoveStorm(changeRequest.Storm, changeRequest.Add); break;
                case DataRefreshRecentStormRequest refreshRequest: RefreshRecentStorm(); break;
            }            
        }

        /// <summary>
        /// Given a storm reference, this adds or removes it from the recorded storm history,
        /// updating the list as well as the most recent storm entry, and sending appropriate events.
        /// </summary>
        public void AddOrRemoveStorm(StormData storm, bool add)
        {         
            if (add && !_storms.Data.Contains(storm))
            {
                // we're adding a new storm, as the most recent.

                _storms.Data.Add(storm);                
                RecentStorm recentStorm;
                if (!Blackboard.Get(out recentStorm))
                {
                    recentStorm = new RecentStorm();
                    Blackboard.Set(recentStorm);
                }

                recentStorm.Data = storm;
                
                FullStormList allStorms;
                if (!Blackboard.Get(out allStorms))
                {
                    allStorms = new FullStormList();
                    Blackboard.Set(allStorms);
                }

                // we want to make sure the full storm list is available,
                // but also protect our own as the source of truth, so we create a copy.
                allStorms.Storms = new List<StormData> (this._storms.Data);

                // if we're adding a storm, it's inherently the most recent storm.
                EventBus.RaiseEvent(new RecentStormChangeEvent());
                SaveDatabaseToFile();
            }
            else if (!add && _storms.Data.Remove(storm))
            {
                // we're removing a storm that existed in our list.

                RecentStorm recentStorm;
                if (!Blackboard.Get(out recentStorm))
                {
                    recentStorm = new RecentStorm();
                    Blackboard.Set(recentStorm);
                }

                FullStormList allStorms;
                if (!Blackboard.Get(out allStorms))
                {
                    allStorms = new FullStormList();
                    Blackboard.Set(allStorms);
                }
                allStorms.Storms = new List<StormData>(this._storms.Data);

                // while as of 8/6 we never remove anything but the most recent storm,
                // we want to leave open the possibility that some rando storm can be removed  from the list.
                bool wasRecent = false;
                if (recentStorm.Data == storm)
                {
                    recentStorm.Data = null;
                    wasRecent = true;
                }                

                EventBus.RaiseEvent(wasRecent ? new RecentStormChangeEvent() : new DataChangeEvent());
                SaveDatabaseToFile();
            }        
        }


        /// <summary>
        /// Ensures the most recent storm in the full list is up to date.
        /// if a change is made, it raises a change event.
        /// </summary>
        public void RefreshRecentStorm()
        {
            bool changed = false;
            RecentStorm recentStorm;
            if (!Blackboard.Get(out recentStorm))
            {
                recentStorm = new RecentStorm();
                Blackboard.Set(recentStorm);
                changed = true;
            }

            StormData latestStormData = _storms.Data.Count > 0 ? _storms.Data[^1] : null;
            if (latestStormData != recentStorm.Data)
            {
                // if the latest addition to all storms is not considered the recent storm,
                // we make it so and send the change event.
                recentStorm.Data = latestStormData;
                changed = true;
            }

            if (changed)
            {
                EventBus.RaiseEvent(new RecentStormChangeEvent());
            }
        }
         


        public void SaveDatabaseToFile()
        {
#if UNITY_STANDALONE
            if ((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastSaveTime) > 500)
            {
                // we only allow saving if it's been at least half a second.

                _lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                try
                {
                    string data = JsonUtility.ToJson(_storms);
                    string directory = Path.GetDirectoryName(_path);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        File.Create(_path);
                    }
                    else if (!File.Exists(_path)) File.Create(_path).Dispose();

                    File.WriteAllText(_path, data);
                    EventBus.RaiseEvent(new DataSaveEvent(true));
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to write storm data: " + e.ToString());
                    EventBus.RaiseEvent(new DataSaveEvent(false));
                }
            }
#endif
        }

        [System.Serializable]
        public class StormDatabase
        {
            public List<StormData> Data = new();
         
        }
    }

}


// q // cD // d
// Unity 6000.0.41f1
