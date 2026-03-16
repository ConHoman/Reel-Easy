using System.Collections.Generic;
using UnityEngine;

public enum PerkType
{
    // Existing perks
    ExtraLine, DenseWaters, LuckyLine,
    OverfilledReel, GlassRod, TrophyHunter,
    SpeedDemon, SilkThread, GamblersHook,
    ChumBucket, RustyLure,
    SteadyHands, IronGrip, TurboReel, QuickFingers,
    SwiftCurrent, StealthyHook, PatientAngler,
    WideGap, SlipStream, CalmWaters,

    // Per-rarity perks
    CommonMagnet, UncommonEye, EpicSurge, LegendaryLore, MythicBlessing,

    // Per-variant perks
    AlbinoAffinity, ShinySense, AncientReverence, TitansSight, GoldRush, CursedPact,

    // General perks
    LastCast, Jackpot, FishWhisperer,
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
        // ── Safety ───────────────────────────────────────────────────────────
        new PerkDefinition(PerkType.ExtraLine,      PerkCategory.Safety,   "+1 Line",           "+1 line immediately",                                    ""),
        new PerkDefinition(PerkType.LuckyLine,      PerkCategory.Safety,   "Lucky Line",        "One snap per run is free",                               "Single use"),
        new PerkDefinition(PerkType.LastCast,       PerkCategory.Safety,   "Last Cast",         "At 1 line remaining, all fish score x5",                 ""),

        // ── Fishing ───────────────────────────────────────────────────────────
        new PerkDefinition(PerkType.DenseWaters,    PerkCategory.Fishing,  "Dense Waters",      "+2 max fish per cast",                                   ""),
        new PerkDefinition(PerkType.OverfilledReel, PerkCategory.Fishing,  "Overfilled Reel",   "+2 max fish per cast",                                   "+1 bubble per fish hooked"),
        new PerkDefinition(PerkType.GlassRod,       PerkCategory.Fishing,  "Glass Rod",         "Steer phase lasts 2x longer",                            "Snap costs 2 lines"),
        new PerkDefinition(PerkType.ChumBucket,     PerkCategory.Fishing,  "Chum Bucket",       "Spawn radius doubled",                                   "Min 4 bubbles always"),
        new PerkDefinition(PerkType.RustyLure,      PerkCategory.Fishing,  "Rusty Lure",        "Bobber hitbox 2x larger",                                "Steer speed halved"),
        new PerkDefinition(PerkType.SwiftCurrent,   PerkCategory.Fishing,  "Swift Current",     "Bobber moves 50% faster",                                "Steer phase 25% shorter"),
        new PerkDefinition(PerkType.StealthyHook,   PerkCategory.Fishing,  "Stealthy Hook",     "Fish detect bobber 35% later",                           "Bobber hitbox 30% smaller"),
        new PerkDefinition(PerkType.PatientAngler,  PerkCategory.Fishing,  "Patient Angler",    "Fish flee 40% slower",                                   "Steer phase 20% shorter"),
        new PerkDefinition(PerkType.FishWhisperer,  PerkCategory.Fishing,  "Fish Whisperer",    "All fish flee 20% slower and detect 25% later",          ""),
        new PerkDefinition(PerkType.CommonMagnet,   PerkCategory.Fishing,  "Common Magnet",     "Common fish spawn 2x more often",                        ""),
        new PerkDefinition(PerkType.UncommonEye,    PerkCategory.Fishing,  "Uncommon Eye",      "Uncommon spawn 3x more, flee 20% slower",                "Common fish score 0"),
        new PerkDefinition(PerkType.EpicSurge,      PerkCategory.Fishing,  "Epic Surge",        "Epic fish spawn 3x more often, score x2",                "Steer phase 15% shorter"),
        new PerkDefinition(PerkType.LegendaryLore,  PerkCategory.Fishing,  "Legendary Lore",    "Legendary spawn 3x more often, score x2",                "Steer phase 20% shorter"),
        new PerkDefinition(PerkType.AlbinoAffinity, PerkCategory.Fishing,  "Albino Affinity",   "Albino spawn x5, Albino fish flee 25% slower",           ""),
        new PerkDefinition(PerkType.ShinySense,     PerkCategory.Fishing,  "Shiny Sense",       "Shiny spawn x5, Shiny fish flee 30% slower",             ""),
        new PerkDefinition(PerkType.TitansSight,    PerkCategory.Fishing,  "Titan's Sight",     "Giant spawn x5",                                         ""),

