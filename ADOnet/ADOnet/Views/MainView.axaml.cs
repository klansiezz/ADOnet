using System.Linq;
using ADOnet.Data;
using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;

namespace ADOnet.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        
        if (Design.IsDesignMode)
        {
            return;
        }

        LoadData();
    }

    private void LoadData()
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated();
        DbSeed.Seed(db);

        ClientsListBox.ItemsSource = db.Clients.ToList();
        CostumesListBox.ItemsSource = db.Costumes.ToList();

        var rentals = db.Rentals
            .Include(rental => rental.Client)
            .Include(rental => rental.Costume)
            .ToList();

        RentalsListBox.ItemsSource = rentals;
        ActiveRentalsListBox.ItemsSource = rentals
            .Where(rental => !rental.IsReturned)
            .ToList();
    }
}
