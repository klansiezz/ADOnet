using System.Text;
using ADOnet.Data;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine($"Файл базы: {AppDbContext.DbPath}");
Console.WriteLine();

using var db = new AppDbContext();
db.Database.EnsureCreated();
DbSeed.Seed(db);

Console.WriteLine("=== Клиенты ===");
foreach (var client in db.Clients.ToList())
{
    Console.WriteLine(client);
}

Console.WriteLine();
Console.WriteLine("=== Костюмы ===");
foreach (var costume in db.Costumes.ToList())
{
    Console.WriteLine(costume);
}

Console.WriteLine();
Console.WriteLine("=== Аренды ===");
var rentals = db.Rentals
    .Include(rental => rental.Client)
    .Include(rental => rental.Costume)
    .ToList();
foreach (var rental in rentals)
{
    Console.WriteLine(rental);
}
