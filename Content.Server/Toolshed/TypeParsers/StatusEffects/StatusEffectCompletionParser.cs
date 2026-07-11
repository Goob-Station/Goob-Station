using Content.Shared.StatusEffectNew;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Syntax;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Server.Toolshed.TypeParsers.StatusEffects;

public sealed class StatusEffectCompletionParser : CustomCompletionParser<EntProtoId>
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override CompletionResult? TryAutocomplete(ParserContext ctx, CommandArgument? arg)
    {
        return CompletionResult.FromHintOptions(
            _entityManager.System<StatusEffectsSystem>().StatusEffectPrototypes,
            GetArgHint(arg));
    }
}
