using System.IO;
using System.Threading.Tasks;
using Content.Goobstation.Common.CCVar;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;

namespace Content.Goobstation.Server.Database;

/// <summary>
/// Your shitty database-related ideas, now in goobmod!
/// </summary>
public interface IGoobstationDbManager
{
    void Init();
    void Shutdown();

    Task<List<BrainrotWord>> GetBrainrotWordsAsync();
    Task AddBrainrotWordAsync(string keyword, string username);
    Task RemoveBrainrotWordAsync(string keyword);
}


public sealed class GoobstationDbManager : IGoobstationDbManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IResourceManager _res = default!;
    [Dependency] private readonly ILogManager _logMgr = default!;

    private ISawmill _sawmill = default!;
    private DbContextOptions? _options;
    private bool _isPostgres;

    public void Init()
    {
        _sawmill = _logMgr.GetSawmill("goob.db");

        var engine = _cfg.GetCVar(GoobCVars.GoobDatabaseEngine).ToLower();
        switch (engine)
        {
            case "sqlite":
                SetupSqlite();
                break;
            case "postgres":
                SetupPostgres();
                break;
            default:
                throw new InvalidDataException($"Unknown Goobstation database engine: {engine}");
        }

        // Run migrations
        using var ctx = CreateContext();
        ctx.Database.Migrate();
    }

    public void Shutdown()
    {
    }

    private void SetupSqlite()
    {
        _isPostgres = false;
        var path = _cfg.GetCVar(GoobCVars.GoobDatabaseSqlitePath);

        string finalPath;
        if (_res.UserData.RootDir != null)
        {
            finalPath = Path.Combine(_res.UserData.RootDir, path);
        }
        else
        {
            finalPath = ":memory:";
        }

        _sawmill.Debug($"Using Goobstation SQLite DB: {finalPath}");

        var builder = new DbContextOptionsBuilder<GoobstationSqliteServerDbContext>();
        builder.UseSqlite($"Data Source={finalPath}");
        _options = builder.Options;
    }

    private void SetupPostgres()
    {
        _isPostgres = true;
        var host = _cfg.GetCVar(GoobCVars.GoobDatabasePgHost);
        var port = _cfg.GetCVar(GoobCVars.GoobDatabasePgPort);
        var db = _cfg.GetCVar(GoobCVars.GoobDatabasePgDatabase);
        var user = _cfg.GetCVar(GoobCVars.GoobDatabasePgUsername);
        var pass = _cfg.GetCVar(GoobCVars.GoobDatabasePgPassword);

        var connString = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = db,
            Username = user,
            Password = pass
        }.ConnectionString;

        _sawmill.Debug($"Using Goobstation Postgres: {host}:{port}/{db}");

        var builder = new DbContextOptionsBuilder<GoobstationPostgresServerDbContext>();
        builder.UseNpgsql(connString);
        _options = builder.Options;
    }

    private GoobstationServerDbContext CreateContext()
    {
        if (_isPostgres)
            return new GoobstationPostgresServerDbContext((DbContextOptions<GoobstationPostgresServerDbContext>)_options!);
        return new GoobstationSqliteServerDbContext((DbContextOptions<GoobstationSqliteServerDbContext>)_options!);
    }

    public async Task<List<BrainrotWord>> GetBrainrotWordsAsync()
    {
        await using var ctx = CreateContext();
        return await ctx.BrainrotWords.ToListAsync();
    }

    public async Task AddBrainrotWordAsync(string keyword, string username)
    {
        await using var ctx = CreateContext();
        ctx.BrainrotWords.Add(new BrainrotWord { Keyword = keyword, Username = username });
        await ctx.SaveChangesAsync();
    }

    public async Task RemoveBrainrotWordAsync(string keyword)
    {
        await using var ctx = CreateContext();
        var word = await ctx.BrainrotWords.FirstOrDefaultAsync(w => w.Keyword == keyword);
        if (word != null)
        {
            ctx.BrainrotWords.Remove(word);
            await ctx.SaveChangesAsync();
        }
    }
}
