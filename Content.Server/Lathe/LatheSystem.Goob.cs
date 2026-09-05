using Content.Shared.Lathe;

namespace Content.Server.Lathe;

public sealed partial class LatheSystem
{
    [Dependency] private readonly IComponentFactory _factory = default!;

    /// <summary>
    /// Produces 0-time items that output into the storage automatically.
    /// Used in order to prevent stack overflows (of the server) caused by printing a lot of materials at once.
    /// TODO It's still not great and in the future should be replaced with something even more optimized.
    ///
    /// update i don't know why its needed this fucking way and i don't care. Im removing the dupe code method.
    /// tl;dr this just gets the next recipe and sets the current recipe again until there isnt any.
    /// </summary>
    private bool TryStartNextBulkRecipe(EntityUid uid, LatheComponent comp)
    {
        if (comp.Queue.First is not { } node) // break here, when queue empty it stop. wow.
            return false;

        var batch = node.Value;
        var recipe = _proto.Index(batch.Recipe);
        var time = _reagentSpeed.ApplySpeed(uid, recipe.CompleteTime) * comp.TimeMultiplier;

        if (time != TimeSpan.Zero)
            return false;

        batch.ItemsPrinted++;
        if (batch.ItemsPrinted >= batch.ItemsRequested || batch.ItemsPrinted < 0)
            comp.Queue.RemoveFirst();

        comp.CurrentRecipe = recipe;
        return true;
    }
}
