using UnityEngine;

public enum FishFlavor
{
    None    = 0,
    Albino  = 1,
    Shiny   = 2,
    Ancient = 3,
    Giant   = 4,
    Golden  = 5,
    Cursed  = 6,
}

public static class FishFlavorData
{
    public struct FlavorInfo
    {
        public string displayName;
        public float  spawnChance;      // independent roll per fish spawn
        public int    difficultyDelta;  // added to base fish difficulty (clamped to 5)
        public float  scoreMultiplier;  // applied on top of perk-adjusted score
        public Color  tintColor;        // sprite renderer color override
        public float  scaleMultiplier;  // applied on top of rarity-based scale
    }

    static readonly FlavorInfo[] _infos = new FlavorInfo[]
    {
        // None
        new FlavorInfo { displayName = "",        spawnChance = 0f,      difficultyDelta = 0, scoreMultiplier = 1f,  tintColor = Color.white,                         scaleMultiplier = 1f   },
        // Albino      — ~1.2% chance, 2× score
        new FlavorInfo { displayName = "Albino",  spawnChance = 0.012f,  difficultyDelta = 0, scoreMultiplier = 2f,  tintColor = new Color(0.95f, 0.95f, 1f,   0.9f), scaleMultiplier = 1f   },
        // Shiny       — ~0.7% chance, 4× score
        new FlavorInfo { displayName = "Shiny",   spawnChance = 0.007f,  difficultyDelta = 1, scoreMultiplier = 4f,  tintColor = new Color(0.45f, 1f,   1f,   0.9f), scaleMultiplier = 1.1f },
        // Ancient     — ~0.4% chance, 7× score
        new FlavorInfo { displayName = "Ancient", spawnChance = 0.004f,  difficultyDelta = 1, scoreMultiplier = 7f,  tintColor = new Color(0.75f, 0.62f, 0.4f, 0.9f), scaleMultiplier = 1.1f },
        // Giant       — ~0.2% chance, 12× score
        new FlavorInfo { displayName = "Giant",   spawnChance = 0.002f,  difficultyDelta = 2, scoreMultiplier = 12f, tintColor = new Color(0.85f, 0.85f, 1f,   0.88f),scaleMultiplier = 1.9f },
        // Golden      — ~0.1% chance, 20× score
        new FlavorInfo { displayName = "Golden",  spawnChance = 0.001f,  difficultyDelta = 2, scoreMultiplier = 20f, tintColor = new Color(1f,   0.85f, 0.15f, 0.92f),scaleMultiplier = 1.2f },
        // Cursed      — ~0.05% chance, 35× score
        new FlavorInfo { displayName = "Cursed",  spawnChance = 0.0005f, difficultyDelta = 3, scoreMultiplier = 35f, tintColor = new Color(0.5f, 0f,   0.8f, 0.95f), scaleMultiplier = 1f   },
    };

    public static FlavorInfo Get(FishFlavor flavor) => _infos[(int)flavor];

    /// <summary>Rolls a flavor for a newly spawned fish. Rarer flavors checked first.</summary>
    public static FishFlavor Roll()
    {
        float r = Random.value;
        if (r < 0.0005f) return FishFlavor.Cursed;   // 0.05%
        if (r < 0.0015f) return FishFlavor.Golden;   // 0.10%
        if (r < 0.0035f) return FishFlavor.Giant;    // 0.20%
        if (r < 0.0075f) return FishFlavor.Ancient;  // 0.40%
        if (r < 0.0145f) return FishFlavor.Shiny;    // 0.70%
        if (r < 0.0265f) return FishFlavor.Albino;   // 1.20%
        return FishFlavor.None;                       // ~97.4%
    }

    /// <summary>Rolls a flavor with per-flavor spawn multipliers applied by perks.</summary>
    public static FishFlavor RollWithMultipliers(float albino, float shiny, float ancient, float giant, float golden, float cursed)
    {
        float r = Random.value;
        float t = 0f;
        t += 0.0005f * cursed;  if (r < t) return FishFlavor.Cursed;
        t += 0.0010f * golden;  if (r < t) return FishFlavor.Golden;
        t += 0.0020f * giant;   if (r < t) return FishFlavor.Giant;
        t += 0.0040f * ancient; if (r < t) return FishFlavor.Ancient;
        t += 0.0070f * shiny;   if (r < t) return FishFlavor.Shiny;
        t += 0.0120f * albino;  if (r < t) return FishFlavor.Albino;
        return FishFlavor.None;
    }

    // All non-None flavors in compendium tab order
    public static readonly FishFlavor[] AllFlavors =
    {
        FishFlavor.Albino, FishFlavor.Shiny, FishFlavor.Ancient,
        FishFlavor.Giant,  FishFlavor.Golden, FishFlavor.Cursed,
    };

    // UI color used for flavor labels (brighter than tintColor for readability)
    public static Color LabelColor(FishFlavor flavor)
    {
        switch (flavor)
        {
            case FishFlavor.Albino:  return new Color(0.85f, 0.9f,  1f);
            case FishFlavor.Shiny:   return new Color(0.3f,  1f,    1f);
            case FishFlavor.Ancient: return new Color(1f,    0.8f,  0.45f);
            case FishFlavor.Giant:   return new Color(0.75f, 0.75f, 1f);
            case FishFlavor.Golden:  return new Color(1f,    0.9f,  0.2f);
            case FishFlavor.Cursed:  return new Color(0.8f,  0.3f,  1f);
            default:                 return Color.white;
        }
    }
}
