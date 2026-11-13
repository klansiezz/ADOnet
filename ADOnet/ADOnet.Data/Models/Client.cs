namespace ADOnet.Data.Models;

public class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime? BirthDate { get; set; }
    public bool IsRegular { get; set; }

    public List<Rental> Rentals { get; set; } = new();

    public override string ToString() => $"{FullName}, тел. {Phone}";
}