        // ── Scoring ───────────────────────────────────────────────────────────
        new PerkDefinition(PerkType.TrophyHunter,   PerkCategory.Scoring,  "Trophy Hunter",     "Legendary fish score 3x",                                "Common fish score 0"),
        new PerkDefinition(PerkType.SpeedDemon,     PerkCategory.Scoring,  "Speed Demon",       "All fish score 2x",                                      "Steer phase is half as long"),
        new PerkDefinition(PerkType.GamblersHook,   PerkCategory.Scoring,  "Gambler's Hook",    "All catches score 3x",                                   "Every snap costs 2 lines"),
        new PerkDefinition(PerkType.Jackpot,        PerkCategory.Scoring,  "Jackpot",           "Every 10th catch scores x10",                            ""),
        new PerkDefinition(PerkType.MythicBlessing, PerkCategory.Scoring,  "Mythic Blessing",   "Mythical spawn x3, Mythical fish score x5",              "Every non-Mythical catch costs 1 line"),
        new PerkDefinition(PerkType.AncientReverence,PerkCategory.Scoring, "Ancient Reverence", "Ancient spawn x5, Ancient variant score x2",             "Steer phase 15% shorter"),
        new PerkDefinition(PerkType.GoldRush,       PerkCategory.Scoring,  "Gold Rush",         "Golden spawn x5, Golden variant score x2",               "All other fish score halved"),
        new PerkDefinition(PerkType.CursedPact,     PerkCategory.Scoring,  "Cursed Pact",       "Cursed spawn x5, Cursed variant score x2",               "Every snap costs 2 extra lines"),

        // ── Minigame ──────────────────────────────────────────────────────────
        new PerkDefinition(PerkType.SilkThread,     PerkCategory.Minigame, "Silk Thread",       "[Bubble Pop] +2 allowed misses",                         "Start with 1 fewer line"),
        new PerkDefinition(PerkType.SteadyHands,    PerkCategory.Minigame, "Steady Hands",      "[Timing Bar] Zone 40% wider",                            "[Timing Bar] Cursor 30% faster"),
        new PerkDefinition(PerkType.IronGrip,       PerkCategory.Minigame, "Iron Grip",         "[Hold Zone] Progress never drains",                      "[Hold Zone] Need 50% more hold time"),
        new PerkDefinition(PerkType.TurboReel,      PerkCategory.Minigame, "Turbo Reel",        "[Tug of War] Your pull is 2x stronger",                  "[Tug of War] Fish pulls 1.5x harder"),
        new PerkDefinition(PerkType.QuickFingers,   PerkCategory.Minigame, "Quick Fingers",     "[Button Mash] Each press counts double",                 "[Button Mash] 40% less time"),
        new PerkDefinition(PerkType.WideGap,        PerkCategory.Minigame, "Wide Gap",          "[Ring Dodge] Gap 45% wider",                             "[Ring Dodge] Rings shrink 25% faster"),
        new PerkDefinition(PerkType.SlipStream,     PerkCategory.Minigame, "Slip Stream",       "[Ring Dodge] Rotate 60% faster",                         "[Ring Dodge] Gap 20% narrower"),
        new PerkDefinition(PerkType.CalmWaters,     PerkCategory.Minigame, "Calm Waters",       "[Ring Dodge] Rings 35% slower",                          "One extra ring to clear"),
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

    public int Count(PerkType t) { int n = 0; foreach (var p in active) if (p == t) n++; return n; }

    // ── Steer / movement ─────────────────────────────────────────────────────

    public float SteerDurationMultiplier =>
        Mathf.Pow(2.00f, Count(PerkType.GlassRod))          *
        Mathf.Pow(0.50f, Count(PerkType.SpeedDemon))         *
        Mathf.Pow(0.75f, Count(PerkType.SwiftCurrent))       *
        Mathf.Pow(0.80f, Count(PerkType.PatientAngler))      *
        Mathf.Pow(0.85f, Count(PerkType.EpicSurge))          *
        Mathf.Pow(0.80f, Count(PerkType.LegendaryLore))      *
        Mathf.Pow(0.85f, Count(PerkType.AncientReverence));

    public float TipSpeedMultiplier =>
        Mathf.Pow(0.50f, Count(PerkType.RustyLure))    *
        Mathf.Pow(1.50f, Count(PerkType.SwiftCurrent));

