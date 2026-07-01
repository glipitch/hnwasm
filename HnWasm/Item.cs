namespace HnWasm;
public record Item(
    string? By,
    int Id,
    int Descendants,
    int[]? Kids,
    int Score,
    int? Time,
    string? Title,
    string? Url,
    string? Text);
