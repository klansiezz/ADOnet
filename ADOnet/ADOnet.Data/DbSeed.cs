using ADOnet.Data.Models;

namespace ADOnet.Data;

public static class DbSeed
{
    public static void Seed(AppDbContext db)
    {
        // Повторный запуск не должен удваивать записи.
        if (db.Clients.Any() || db.Costumes.Any() || db.Rentals.Any())
        {
            return;
        }

        var clients = new List<Client>
        {
            new() { FullName = "Иванова Анна Сергеевна", Phone = "+7 900 111-22-33", Email = "anna@example.com", BirthDate = new DateTime(1998, 5, 14), IsRegular = true },
            new() { FullName = "Петров Дмитрий Олегович", Phone = "+7 900 222-33-44", Email = null, BirthDate = new DateTime(1995, 11, 2), IsRegular = false },
            new() { FullName = "Смирнова Елена Викторовна", Phone = "+7 900 333-44-55", Email = "elena@example.com", BirthDate = null, IsRegular = true },
            new() { FullName = "Козлов Максим Андреевич", Phone = "+7 900 444-55-66", Email = null, BirthDate = new DateTime(2001, 8, 21), IsRegular = false },
        };

        var costumes = new List<Costume>
        {
            new() { Name = "Бархатное платье «Версаль»", Style = "элегантный", Size = 44, Color = "бордовый", DailyPrice = 2500, IsAvailable = true },
            new() { Name = "Вечерний смокинг", Style = "элегантный", Size = 50, Color = "чёрный", DailyPrice = 2000, IsAvailable = false },
            new() { Name = "Платье в стиле 1920-х", Style = "винтаж", Size = 42, Color = "золотой", DailyPrice = 1800, IsAvailable = true },
            new() { Name = "Костюм эльфийки", Style = "косплей", Size = 44, Color = null, DailyPrice = 1500, IsAvailable = false },
            new() { Name = "Мундир гусара", Style = "исторический", Size = 52, Color = "синий", DailyPrice = 2200, IsAvailable = true },
        };

        db.Clients.AddRange(clients);
        db.Costumes.AddRange(costumes);

        var rentals = new List<Rental>
        {
            new() { Client = clients[0], Costume = costumes[0], StartDate = new DateTime(2026, 10, 1), ReturnDate = new DateTime(2026, 10, 3), Days = 2, TotalPrice = 2 * 2500, IsReturned = true },
            new() { Client = clients[1], Costume = costumes[1], StartDate = new DateTime(2026, 11, 5), ReturnDate = null, Days = 3, TotalPrice = 3 * 2000, IsReturned = false },
            new() { Client = clients[2], Costume = costumes[3], StartDate = new DateTime(2026, 11, 8), ReturnDate = null, Days = 2, TotalPrice = 2 * 1500, IsReturned = false },
            new() { Client = clients[0], Costume = costumes[2], StartDate = new DateTime(2026, 9, 20), ReturnDate = new DateTime(2026, 9, 21), Days = 1, TotalPrice = 1 * 1800, IsReturned = true },
            new() { Client = clients[3], Costume = costumes[4], StartDate = new DateTime(2026, 10, 15), ReturnDate = new DateTime(2026, 10, 18), Days = 3, TotalPrice = 3 * 2200, IsReturned = true },
        };

        db.Rentals.AddRange(rentals);
        db.SaveChanges();
    }
}
