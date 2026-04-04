using Content.Shared._CorvaxGoob.ChameleonStamp;
using Content.Shared.Paper;

namespace Content.Server._CorvaxGoob.ChameleonStamp;

public sealed partial class ChameleonStampSystem : SharedChameleonStampSystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChameleonStampComponent, ChameleonStampApplySettingsMessage>(OnApplySettings);

        UpdatePresets();
    }

    private void OnApplySettings(Entity<ChameleonStampComponent> entity, ref ChameleonStampApplySettingsMessage args)
    {
        if (!TryComp<StampComponent>(entity, out var stamp))
            return;

        var presets = GetAllPresets();

        if (!ValidatePreset(args.SelectedStampColorPrototype, out var stampColorPrototype, out var stampColorComponent))
            return;

        if (!ValidatePreset(args.SelectedStampStatePrototype, out var stampStatePrototype, out var stampStateComponent))
            return;

        if (!ValidatePreset(args.SelectedStampSpritePrototype, out var stampSpritePrototype, out var stampSpriteComponent))
            return;

        // Color
        entity.Comp.SelectedStampColorPrototype = args.SelectedStampColorPrototype;
        entity.Comp.CustomStampColor = args.CustomStampColor;
        stamp.StampedColor = stampColorComponent.StampedColor == args.CustomStampColor ? stampColorComponent.StampedColor : args.CustomStampColor;

        // StampedName
        stamp.StampedName = Loc.GetString(args.StampedName ?? stampSpriteComponent.StampedName);

        // Metadata
        if (args.CustomName == null)
            ApplyName(entity, stampSpritePrototype.Name);
        else
            ApplyName(entity, args.CustomName, true);

        if (args.CustomDescription == null)
            ApplyDescription(entity, stampSpritePrototype.Description);
        else
            ApplyDescription(entity, args.CustomDescription, true);

        // StampState
        entity.Comp.SelectedStampStatePrototype = args.SelectedStampStatePrototype;
        stamp.StampState = stampStateComponent.StampState;

        // StampSprite
        entity.Comp.SelectedStampSpritePrototype = args.SelectedStampSpritePrototype;

        Dirty(entity);
        Dirty(entity, stamp);

        void ApplyDescription(Entity<ChameleonStampComponent> entity, string value, bool applyToComponent = false)
        {
            _metaData.SetEntityDescription(entity, value);
            entity.Comp.CustomDescription = applyToComponent ? value : null;
        }

        void ApplyName(Entity<ChameleonStampComponent> entity, string value, bool applyToComponent = false)
        {
            _metaData.SetEntityName(entity, value);
            entity.Comp.CustomName = applyToComponent ? value : null;
        }
    }
}
