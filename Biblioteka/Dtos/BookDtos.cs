using System.Text.Json.Serialization;

namespace LibraryApi.Dtos;

public class BookDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("author")]
    public AuthorDto Author { get; set; } = new();
}

public class BookCreateDto
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("authorId")]
    public int AuthorId { get; set; }
}

public class BookUpdateDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("authorId")]
    public int AuthorId { get; set; }
}
