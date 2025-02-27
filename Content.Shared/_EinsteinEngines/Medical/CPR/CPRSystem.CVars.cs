using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Shared._EinsteinEngines.CPR
{
    public sealed partial class CPRSystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        public bool EnableCPR { get; private set; }
        public bool HealsAirloss { get; private set; }
        public bool ReducesRot { get; private set; }
        public float ResuscitationChance { get; private set; }
        public float RotReductionMultiplier { get; private set; }
        public float AirlossReductionMultiplier { get; private set; }

        private void InitializeCVars()
        {
            Subs.CVar(_cfg, CCVars.EnableCPR, value => EnableCPR = value, true);
            Subs.CVar(_cfg, CCVars.CPRHealsAirloss, value => HealsAirloss = value, true);
            Subs.CVar(_cfg, CCVars.CPRReducesRot, value => ReducesRot = value, true);
            Subs.CVar(_cfg, CCVars.CPRResuscitationChance, value => ResuscitationChance = value, true);
            Subs.CVar(_cfg, CCVars.CPRRotReductionMultiplier, value => RotReductionMultiplier = value, true);
            Subs.CVar(_cfg, CCVars.CPRAirlossReductionMultiplier, value => AirlossReductionMultiplier = value, true);
        }
    }
}
