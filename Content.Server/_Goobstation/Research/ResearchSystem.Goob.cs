using Content.Server.Research.Components;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using System.Linq;
using Content.Shared._Goobstation.Research;

namespace Content.Server.Research.Systems;

public sealed partial class ResearchSystem
{
    private void UpdateFancyConsoleInterface(EntityUid uid, ResearchConsoleComponent? component = null, ResearchClientComponent? clientComponent = null)
    {
        if (!Resolve(uid, ref component, ref clientComponent, false))
            return;

        ResearchConsoleBoundInterfaceState state;

        Dictionary<string, ResearchAvailablity> list = new();
        foreach (var proto in PrototypeManager.EnumeratePrototypes<TechnologyPrototype>().ToList())
        {
            list.Add(proto.ID, ResearchAvailablity.Unavailable);
        }

        if (TryGetClientServer(uid, out var serverUid, out var serverComponent, clientComponent))
        {
            if (clientComponent.Server.HasValue && TryComp<TechnologyDatabaseComponent>(clientComponent.Server.Value, out var db))
            {
                var toList = list.ToList();
                for (var i = 0; i < toList.Count; i++)
                {
                    var item = PrototypeManager.Index<TechnologyPrototype>(toList[i].Key);
                    if (CompOrNull<TechnologyDatabaseComponent>(serverUid)?.UnlockedTechnologies.Contains(item.ID) ?? false)
                        list[item.ID] = ResearchAvailablity.Researched;
                    else if (item.TechnologyPrerequisites.Count <= 0)
                        list[item.ID] = serverComponent.Points >= item.Cost ? ResearchAvailablity.Available : ResearchAvailablity.Unavailable;
                    else
                    {
                        var success = true;
                        foreach (var required in item.TechnologyPrerequisites)
                        {
                            if (!db.UnlockedTechnologies.Contains(required))
                                success = false;
                        }

                        var available = success && serverComponent.Points >= item.Cost;
                        if (success)
                            list[item.ID] = available ? ResearchAvailablity.Available : ResearchAvailablity.Unavailable;
                    }
                }
            }

            var points = clientComponent.ConnectedToServer ? serverComponent.Points : 0;
            state = new ResearchConsoleBoundInterfaceState(points, list);
        }
        else
        {
            state = new ResearchConsoleBoundInterfaceState(default, list);
        }

        _uiSystem.SetUiState(uid, ResearchConsoleUiKey.Key, state);
    }
}
