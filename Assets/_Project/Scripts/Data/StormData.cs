namespace LorePath.Stormsign
{
    using UnityEngine;
    using System.Collections.Generic;

    [System.Serializable]
    public class StormData
    {        
        // normalized coordinate values relative to a 2D square of the hagga basin map.
        // note that the accuracy of these coordinates will vary based on the map image used.
        public Vector2 StartCoord;
        public Vector2 EndCoord;

        /// <summary>
        /// The time this storm appeared, in UNIX UTC SECONDS
        /// </summary>
        public long SpawnTime;

        /// <summary>
        /// The time we started waiting for this storm in unix UTC seconds,
        /// including all manual offsets and pause offsets.
        /// </summary>
        public long TimerStart; 

        /// <summary>
        /// The true time we started waiting for this storm, equal to the moment we sighted a previous one
        /// with the 'storm sighted' button. This is combined with TotalOffset to get OffsetStart.
        /// </summary>
        public long TimerOffset;

        /// <summary>
        /// How long since the previous storm did we wait for this one to arrive?
        /// this is the spawn time minus the offset timer start.
        /// </summary>
        public long Delay => SpawnTime - (TimerStart + TimerOffset);


        /// <summary>
        /// How much weight to give the data in aggregate analysis. Higher quality (5) is considered precise
        /// and accurate. Lower quality (1) is for storms seen late, or with limited directional accuracy, 
        /// or otherwise guestimated. Quality 0 or less is ignored entirely but still saved to file.
        /// </summary>
        public int DataQuality = 5;

        /// <summary>
        /// The application version used to collect this storm data
        /// </summary>
        public string Version;

    }


    


    public class RecentStorm : IDataObject
    {
        public StormData Data;
    }


    public class FullStormList : IDataObject
    {
        public List<StormData> Storms;
    }
}