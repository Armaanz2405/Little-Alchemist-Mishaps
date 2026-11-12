using System.Collections.Generic;
using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    // One piece of chopping music and how it lines up with the game.
    [System.Serializable]
    public class BeatTrack
    {
        [Tooltip("Plays when the ingredient's name contains this. Leave empty to let it stand in for anything.")]
        public string ingredient;
        public AudioClip clip;
        public float bpm = 100f;
        [Tooltip("Seconds into the clip where the first count-in beat lands.")]
        public float offset;
    }

    public static class BeatTracks
    {
        // An ingredient's own track when it has one, otherwise the same stand-in every time. -1 when there's nothing to play.
        public static int Pick(IReadOnlyList<BeatTrack> tracks, string ingredientName)
        {
            if (tracks == null || tracks.Count == 0) return -1;
            string name = (ingredientName ?? "").ToLowerInvariant();
            for (int i = 0; i < tracks.Count; i++)
            {
                string key = tracks[i] != null ? tracks[i].ingredient : null;
                if (!string.IsNullOrEmpty(key) && name.Contains(key.ToLowerInvariant())) return i;
            }
            uint hash = 2166136261;
            foreach (char c in name) hash = (hash ^ c) * 16777619;
            return (int)(hash % (uint)tracks.Count);
        }
    }
}
