using System.Collections.Generic;
using UnityEngine;

public enum PerkType
{
    ExtraLine, DenseWaters, LuckyLine,
    OverfilledReel, GlassRod, TrophyHunter,
    SpeedDemon, SilkThread, GamblersHook,
    ChumBucket, RustyLure,
    SteadyHands, IronGrip, TurboReel, QuickFingers
}

public enum PerkCategory { Safety, Fishing, Scoring, Minigame }

[System.Serializable]
public class PerkDefinition
{
    public PerkType type;
    public PerkCategory category;
    public string displayName;
    public string upside;
    public string downside;

    public PerkDefinition(PerkType t, PerkCategory cat, string name, string up, string down)
    { type = t; category = cat; displayName = name; upside = up; downside = down; }
}

public class PerkManager : MonoBehaviour
{
    public static PerkManager Instance;

    private readonly List<PerkType> active = new List<PerkType>();
    private bool luckyLineAvailable;

    public static readonly PerkDefinition[] AllPerks =
    {
        new PerkDefinition(PerkType.ExtraLine,      PerkCategory.Safety,   "+1 Line",         "+1 line immediately",               ""),
        new PerkDefinition(PerkType.LuckyLine,      PerkCategory.Safety,   "Lucky Line",      "One snap per run is free",          "Single use"),
        new PerkDefinition(PerkType.DenseWaters,    PerkCategory.Fishing,  "Dense Waters",    "+2 max fish per cast",              ""),
        new PerkDefinition(PerkType.OverfilledReel, PerkCategory.Fishing,  "Overfilled Reel", "+2 max fish per cast",              "+1 bubble per fish hooked"),
        new PerkDefinition(PerkType.GlassRod,       PerkCategory.Fishing,  "Glass Rod",       "Steer phase lasts 2x longer",       "Snap costs 2 lines"),
        new PerkDefinition(PerkType.ChumBucket,     PerkCategory.Fishing,  "Chum Bucket",     "Spawn radius doubled",              "Min 4 bubbles always"),
        new PerkDefinition(PerkType.RustyLure,      PerkCategory.Fishing,  "Rusty Lure",      "Bobber hitbox 2x larger",           "Steer speed halved"),
        new PerkDefinition(PerkType.TrophyHunter,   PerkCategory.Scoring,  "Trophy Hunter",   "Legendary fish score 3x",           "Common fish score 0"),
        new PerkDefinition(PerkType.SpeedDemon,     PerkCategory.Scoring,  "Speed Demon",     "All fish score 2x",                 "Steer phase is half as long"),
        new PerkDefinition(PerkType.GamblersHook,   PerkCategory.Scoring,  "Gambler's Hook",  "All catches score 3x",              "Every snap costs 2 lines"),
        new PerkDefinition(PerkType.SilkThread,     PerkCategory.Minigame, "Silk Thread",     "[Bubble Pop] +2 allowed misses",        "Start with 1 fewer line"),
        new PerkDefinition(PerkType.SteadyHands,    PerkCategory.Minigame, "Steady Hands",    "[Timing Bar] Zone 40% wider",           "[Timing Bar] Cursor 30% faster"),
        new PerkDefinition(PerkType.IronGrip,       PerkCategory.Minigame, "Iron Grip",       "[Hold Zone] Progress never drains",     "[Hold Zone] Need 50% more hold time"),
        new PerkDefinition(PerkType.TurboReel,      PerkCategory.Minigame, "Turbo Reel",      "[Tug of War] Your pull is 2x stronger", "[Tug of War] Fish pulls 1.5x harder"),
        new PerkDefinition(PerkType.QuickFingers,   PerkCategory.Minigame, "Quick Fingers",   "[Button Mash] Each press counts double","[Button Mash] 40% less time"),
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

    // Minigame-specific modifiers
    public float TimingZoneMultiplier    => HasPerk(PerkType.SteadyHands) ? 1.4f : 1f;
    public float TimingSpeedMultiplier   => HasPerk(PerkType.SteadyHands) ? 1.3f : 1f;
    public bool  HoldZoneNoDrain         => HasPerk(PerkType.IronGrip);
    public float HoldTimeMultiplier      => HasPerk(PerkType.IronGrip) ? 1.5f : 1f;
    public float TugPlayerMultiplier     => HasPerk(PerkType.TurboReel) ? 2f : 1f;
    public float TugFishMultiplier       => HasPerk(PerkType.TurboReel) ? 1.5f : 1f;
    public int   MashClickMultiplier     => HasPerk(PerkType.QuickFingers) ? 2 : 1;
    public float MashTimeLimitMultiplier => HasPerk(PerkType.QuickFingers) ? 0.6f : 1f;

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