    public float HitboxMultiplier =>
        Mathf.Pow(2.00f, Count(PerkType.RustyLure))    *
        Mathf.Pow(0.70f, Count(PerkType.StealthyHook));

    // ── Spawn ─────────────────────────────────────────────────────────────────

    public int   SpawnCountBonus       => Count(PerkType.DenseWaters) * 2 + Count(PerkType.OverfilledReel) * 2;
    public float SpawnRadiusMultiplier => Mathf.Pow(2f, Count(PerkType.ChumBucket));

    /// <summary>Extra weight added to the rarity pool for each rarity tier.</summary>
    public int RaritySpawnBonus(int rarity)
    {
        switch (rarity)
        {
            case 1: return Count(PerkType.CommonMagnet)   * 12;  // doubles per stack (base 12)
            case 2: return Count(PerkType.UncommonEye)    * 6;   // triples per stack (base 3)
            case 3: return Count(PerkType.EpicSurge)      * 4;   // triples per stack (base 2)
            case 4: return Count(PerkType.LegendaryLore)  * 2;   // triples per stack (base 1)
            default: return 0;
        }
    }

    /// <summary>Multiplier applied to the base 4% mythical gate probability.</summary>
    public float MythicalGateBonus => Mathf.Pow(3f, Count(PerkType.MythicBlessing));

    /// <summary>Per-flavor spawn multiplier driven by variant-specific perks.</summary>
    public float FlavorSpawnMultiplier(FishFlavor f)
    {
        switch (f)
        {
            case FishFlavor.Albino:  return Mathf.Pow(5f, Count(PerkType.AlbinoAffinity));
            case FishFlavor.Shiny:   return Mathf.Pow(5f, Count(PerkType.ShinySense));
            case FishFlavor.Ancient: return Mathf.Pow(5f, Count(PerkType.AncientReverence));
            case FishFlavor.Giant:   return Mathf.Pow(5f, Count(PerkType.TitansSight));
            case FishFlavor.Golden:  return Mathf.Pow(5f, Count(PerkType.GoldRush));
            case FishFlavor.Cursed:  return Mathf.Pow(5f, Count(PerkType.CursedPact));
            default: return 1f;
        }
    }

    // ── Fish flee / detection ─────────────────────────────────────────────────

    /// <summary>Global flee speed modifier (applied to all fish).</summary>
    public float FishFleeSpeedMultiplier =>
        Mathf.Pow(0.60f, Count(PerkType.PatientAngler)) *
        Mathf.Pow(0.80f, Count(PerkType.FishWhisperer));

    /// <summary>Global detection radius modifier (applied to all fish).</summary>
    public float FishDetectionMultiplier =>
        Mathf.Pow(0.65f, Count(PerkType.StealthyHook)) *
        Mathf.Pow(0.75f, Count(PerkType.FishWhisperer));

    /// <summary>Additional flee multiplier based on a fish's rarity.</summary>
    public float RarityFleeMultiplier(int rarity)
    {
        float m = 1f;
        if (rarity == 2) m *= Mathf.Pow(0.80f, Count(PerkType.UncommonEye));
        return m;
    }

    /// <summary>Additional flee multiplier based on a fish's flavor.</summary>
    public float FlavorFleeMultiplier(FishFlavor f)
    {
        switch (f)
        {
            case FishFlavor.Albino: return Mathf.Pow(0.75f, Count(PerkType.AlbinoAffinity));
            case FishFlavor.Shiny:  return Mathf.Pow(0.70f, Count(PerkType.ShinySense));
            default: return 1f;
        }
    }

    // ── Minigame misc ─────────────────────────────────────────────────────────

    public int   ExtraBubblesPerFish   => Count(PerkType.OverfilledReel);
    public int   MinBubbles            => HasPerk(PerkType.ChumBucket) ? 4 : 0;
    public int   AllowedMissesBonus    => Count(PerkType.SilkThread) * 2;

    public int LinesLostOnSnap
    {
        get
        {
            int loss = 1;
            if (HasPerk(PerkType.GlassRod) || HasPerk(PerkType.GamblersHook)) loss++;
            loss += Count(PerkType.CursedPact) * 2;
            return loss;
        }
    }

