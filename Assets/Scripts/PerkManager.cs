using System.Collections.Generic;
using UnityEngine;

public enum PerkType
{
    ExtraLine, DenseWaters, LuckyLine,
    OverfilledReel, GlassRod, TrophyHunter,
    SpeedDemon, SilkThread, GamblersHook,
    ChumBucket, RustyLure,
    SteadyHands, IronGrip, TurboReel, QuickFingers,
    SwiftCurrent, StealthyHook, PatientAngler,
    WideGap, SlipStream, CalmWaters
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
        new PerkDefinition(PerkType.SwiftCurrent,   PerkCategory.Fishing,  "Swift Current",   "Bobber moves 50% faster",               "Steer phase 25% shorter"),
        new PerkDefinition(PerkType.StealthyHook,   PerkCategory.Fishing,  "Stealthy Hook",   "Fish detect bobber 35% later",          "Bobber hitbox 30% smaller"),
        new PerkDefinition(PerkType.PatientAngler,  PerkCategory.Fishing,  "Patient Angler",  "Fish flee 40% slower",                  "Steer phase 20% shorter"),
        new PerkDefinition(PerkType.WideGap,        PerkCategory.Minigame, "Wide Gap",        "[Ring Dodge] Gap 45% wider",            "[Ring Dodge] Rings shrink 25% faster"),
        new PerkDefinition(PerkType.SlipStream,     PerkCategory.Minigame, "Slip Stream",     "[Ring Dodge] Rotate 60% faster",        "[Ring Dodge] Gap 20% narrower"),
        new PerkDefinition(PerkType.CalmWaters,     PerkCategory.Minigame, "Calm Waters",     "[Ring Dodge] Rings 35% slower",         "One extra ring to clear"),
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
    public void RemovePerk(PerkType perk) => active.Remove(perk);
    public List<PerkType> ActivePerks => active;

    // Count stacks of a perk — used so picking the same perk twice actually helps.
    public int Count(PerkType t) { int n = 0; foreach (var p in active) if (p == t) n++; return n; }

    // ── Stat modifiers — all stack multiplicatively ───────────

    public float SteerDurationMultiplier =>
        Mathf.Pow(2.00f, Count(PerkType.GlassRod))     *
        Mathf.Pow(0.50f, Count(PerkType.SpeedDemon))   *
        Mathf.Pow(0.75f, Count(PerkType.SwiftCurrent)) *
        Mathf.Pow(0.80f, Count(PerkType.PatientAngler));

    public float TipSpeedMultiplier =>
        Mathf.Pow(0.50f, Count(PerkType.RustyLure))    *
        Mathf.Pow(1.50f, Count(PerkType.SwiftCurrent));

    public float HitboxMultiplier =>
        Mathf.Pow(2.00f, Count(PerkType.RustyLure))    *
        Mathf.Pow(0.70f, Count(PerkType.StealthyHook));

    public int   SpawnCountBonus       => Count(PerkType.DenseWaters) * 2 + Count(PerkType.OverfilledReel) * 2;
    public float SpawnRadiusMultiplier => Mathf.Pow(2f, Count(PerkType.ChumBucket));
    public int   ExtraBubblesPerFish   => Count(PerkType.OverfilledReel);
    public int   MinBubbles            => HasPerk(PerkType.ChumBucket) ? 4 : 0;  // caps at 4
    public int   AllowedMissesBonus    => Count(PerkType.SilkThread) * 2;
    public int   LinesLostOnSnap       => (HasPerk(PerkType.GlassRod) || HasPerk(PerkType.GamblersHook)) ? 2 : 1;

    // Minigame-specific modifiers
    public float TimingZoneMultiplier    => Mathf.Pow(1.40f, Count(PerkType.SteadyHands));
    public float TimingSpeedMultiplier   => Mathf.Pow(1.30f, Count(PerkType.SteadyHands));
    public bool  HoldZoneNoDrain         => HasPerk(PerkType.IronGrip);
    public float HoldTimeMultiplier      => Mathf.Pow(1.50f, Count(PerkType.IronGrip));
    public float TugPlayerMultiplier     => Mathf.Pow(2.00f, Count(PerkType.TurboReel));
    public float TugFishMultiplier       => Mathf.Pow(1.50f, Count(PerkType.TurboReel));
    public int   MashClickMultiplier     { get { int m = 1; for (int i = 0; i < Count(PerkType.QuickFingers); i++) m *= 2; return m; } }
    public float MashTimeLimitMultiplier => Mathf.Pow(0.60f, Count(PerkType.QuickFingers));
    public float FishDetectionMultiplier => Mathf.Pow(0.65f, Count(PerkType.StealthyHook));
    public float FishFleeSpeedMultiplier => Mathf.Pow(0.60f, Count(PerkType.PatientAngler));

    // Ring Dodge modifiers
    public float RingDodgeGapMultiplier      => Mathf.Pow(1.45f, Count(PerkType.WideGap))    * Mathf.Pow(0.80f, Count(PerkType.SlipStream));
    public float RingDodgeShrinkDurationMult => Mathf.Pow(1.35f, Count(PerkType.CalmWaters)) * Mathf.Pow(0.80f, Count(PerkType.WideGap));
    public float RingDodgeRotSpeedMultiplier => Mathf.Pow(1.60f, Count(PerkType.SlipStream));
    public int   RingDodgeExtraRings         => Count(PerkType.CalmWaters);

    public int GetScoreForFish(FishData fish)
    {
        if (HasPerk(PerkType.TrophyHunter) && fish.rarity == 1) return 0;
        int score = fish.scoreValue;
        if (HasPerk(PerkType.TrophyHunter) && fish.rarity == 4) score *= 10; // mythical — massive bonus
        else if (HasPerk(PerkType.TrophyHunter) && fish.rarity == 3) score *= 3; // TrophyHunter doesn't stack
        for (int i = 0; i < Count(PerkType.SpeedDemon);   i++) score *= 2;
        for (int i = 0; i < Count(PerkType.GamblersHook); i++) score *= 3;
        return score;
    }

    public bool TryConsumeLuckyLine()
    {
        if (!luckyLineAvailable) return false;
        luckyLineAvailable = false;
        return true;
    }
}
