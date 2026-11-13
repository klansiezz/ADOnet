namespace ADOnet.Data.Models;

public class Rental
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int CostumeId { get; set; }
    public Costume Costume { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Days { get; set; }
    public int TotalPrice { get; set; }
    public bool IsReturned { get; set; }

    public override string ToString()
    {
        var status = IsReturned ? "возвращён" : "на руках";
        return $"{Client.FullName} — {Costume.Name}, с {StartDate:dd.MM.yyyy}, {Days} сут., {TotalPrice} руб. ({status})";
    }
}
