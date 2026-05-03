using Content.Goobstation.Common.CCVar;
using Robust.Shared.Configuration;

namespace Content.Client.Options.UI.Tabs;

public sealed partial class GraphicsTab
{
    private sealed class OptionParticleQuality : BaseOption
    {
        private readonly IConfigurationManager _cfg;
        private readonly OptionDropDown _dropDown;

        private const int QualityOff = 0;
        private const int QualityLow = 1;
        private const int QualityMedium = 2;
        private const int QualityHigh = 3;
        private const int QualityUltra = 4;
        private const int QualityDefault = QualityHigh;

        public OptionParticleQuality(OptionsTabControlRow controller, IConfigurationManager cfg, OptionDropDown dropDown) : base(controller)
        {
            _cfg = cfg;
            _dropDown = dropDown;
            var button = dropDown.Button;
            button.AddItem(Loc.GetString("ui-options-particles-off"), QualityOff);
            button.AddItem(Loc.GetString("ui-options-particles-low"), QualityLow);
            button.AddItem(Loc.GetString("ui-options-particles-medium"), QualityMedium);
            button.AddItem(Loc.GetString("ui-options-particles-high"), QualityHigh);
            button.AddItem(Loc.GetString("ui-options-particles-ultra"), QualityUltra);
            button.OnItemSelected += args =>
            {
                _dropDown.Button.SelectId(args.Id);
                ValueChanged();
            };
        }

        public override void LoadValue()
        {
            _dropDown.Button.SelectId(_cfg.GetCVar(GoobCVars.ParticleQuality));
        }

        public override void SaveValue()
        {
            _cfg.SetCVar(GoobCVars.ParticleQuality, _dropDown.Button.SelectedId);
        }

        public override void ResetToDefault()
        {
            _dropDown.Button.SelectId(QualityDefault);
        }

        public override bool IsModified()
        {
            return _dropDown.Button.SelectedId != _cfg.GetCVar(GoobCVars.ParticleQuality);
        }

        public override bool IsModifiedFromDefault()
        {
            return _dropDown.Button.SelectedId != QualityDefault;
        }
    }
}
