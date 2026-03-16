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
            Make("Minnow",          1, 1,   5, "Clown Fish Icon",      "Lives fast, dies young, gets eaten constantly."),
            Make("Roach",           1, 1,   7, "Redfin Fish Icon",     "The cockroach of the waterway. It will outlive you."),
            Make("Sunfish",         1, 1,   8, "Clown Fish Icon",      "Catches more rays than anything else in this pond."),
            Make("Bluegill",        1, 1,  10, "Blue Clown Fish Icon", "A reliable classic. Your grandpa's favorite."),
            Make("Gudgeon",         1, 1,  10, "Cod Fish Icon",        "Bottom feeder. No judgment."),
            Make("Rudd",            1, 1,  11, "Redfin Fish Icon",     "Red fins. Mediocre attitude."),
            Make("Crappie",         1, 1,  12, "Flat Fish Icon",       "The name says it all, really."),
            Make("Perch",           1, 1,  14, "Redfin Fish Icon",     "Striped and proud of it."),
            Make("Bream",           2, 1,  16, "Flat Fish Icon",       "Flat as a pancake and twice as unremarkable."),
            Make("Tench",           2, 1,  17, "Cod Fish Icon",        "Slimy to the touch. Oddly satisfying."),
            Make("Carp",            2, 1,  18, "Gold Fish Icon",       "The eternal optimist of the fish world."),
            Make("Catfish",         2, 1,  20, "Magma Snake Icon",     "Not a cat. Not a fish. Somehow both."),
            Make("Bass",            2, 1,  22, "Bass Fish Icon",       "Too cool for school. Not too cool for your hook."),
            Make("Trout",           2, 1,  25, "Samon fish Icon",      "The fish equivalent of a firm handshake."),
            Make("Barbel",          2, 1,  26, "Magma Snake Icon",     "Those whiskers aren't just for show."),
            Make("Chub",            2, 1,  20, "Cod Fish Icon",        "Round. Proud. Refuses to diet."),

            // ── Uncommon (rarity 2) ───────────────────────────────────────────
            Make("Flounder",        2, 2,  42, "Flat Fish Icon",       "Spends its whole life looking up at things it can't reach."),
            Make("Channel Drum",    2, 2,  44, "Bass Fish Icon",       "Makes a knocking sound when excited. Try not to take it personally."),
            Make("Walleye",         2, 2,  46, "Bass Fish Icon",       "My dad's favorite."),
            Make("Bowfin",          3, 2,  50, "Magma Snake Icon",     "A living fossil. Your rod might not be old enough for this one."),
            Make("Sheepshead",      3, 2,  52, "Flat Fish Icon",       "Has actual teeth. Human teeth. Please don't look."),
            Make("Salmon",          3, 2,  55, "Samon fish Icon",      "Born to run. Unfortunately, also born to be delicious."),
            Make("Rainbow Trout",   3, 2,  58, "Acid Fish Icon",       "So beautiful it almost feels wrong to eat it. Almost."),
            Make("Brown Trout",     3, 2,  58, "Samon fish Icon",      "The sensible cousin of the rainbow trout."),
            Make("Chain Pickerel",  3, 2,  60, "Sea snake Icon",       "All chain, no picnic."),
            Make("Northern Pike",   3, 2,  62, "Sea snake Icon",       "The apex predator of the pond. Humbled today."),
            Make("Redfish",         3, 2,  62, "Redfin Fish Icon",     "Fiery temperament. Genuinely beautiful scales."),
            Make("Lake Trout",      3, 2,  64, "Samon fish Icon",      "Deep water, deeper grudges."),
            Make("Steelhead",       3, 2,  65, "Samon fish Icon",      "A trout that thinks it's a salmon. Aspirational."),
            Make("Smallmouth Bass", 3, 2,  66, "Bass Fish Icon",       "Packs a serious punch for its size."),
            Make("Largemouth Bass", 3, 2,  68, "Bass Fish Icon",       "Can unhinge its jaw. Very useful at parties."),
            Make("Tiger Trout",     3, 2,  70, "Iceshard Fish Icon",   "Hybrid. Fierce. Impossible to predict."),
            Make("Gar",             3, 2,  72, "Sea snake Icon",       "A prehistoric weapon disguised as a fish."),
            Make("Snook",           4, 2,  78, "Bass Fish Icon",       "Very picky about being caught. Congratulations."),
            Make("Cobia",           4, 2,  80, "Bass Fish Icon",       "Follows sharks around hoping for scraps. Bold strategy."),
            Make("Tarpon",          4, 2,  85, "Bass Fish Icon",       "Leaps when hooked. Practically jumped right at you."),
            Make("Muskie",          4, 2,  88, "Sea snake Icon",       "Fish of 10,000 casts. Today was cast 10,001."),

            // ── Epic (rarity 3) ───────────────────────────────────────────────
            Make("Swordfish",       4, 3,  95, "Samon fish Icon",      "The gladiator of the open water. Your pond has no right to have one."),
            Make("Electric Eel",    4, 3, 100, "Sea snake Icon",       "Technically not an eel. Absolutely an electrocution hazard."),
            Make("Alligator Gar",   4, 3, 108, "Sea snake Icon",       "Old enough to have met a dinosaur. Grumpy about it."),
            Make("Permit",          4, 3, 112, "Flat Fish Icon",       "Notoriously finicky. Your lure must have been perfect. Or it just gave up."),
            Make("Striped Bass",    4, 3, 118, "Bass Fish Icon",       "The aristocrat of the coast. Slumming it in your local water today."),
            Make("Bull Shark",      5, 3, 125, "Magma Snake Icon",     "Freshwater tolerant. This is not a comforting fact."),
            Make("Goliath Grouper", 5, 3, 133, "Bass Fish Icon",       "Hundreds of pounds of fish that had somewhere to be. Not anymore."),
            Make("Coelacanth",      5, 3, 142, "Iceshard Fish Icon",   "A living fossil from four hundred million years ago. Still mad about it."),

            // ── Legendary (rarity 4) ──────────────────────────────────────────
            Make("Ember Koi",        4, 4, 155, "Magma Fish Icon",      "Ancient waters run hot through its fins."),
            Make("Golden Carp",      4, 4, 160, "Gold Fish Icon",       "Three wishes included. Unfortunately, they're all 'let me go.'"),
            Make("Ghost Koi",        4, 4, 165, "Snowball Fish Icon",   "Its scales shimmer like moonlight. Some say it never truly dies."),
            Make("Phantom Eel",      4, 4, 170, "Sea snake Icon",       "You can look right through it and still not see the whole picture."),
            Make("Celestial Trout",  4, 4, 180, "Iceshard Fish Icon",   "Fell from the sky into this pond. Still adjusting."),
            Make("Koi Dragon",       4, 4, 190, "Fancy Fish Icon",      "Said to bring fortune. Definitely brings bragging rights."),
            Make("Arapaima",         5, 4, 200, "Magma Snake Icon",     "One of the largest freshwater fish on earth. Respect."),
            Make("Ancient Sturgeon", 5, 4, 215, "Magma Snake Icon",     "Older than some civilizations. Let that sink in."),
            Make("Ironscale Pike",   5, 4, 225, "Iceshard Fish Icon",   "Its scales are harder than most metals. Your hook is exceptional."),
            Make("Void Carp",        5, 4, 235, "Acid Fish Icon",       "Swims through spaces that shouldn't exist."),
            Make("Midnight Bass",    5, 4, 240, "Bass Fish Icon",       "Only emerges in total darkness. You got lucky."),
            Make("Leviathan Bass",   5, 4, 250, "Bass Fish Icon",       "Ancient, massive, furious. Congratulations."),
            Make("Abyssal Tench",    5, 4, 265, "Acid Fish Icon",       "Dragged itself up from the deepest trench to ruin your day."),
            Make("Shimmerfin",       5, 4, 280, "Neon Jellyfish Icon",  "Refracts light in ways that physics hasn't named yet."),
            Make("Soulfish",         5, 4, 320, "Neon Jellyfish Icon",  "It's said catching one extends your life. Or costs it. Unclear."),

            // ── Mythical (rarity 5) ───────────────────────────────────────────
            Make("Kylesiwi",         5, 5,  999, "Fancy Fish Icon",      "Eugh just throw it back."),
            Make("Old Red Eye",      5, 5, 1200, "Magma Fish Icon",      "Has lived in this pond since before the pond existed."),
            Make("The Pale One",     5, 5, 1500, "Snowball Fish Icon",   "No angler has ever reported catching it. Until now."),
            Make("Neptune's Trophy", 5, 5, 2000, "Fancy Fish Icon",      "Awarded to one angler per century. Congratulations, probably."),
        };
    }

    static FishData Make(string name, int difficulty, int rarity, int score, string sprite, string flavorText)
    {
        var fd = ScriptableObject.CreateInstance<FishData>();
        fd.fishName   = name;
        fd.difficulty = difficulty;
        fd.rarity     = rarity;
        fd.scoreValue = score;
        fd.flavorText = flavorText;
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
