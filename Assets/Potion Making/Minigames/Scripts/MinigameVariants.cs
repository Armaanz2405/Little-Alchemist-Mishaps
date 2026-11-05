using System;
using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    public enum CauldronVariant { Brew, StirRing }
    public enum ChopVariant { Rhythm, Knife }

    // Which version of each station minigame is live, for A/B playtesting. Remembered between runs.
    public static class MinigameVariants
    {
        private const string CauldronKey = "minigame.cauldron";
        private const string ChopKey = "minigame.chop";

        public static event Action Changed;

        public static CauldronVariant Cauldron
        {
            get => (CauldronVariant)PlayerPrefs.GetInt(CauldronKey, (int)CauldronVariant.Brew);
            set { PlayerPrefs.SetInt(CauldronKey, (int)value); Changed?.Invoke(); }
        }

        public static ChopVariant Chop
        {
            get => (ChopVariant)PlayerPrefs.GetInt(ChopKey, (int)ChopVariant.Rhythm);
            set { PlayerPrefs.SetInt(ChopKey, (int)value); Changed?.Invoke(); }
        }

        // flameflies caught at the window; the next brew burns them for a stronger fire
        public static int Flameflies { get; set; }

        public static string Describe(CauldronVariant v) => v == CauldronVariant.Brew ? "Brew (stir + fire)" : "Stir ring";
        public static string Describe(ChopVariant v) => v == ChopVariant.Rhythm ? "Chop to the beat" : "Knife swipe (mouse)";
    }
}
