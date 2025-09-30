namespace Content.Goobstation.Server.Books;

public sealed class BookData
{
    public string Title = "";
    public string Genre = "";
    public string Author = "";
    public string Desc = "";
    public List<string> Pages = new();

    public BookData(string title, string genre, string author, string desc, List<string> pages)
    {
        Title = title;
        Genre = genre;
        Author = author;
        Desc = desc;
        Pages = pages;
    }
}
