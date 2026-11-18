using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Shared.Humanoid.Markings;

public sealed partial class MarkingManager
{
    private FrozenDictionary<string, string> _stillByWagging = FrozenDictionary<string, string>.Empty;

    private void CacheWagging()
    {
        var dict = new Dictionary<string, string>();
        foreach (var marking in Markings.Values)
        {
            if (marking.Wagging is { } wag)
                dict.TryAdd(wag.Id, marking.ID);
        }

        _stillByWagging = dict.ToFrozenDictionary();
    }

    public bool IsWaggingVariant(string id)
    {
        return _stillByWagging.ContainsKey(id);
    }

    public bool TryGetWaggingMarking(string id, [NotNullWhen(true)] out string? wagging)
    {
        wagging = Markings.TryGetValue(id, out var proto) ? proto.Wagging?.Id : null;
        return wagging != null;
    }

    public bool TryGetStillMarking(string id, [NotNullWhen(true)] out string? still)
    {
        return _stillByWagging.TryGetValue(id, out still);
    }

    public IReadOnlyDictionary<string, MarkingPrototype> WithoutWaggingVariants(IReadOnlyDictionary<string, MarkingPrototype> markings)
    {
        return markings.Where(x => !IsWaggingVariant(x.Key)).ToDictionary(x => x.Key, x => x.Value);
    }
}
