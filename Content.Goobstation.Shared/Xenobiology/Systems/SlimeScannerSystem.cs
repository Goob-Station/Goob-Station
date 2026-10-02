using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Goobstation.Shared.Xenobiology.Components.Equipment;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;
using System.Text;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

public sealed class SlimeScannerSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private StringBuilder _sb = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeComponent, AfterInteractUsingEvent>(OnSlimeAfterInteractUsing);
        SubscribeLocalEvent<SlimeExtractComponent, AfterInteractUsingEvent>(OnExtractAfterInteractUsing);
    }

    private void OnSlimeAfterInteractUsing(Entity<SlimeComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (!CanSendTooltip(args))
            return;

        TrySendTooltip(args.User, ent, GenerateSlimeMarkup(ent));
        args.Handled = true;
    }

    private void OnExtractAfterInteractUsing(Entity<SlimeExtractComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (!CanSendTooltip(args))
            return;

        var loc = Loc.GetString("slime-scanner-examine-extract", ("reagents", GenerateExtractMarkup(ent)));
        TrySendTooltip(args.User, ent, loc);
        args.Handled = true;
    }

    private bool CanSendTooltip(AfterInteractUsingEvent args)
        => !args.Handled && args.Target != null && args.CanReach && HasComp<SlimeScannerComponent>(args.Used);

    private void TrySendTooltip(EntityUid player, EntityUid target, string message)
    {
        var markup = FormattedMessage.FromMarkupOrThrow(message);
        _examine.SendExamineTooltip(player, target, markup, false, true);
    }

    private string GenerateSlimeMarkup(Entity<SlimeComponent> ent)
    {
        var mutationChancePercent = MathF.Floor(ent.Comp.MutationChance * 100f);

        var sb = new StringBuilder();

        sb.AppendLine(Loc.GetString("slime-scanner-examine-slime-description", ("color", ent.Comp.SlimeColor.ToHex()), ("name", _proto.Index(ent.Comp.Breed).BreedName)));

        // all this shit for a good looking examine text. imagine.
        sb.Append($"{Loc.GetString("slime-scanner-examine-slime-mutations", ("chance", mutationChancePercent))} ");
        var mutations = ent.Comp.PotentialMutations.ToList();
        for (int i = 0; i < mutations.Count; i++)
        {
            var info = _proto.Index(mutations[i]);

            var color = "white";
            // todo make the colors work
            if (info.Components.TryGetComponent(nameof(SlimeComponent), out var sc))
                color = ((SlimeComponent) sc).SlimeColor.ToHex();

            sb.Append($"[color={color}]{info.BreedName}[/color]");

            if (i == mutations.Count - 1)
                sb.AppendLine(".");
            else
                sb.Append(", ");
        }

        sb.AppendLine(Loc.GetString("slime-scanner-examine-slime-extracts", ("num", ent.Comp.ExtractsProduced)));

        return sb.ToString();
    }

    private string GenerateExtractMarkup(Entity<SlimeExtractComponent> ent)
    {
        _sb.Clear();

        if (!TryComp<ReactiveComponent>(ent, out var reactive) || reactive.Reactions == null)
        {
            _sb.AppendLine(Loc.GetString("slime-scanner-examine-extract-unreactive"));
            return _sb.ToString();
        }

        var reactions = reactive.Reactions;
        for (int i = 0; i < reactions.Count; i++)
        {
            var item = reactions[i];
            if (item.Reagents == null)
                continue;

            var reagents = item.Reagents.ToList();
            for (int j = 0; j < reagents.Count; j++)
            {
                var reagent = reagents[j];
                if (!_proto.TryIndex<ReagentPrototype>(reagent, out var rid))
                    continue;

                _sb.Append($"[color={rid.SubstanceColor.ToHex()}]{rid.ID.ToLower()}[/color]");

                if (reagents.Count <= 1)
                    continue;

                // jic
                if (i == reagents.Count - 1)
                    _sb.Append("; ");
                else
                    _sb.Append(", ");
            }

            if (i == reactions.Count - 1)
                _sb.AppendLine(".");
            else
                _sb.Append(", ");
        }

        return _sb.ToString();
    }
}
