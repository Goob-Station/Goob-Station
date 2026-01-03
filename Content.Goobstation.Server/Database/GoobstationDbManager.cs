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

        var _ = _cfg.GetCVar(GoobCVars.GoobDatabaseEngine).ToLower() switch
        { "sqlite" => SetupSqlite()
        , "postgres" => SetupPostgres()
        , var engine => throw new InvalidDataException($"Unknown Goobstation database engine: {engine}")
        };

        using var ctx = CreateContext();
        ctx.Database.Migrate();
    }

    public void Shutdown() { }

    private bool SetupSqlite()
    { _isPostgres = false;
      var path = _cfg.GetCVar(GoobCVars.GoobDatabaseSqlitePath);
      var finalPath = _res.UserData.RootDir is { } root
          ? Path.Combine(root, path)
          : ":memory:";
      _sawmill.Debug($"Using Goobstation SQLite DB: {finalPath}");
      var builder = new DbContextOptionsBuilder<GoobstationSqliteServerDbContext>();
      builder.UseSqlite($"Data Source={finalPath}");
      _options = builder.Options;
      return true;
    }

    private bool SetupPostgres()
    { _isPostgres = true;
      var (host, port, db, user, pass) =
          ( _cfg.GetCVar(GoobCVars.GoobDatabasePgHost)
          , _cfg.GetCVar(GoobCVars.GoobDatabasePgPort)
          , _cfg.GetCVar(GoobCVars.GoobDatabasePgDatabase)
          , _cfg.GetCVar(GoobCVars.GoobDatabasePgUsername)
          , _cfg.GetCVar(GoobCVars.GoobDatabasePgPassword)
          );
      var connString = new NpgsqlConnectionStringBuilder
          { Host = host
          , Port = port
          , Database = db
          , Username = user
          , Password = pass
          }.ConnectionString;
      _sawmill.Debug($"Using Goobstation Postgres: {host}:{port}/{db}");
      var builder = new DbContextOptionsBuilder<GoobstationPostgresServerDbContext>();
      builder.UseNpgsql(connString);
      _options = builder.Options;
      return true;
    }

    private GoobstationServerDbContext CreateContext() => _isPostgres switch
    { true => new GoobstationPostgresServerDbContext((DbContextOptions<GoobstationPostgresServerDbContext>)_options!)
    , false => new GoobstationSqliteServerDbContext((DbContextOptions<GoobstationSqliteServerDbContext>)_options!)
    };

    public async Task<List<BrainrotWord>> GetBrainrotWordsAsync()
    { await using var ctx = CreateContext();
      return await ctx.BrainrotWords.ToListAsync();
    }

    public async Task AddBrainrotWordAsync(string keyword, string username)
    { await using var ctx = CreateContext();
      ctx.BrainrotWords.Add(new BrainrotWord { Keyword = keyword, Username = username });
      await ctx.SaveChangesAsync();
    }

    public async Task RemoveBrainrotWordAsync(string keyword)
    { await using var ctx = CreateContext();
      if (await ctx.BrainrotWords.FirstOrDefaultAsync(w => w.Keyword == keyword) is { } word)
      { ctx.BrainrotWords.Remove(word);
        await ctx.SaveChangesAsync();
      }
    }
}
