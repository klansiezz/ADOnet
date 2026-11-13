namespace ADOnet.Data.Models;

public class Costume
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
    public int Size { get; set; }
    public string? Color { get; set; }
    public int DailyPrice { get; set; }
    public bool IsAvailable { get; set; }

    public List<Rental> Rentals { get; set; } = new();

    public override string ToString() => $"{Name} ({Style}), размер {Size}, {DailyPrice} руб./сутки";
}