    public float TimingZoneMultiplier    => Mathf.Pow(1.40f, Count(PerkType.SteadyHands));
    public float TimingSpeedMultiplier   => Mathf.Pow(1.30f, Count(PerkType.SteadyHands));
    public bool  HoldZoneNoDrain         => HasPerk(PerkType.IronGrip);
    public float HoldTimeMultiplier      => Mathf.Pow(1.50f, Count(PerkType.IronGrip));
    public float TugPlayerMultiplier     => Mathf.Pow(2.00f, Count(PerkType.TurboReel));
    public float TugFishMultiplier       => Mathf.Pow(1.50f, Count(PerkType.TurboReel));
    public int   MashClickMultiplier     { get { int m = 1; for (int i = 0; i < Count(PerkType.QuickFingers); i++) m *= 2; return m; } }
    public float MashTimeLimitMultiplier => Mathf.Pow(0.60f, Count(PerkType.QuickFingers));

    public float RingDodgeGapMultiplier      => Mathf.Pow(1.45f, Count(PerkType.WideGap))    * Mathf.Pow(0.80f, Count(PerkType.SlipStream));
    public float RingDodgeShrinkDurationMult => Mathf.Pow(1.35f, Count(PerkType.CalmWaters)) * Mathf.Pow(0.80f, Count(PerkType.WideGap));
    public float RingDodgeRotSpeedMultiplier => Mathf.Pow(1.60f, Count(PerkType.SlipStream));
    public int   RingDodgeExtraRings         => Count(PerkType.CalmWaters);

    // ── Scoring ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Full score calculation including perk modifiers, flavor multipliers, Last Cast, and Jackpot.
    /// linesLeft: current lines remaining. catchCount: total fish caught so far (after incrementing).
    /// </summary>
    public int GetScoreForCaught(CaughtFish cf, int linesLeft, int catchCount)
    {
        FishData   fish   = cf.data;
        FishFlavor flavor = cf.flavor;

        // Score-zero conditions
        if (HasPerk(PerkType.TrophyHunter) && fish.rarity == 1) return 0;
        if (HasPerk(PerkType.UncommonEye)  && fish.rarity == 1) return 0;

        int score = fish.scoreValue;

        // Rarity-specific score multipliers
        if (fish.rarity == 3) for (int i = 0; i < Count(PerkType.EpicSurge);      i++) score *= 2;
        if (fish.rarity == 4) for (int i = 0; i < Count(PerkType.LegendaryLore);  i++) score *= 2;
        if (fish.rarity == 5) for (int i = 0; i < Count(PerkType.MythicBlessing); i++) score *= 5;

        // TrophyHunter bonuses (Legendary=4, Mythical=5)
        if (HasPerk(PerkType.TrophyHunter) && fish.rarity == 5) score = Mathf.RoundToInt(score * 10f);
        else if (HasPerk(PerkType.TrophyHunter) && fish.rarity == 4) score = Mathf.RoundToInt(score * 3f);

        // Global multipliers
        for (int i = 0; i < Count(PerkType.SpeedDemon);   i++) score *= 2;
        for (int i = 0; i < Count(PerkType.GamblersHook); i++) score *= 3;

        // Flavor multiplier (base + variant-specific bonus)
        float flavorMult = FishFlavorData.Get(flavor).scoreMultiplier;
        for (int i = 0; i < Count(PerkType.AncientReverence); i++) if (flavor == FishFlavor.Ancient) flavorMult *= 2f;
        for (int i = 0; i < Count(PerkType.GoldRush);         i++) if (flavor == FishFlavor.Golden)  flavorMult *= 2f;
        for (int i = 0; i < Count(PerkType.CursedPact);       i++) if (flavor == FishFlavor.Cursed)  flavorMult *= 2f;

        // Gold Rush halves non-golden fish
        if (HasPerk(PerkType.GoldRush) && flavor != FishFlavor.Golden) flavorMult *= 0.5f;

        score = Mathf.RoundToInt(score * flavorMult);

        // Last Cast: x5 when on the final line
        if (HasPerk(PerkType.LastCast) && linesLeft == 1) score *= 5;

        // Jackpot: x10 on every 10th catch
        if (HasPerk(PerkType.Jackpot) && catchCount % 10 == 0) score *= 10;

        return score;
    }

    /// <summary>True when Mythic Blessing should deduct a line for catching a non-Mythical fish.</summary>
    public bool MythicBlessingPenalty => HasPerk(PerkType.MythicBlessing);

    public bool TryConsumeLuckyLine()
    {
        if (!luckyLineAvailable) return false;
        luckyLineAvailable = false;
        return true;
    }
}
