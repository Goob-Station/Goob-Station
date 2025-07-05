using Robust.Shared.Player;

namespace Content.Goobstation.Common.MisandryBox;

public interface ISpiderManager
{
    public void Initialize();

    public void RequestSpider();
    public void AddTemporarySpider(ICommonSession? victim = null);
    public void AddPermanentSpider(ICommonSession? victim = null);
    public void ClearTemporarySpiders();
}
