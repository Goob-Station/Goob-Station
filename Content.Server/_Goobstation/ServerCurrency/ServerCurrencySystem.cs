using Content.Shared.GameTicking;

namespace Content.Server._Goobstation.ServerCurrency
{
    /// <summary>
    /// Connects <see cref="ServerCurrencyManager"/> to the simulation state.
    /// </summary>
    public sealed class ServerCurrencySystem : EntitySystem
    {
        [Dependency] private readonly ServerCurrencyManager _currencyMan = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnd);
        }

        private void OnRoundEnd(RoundRestartCleanupEvent ev)
        {
            _currencyMan.Save();
        }
    }
}
