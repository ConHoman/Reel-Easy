using System.Collections.Generic;
using UnityEngine;

public enum PerkType
{
    ExtraLine, DenseWaters, LuckyLine,
    OverfilledReel, GlassRod, TrophyHunter,
    SpeedDemon, SilkThread, GamblersHook,
    ChumBucket, RustyLure
}

[System.Serializable]
public class PerkDefinition
{
    public PerkType type;
    public string displayName;
    public string upside;
    public string downside;

    public PerkDefinition(PerkType t, string name, string up, string down)
    { type = t; displayName = name; upside = up; downside = down; }
}

public class PerkManager : MonoBehaviour
{
    public static PerkManager Instance;

    private readonly List<PerkType> active = new List<PerkType>();
    private bool luckyLineAvailable;

    public static readonly PerkDefinition[] AllPerks =
    {
        new PerkDefinition(PerkType.ExtraLine,      "+1 Line",         "+1 line immediately",               ""),
        new PerkDefinition(PerkType.DenseWaters,    "Dense Waters",    "+2 max fish per cast",              ""),
        new PerkDefinition(PerkType.LuckyLine,      "Lucky Line",      "One snap per run is free",          "Single use"),
        new PerkDefinition(PerkType.OverfilledReel, "Overfilled Reel", "+2 max fish per cast",              "+1 bubble per fish hooked"),
        new PerkDefinition(PerkType.GlassRod,       "Glass Rod",       "Steer phase lasts 2x longer",       "Snap costs 2 lines"),
        new PerkDefinition(PerkType.TrophyHunter,   "Trophy Hunter",   "Legendary fish score 3x",           "Common fish score 0"),
        new PerkDefinition(PerkType.SpeedDemon,     "Speed Demon",     "All fish score 2x",                 "Steer phase is half as long"),
        new PerkDefinition(PerkType.SilkThread,     "Silk Thread",     "+2 allowed misses in minigame",     "Start with 1 fewer line"),
        new PerkDefinition(PerkType.GamblersHook,   "Gambler's Hook",  "All catches score 3x",              "Every snap costs 2 lines"),
        new PerkDefinition(PerkType.ChumBucket,     "Chum Bucket",     "Spawn radius doubled",              "Minimum 4 bubbles always"),
        new PerkDefinition(PerkType.RustyLure,      "Rusty Lure",      "Bobber hitbox 2x larger",           "Steer speed halved"),
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public static void EnsureExists()
    {
        if (Instance == null)
            new GameObject("PerkManager").AddComponent<PerkManager>();
    }

    public void ResetForRun()
    {
        active.Clear();
        luckyLineAvailable = false;
    }

    public void AddPerk(PerkType perk)
    {
        active.Add(perk);
        if (perk == PerkType.LuckyLine) luckyLineAvailable = true;
    }

    public bool HasPerk(PerkType perk) => active.Contains(perk);
    public List<PerkType> ActivePerks => active;

    // ── Stat modifiers queried by other scripts ──────────────

    public float SteerDurationMultiplier
    {
        get
        {
            float m = 1f;
            if (HasPerk(PerkType.GlassRod))  m *= 2f;
            if (HasPerk(PerkType.SpeedDemon)) m *= 0.5f;
            return m;
        }
    }

    public float TipSpeedMultiplier    => HasPerk(PerkType.RustyLure) ? 0.5f : 1f;
    public float HitboxMultiplier      => HasPerk(PerkType.RustyLure) ? 2f : 1f;
    public int   SpawnCountBonus       => (HasPerk(PerkType.DenseWaters) ? 2 : 0) + (HasPerk(PerkType.OverfilledReel) ? 2 : 0);
    public float SpawnRadiusMultiplier => HasPerk(PerkType.ChumBucket) ? 2f : 1f;
    public int   ExtraBubblesPerFish   => HasPerk(PerkType.OverfilledReel) ? 1 : 0;
    public int   MinBubbles            => HasPerk(PerkType.ChumBucket) ? 4 : 0;
    public int   AllowedMissesBonus    => HasPerk(PerkType.SilkThread) ? 2 : 0;
    public int   LinesLostOnSnap       => (HasPerk(PerkType.GlassRod) || HasPerk(PerkType.GamblersHook)) ? 2 : 1;

    public int GetScoreForFish(FishData fish)
    {
        if (HasPerk(PerkType.TrophyHunter) && fish.rarity == 1) return 0;
        int score = fish.scoreValue;
        if (HasPerk(PerkType.TrophyHunter) && fish.rarity == 3) score *= 3;
        if (HasPerk(PerkType.SpeedDemon))   score *= 2;
        if (HasPerk(PerkType.GamblersHook)) score *= 3;
        return score;
    }

    public bool TryConsumeLuckyLine()
    {
        if (!luckyLineAvailable) return false;
        luckyLineAvailable = false;
        return true;
    }
}
