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

        var allTechs = PrototypeManager.EnumeratePrototypes<TechnologyPrototype>().ToList();
        Dictionary<string, ResearchAvailability> techList;
        var points = 0;

        if (TryGetClientServer(uid, out var serverUid, out var server, clientComponent) &&
            TryComp<TechnologyDatabaseComponent>(serverUid, out var db))
        {
            var unlockedTechs = new HashSet<string>(db.UnlockedTechnologies);
            techList = allTechs.ToDictionary(
                proto => proto.ID,
                proto =>
                {
                    if (unlockedTechs.Contains(proto.ID))
                        return ResearchAvailability.Researched;

                    var prereqsMet = proto.TechnologyPrerequisites.All(p => unlockedTechs.Contains(p));
                    var canAfford = server.Points >= proto.Cost;

                    return prereqsMet ?
                        (canAfford ? ResearchAvailability.Available : ResearchAvailability.PrereqsMet)
                        : ResearchAvailability.Unavailable;
                });

            points = clientComponent.ConnectedToServer ? server.Points : 0;
        }
        else
        {
            techList = allTechs.ToDictionary(proto => proto.ID, _ => ResearchAvailability.Unavailable);
        }

        _uiSystem.SetUiState(uid, ResearchConsoleUiKey.Key,
            new ResearchConsoleBoundInterfaceState(points, techList));
    }
}
