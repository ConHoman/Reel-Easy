/// <summary>
/// A fish that has been hooked and is passing through the minigame pipeline.
/// Carries both the species data and the flavor rolled at spawn time.
/// </summary>
public struct CaughtFish
{
    public FishData   data;
    public FishFlavor flavor;

    public string DisplayName =>
        flavor == FishFlavor.None
            ? data.fishName
            : FishFlavorData.Get(flavor).displayName + " " + data.fishName;
}
