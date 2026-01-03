using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Goobstation.Server.Database;

[Table("brainrot_words")]
public class BrainrotWord
{
    [Key]
    public int Id { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
