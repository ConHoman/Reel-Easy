using UnityEngine;

// Defines every fish in the game. FishSpawner calls CreateAll() at Awake.
// Sprites are loaded from Resources/FishSprites/ — multiple fish share sprites as placeholders.
public static class FishDatabase
{
    public static FishData[] CreateAll()
    {
        return new FishData[]
        {
            // ── Common (rarity 1) ─────────────────────────────────────────────
            Make("Minnow",          difficulty: 1, rarity: 1, score:  5, sprite: "Clown Fish Icon"),
            Make("Roach",           difficulty: 1, rarity: 1, score:  7, sprite: "Redfin Fish Icon"),
            Make("Sunfish",         difficulty: 1, rarity: 1, score:  8, sprite: "Clown Fish Icon"),
            Make("Bluegill",        difficulty: 1, rarity: 1, score: 10, sprite: "Blue Clown Fish Icon"),
            Make("Gudgeon",         difficulty: 1, rarity: 1, score: 10, sprite: "Cod Fish Icon"),
            Make("Rudd",            difficulty: 1, rarity: 1, score: 11, sprite: "Redfin Fish Icon"),
            Make("Crappie",         difficulty: 1, rarity: 1, score: 12, sprite: "Flat Fish Icon"),
            Make("Perch",           difficulty: 1, rarity: 1, score: 14, sprite: "Redfin Fish Icon"),
            Make("Bream",           difficulty: 2, rarity: 1, score: 16, sprite: "Flat Fish Icon"),
            Make("Tench",           difficulty: 2, rarity: 1, score: 17, sprite: "Cod Fish Icon"),
            Make("Carp",            difficulty: 2, rarity: 1, score: 18, sprite: "Gold Fish Icon"),
            Make("Catfish",         difficulty: 2, rarity: 1, score: 20, sprite: "Magma Snake Icon"),
            Make("Bass",            difficulty: 2, rarity: 1, score: 22, sprite: "Bass Fish Icon"),
            Make("Trout",           difficulty: 2, rarity: 1, score: 25, sprite: "Samon fish Icon"),
            Make("Barbel",          difficulty: 2, rarity: 1, score: 26, sprite: "Magma Snake Icon"),
            Make("Chub",            difficulty: 2, rarity: 1, score: 20, sprite: "Cod Fish Icon"),

            // ── Uncommon (rarity 2) ───────────────────────────────────────────
            Make("Flounder",        difficulty: 2, rarity: 2, score: 42, sprite: "Flat Fish Icon"),
            Make("Channel Drum",    difficulty: 2, rarity: 2, score: 44, sprite: "Bass Fish Icon"),
            Make("Walleye",         difficulty: 2, rarity: 2, score: 46, sprite: "Bass Fish Icon"),
            Make("Bowfin",          difficulty: 3, rarity: 2, score: 50, sprite: "Magma Snake Icon"),
            Make("Sheepshead",      difficulty: 3, rarity: 2, score: 52, sprite: "Flat Fish Icon"),
            Make("Salmon",          difficulty: 3, rarity: 2, score: 55, sprite: "Samon fish Icon"),
            Make("Rainbow Trout",   difficulty: 3, rarity: 2, score: 58, sprite: "Acid Fish Icon"),
            Make("Brown Trout",     difficulty: 3, rarity: 2, score: 58, sprite: "Samon fish Icon"),
            Make("Chain Pickerel",  difficulty: 3, rarity: 2, score: 60, sprite: "Sea snake Icon"),
            Make("Northern Pike",   difficulty: 3, rarity: 2, score: 62, sprite: "Sea snake Icon"),
            Make("Redfish",         difficulty: 3, rarity: 2, score: 62, sprite: "Redfin Fish Icon"),
            Make("Lake Trout",      difficulty: 3, rarity: 2, score: 64, sprite: "Samon fish Icon"),
            Make("Steelhead",       difficulty: 3, rarity: 2, score: 65, sprite: "Samon fish Icon"),
            Make("Smallmouth Bass", difficulty: 3, rarity: 2, score: 66, sprite: "Bass Fish Icon"),
            Make("Largemouth Bass", difficulty: 3, rarity: 2, score: 68, sprite: "Bass Fish Icon"),
            Make("Tiger Trout",     difficulty: 3, rarity: 2, score: 70, sprite: "Iceshard Fish Icon"),
            Make("Gar",             difficulty: 3, rarity: 2, score: 72, sprite: "Sea snake Icon"),
            Make("Snook",           difficulty: 4, rarity: 2, score: 78, sprite: "Bass Fish Icon"),
            Make("Cobia",           difficulty: 4, rarity: 2, score: 80, sprite: "Bass Fish Icon"),
            Make("Tarpon",          difficulty: 4, rarity: 2, score: 85, sprite: "Bass Fish Icon"),
            Make("Muskie",          difficulty: 4, rarity: 2, score: 88, sprite: "Sea snake Icon"),

            // ── Legendary (rarity 3) ──────────────────────────────────────────
            Make("Ember Koi",       difficulty: 4, rarity: 3, score: 155, sprite: "Magma Fish Icon"),
            Make("Golden Carp",     difficulty: 4, rarity: 3, score: 160, sprite: "Gold Fish Icon"),
            Make("Ghost Koi",       difficulty: 4, rarity: 3, score: 165, sprite: "Snowball Fish Icon"),
            Make("Phantom Eel",     difficulty: 4, rarity: 3, score: 170, sprite: "Sea snake Icon"),
            Make("Celestial Trout", difficulty: 4, rarity: 3, score: 180, sprite: "Iceshard Fish Icon"),
            Make("Koi Dragon",      difficulty: 4, rarity: 3, score: 190, sprite: "Fancy Fish Icon"),
            Make("Arapaima",        difficulty: 5, rarity: 3, score: 200, sprite: "Magma Snake Icon"),
            Make("Ancient Sturgeon",difficulty: 5, rarity: 3, score: 215, sprite: "Magma Snake Icon"),
            Make("Ironscale Pike",  difficulty: 5, rarity: 3, score: 225, sprite: "Iceshard Fish Icon"),
            Make("Void Carp",       difficulty: 5, rarity: 3, score: 235, sprite: "Acid Fish Icon"),
            Make("Midnight Bass",   difficulty: 5, rarity: 3, score: 240, sprite: "Bass Fish Icon"),
            Make("Leviathan Bass",  difficulty: 5, rarity: 3, score: 250, sprite: "Bass Fish Icon"),
            Make("Abyssal Tench",   difficulty: 5, rarity: 3, score: 265, sprite: "Acid Fish Icon"),
            Make("Shimmerfin",      difficulty: 5, rarity: 3, score: 280, sprite: "Neon Jellyfish Icon"),
            Make("Soulfish",        difficulty: 5, rarity: 3, score: 320, sprite: "Neon Jellyfish Icon"),

            // ── Mythical (rarity 4) ───────────────────────────────────────────
            Make("Kylesiwi",        difficulty: 5, rarity: 4, score: 999, sprite: "Fancy Fish Icon"),
        };
    }

    static FishData Make(string name, int difficulty, int rarity, int score, string sprite = null)
    {
        var fd = ScriptableObject.CreateInstance<FishData>();
        fd.fishName   = name;
        fd.difficulty = difficulty;
        fd.rarity     = rarity;
        fd.scoreValue = score;
        if (sprite != null)
            fd.fishSprite = LoadSprite(sprite);
        return fd;
    }

    static Sprite LoadSprite(string name)
    {
        var tex = Resources.Load<Texture2D>("FishSprites/" + name);
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 16f);
    }
}
