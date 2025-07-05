using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Server.Database;

[Table("goob_spiders")]
public sealed class SpiderFriend
{
    [Key]
    public ulong Id { get; set; }

    public Guid Guid { get; set; }
}
