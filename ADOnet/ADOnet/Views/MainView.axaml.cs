using System;
using System.Linq;
using ADOnet.Data;
using ADOnet.Data.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;

namespace ADOnet.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        // В предпросмотре редактора базу не открываем.
        if (Design.IsDesignMode)
        {
            return;
        }

        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            DbSeed.Seed(db);
        }

        RefreshAll();
    }

    // ---------------------------------------------------------------
    // Общие вспомогательные методы
    // ---------------------------------------------------------------

    private static string? NullIfEmpty(string? text)
    {
        var trimmed = text?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static string GetErrorText(Exception exception)
        => exception.InnerException?.Message ?? exception.Message;

    private static void ShowMessage(TextBlock target, string text, bool isError)
    {
        target.Text = text;
        target.Foreground = isError ? Brushes.Red : Brushes.Green;
    }

    private static bool TryReadInt(string? text, out int value)
        => int.TryParse(text?.Trim(), out value);

    // ---------------------------------------------------------------
    // Обновление всех списков (включая выпадающие и вкладку «На руках»)
    // ---------------------------------------------------------------

    private void RefreshAll()
    {
        using var db = new AppDbContext();

        var clients = db.Clients.ToList();
        var costumes = db.Costumes.ToList();
        var rentals = db.Rentals
            .Include(rental => rental.Client)
            .Include(rental => rental.Costume)
            .ToList();

        ClientsListBox.ItemsSource = clients;
        RentalClientComboBox.ItemsSource = clients;

        CostumesListBox.ItemsSource = costumes;
        RentalCostumeComboBox.ItemsSource = costumes;

        RentalsListBox.ItemsSource = rentals;
        ActiveRentalsListBox.ItemsSource = rentals
            .Where(rental => !rental.IsReturned)
            .ToList();
    }

    // ---------------------------------------------------------------
    // Клиенты
    // ---------------------------------------------------------------

    private Client? ReadClientForm()
    {
        var fullName = NullIfEmpty(ClientFullNameTextBox.Text);
        if (fullName is null)
        {
            ShowMessage(ClientsMessageTextBlock, "Введите ФИО клиента.", true);
            return null;
        }

        var phone = NullIfEmpty(ClientPhoneTextBox.Text);
        if (phone is null)
        {
            ShowMessage(ClientsMessageTextBlock, "Введите телефон клиента.", true);
            return null;
        }

        var birthDate = ClientBirthDatePicker.SelectedDate;
        if (birthDate is not null && birthDate.Value.Date > DateTime.Today)
        {
            ShowMessage(ClientsMessageTextBlock, "Дата рождения не может быть в будущем.", true);
            return null;
        }

        return new Client
        {
            FullName = fullName,
            Phone = phone,
            Email = NullIfEmpty(ClientEmailTextBox.Text),
            BirthDate = birthDate,
            IsRegular = ClientIsRegularCheckBox.IsChecked == true,
        };
    }

    private void ClearClientForm()
    {
        ClientFullNameTextBox.Text = string.Empty;
        ClientPhoneTextBox.Text = string.Empty;
        ClientEmailTextBox.Text = string.Empty;
        ClientBirthDatePicker.SelectedDate = null;
        ClientIsRegularCheckBox.IsChecked = false;
    }

    private void ClientsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // После обновления списка выбор сбрасывается: тогда ничего не делаем.
        if (ClientsListBox.SelectedItem is not Client client)
        {
            return;
        }

        ClientFullNameTextBox.Text = client.FullName;
        ClientPhoneTextBox.Text = client.Phone;
        ClientEmailTextBox.Text = client.Email;
        ClientBirthDatePicker.SelectedDate = client.BirthDate;
        ClientIsRegularCheckBox.IsChecked = client.IsRegular;
        ClientsMessageTextBlock.Text = string.Empty;
    }

    private void AddClientButton_Click(object? sender, RoutedEventArgs e)
    {
        var newClient = ReadClientForm();
        if (newClient is null)
        {
            return;
        }

        try
        {
            using var db = new AppDbContext();
            db.Clients.Add(newClient);
            db.SaveChanges();
        }
        catch (Exception exception)
        {
            ShowMessage(ClientsMessageTextBlock, $"Не удалось сохранить: {GetErrorText(exception)}", true);
            return;
        }

        ClearClientForm();
        RefreshAll();
        ShowMessage(ClientsMessageTextBlock, "Клиент добавлен.", false);
    }

    private void SaveClientButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ClientsListBox.SelectedItem is not Client selected)
        {
            ShowMessage(ClientsMessageTextBlock, "Выберите клиента в списке.", true);
            return;
        }

        var formData = ReadClientForm();
        if (formData is null)
        {
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var client = db.Clients.Find(selected.Id);
            if (client is null)
            {
                ShowMessage(ClientsMessageTextBlock, "Клиент не найден: возможно, он уже удалён.", true);
                RefreshAll();
                return;
            }

            client.FullName = formData.FullName;
            client.Phone = formData.Phone;
            client.Email = formData.Email;
            client.BirthDate = formData.BirthDate;
            client.IsRegular = formData.IsRegular;
            db.SaveChanges();
        }
        catch (Exception exception)
        {
            ShowMessage(ClientsMessageTextBlock, $"Не удалось сохранить: {GetErrorText(exception)}", true);
            return;
        }

        RefreshAll();
        ShowMessage(ClientsMessageTextBlock, "Изменения сохранены.", false);
    }

    private void DeleteClientButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ClientsListBox.SelectedItem is not Client selected)
        {
            ShowMessage(ClientsMessageTextBlock, "Выберите клиента в списке.", true);
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var client = db.Clients.Find(selected.Id);
            if (client is null)
            {
                ShowMessage(ClientsMessageTextBlock, "Клиент не найден: возможно, он уже удалён.", true);
                RefreshAll();
                return;
            }

            db.Clients.Remove(client);
            db.SaveChanges();
        }
        catch (Exception exception)
        {
            ShowMessage(ClientsMessageTextBlock, $"Не удалось удалить: {GetErrorText(exception)}", true);
            return;
        }

        ClearClientForm();
        RefreshAll();
        ShowMessage(ClientsMessageTextBlock, "Клиент удалён.", false);
    }

    // ---------------------------------------------------------------
    // Костюмы
    // ---------------------------------------------------------------

    private Costume? ReadCostumeForm()
    {
        var name = NullIfEmpty(CostumeNameTextBox.Text);
        if (name is null)
        {
            ShowMessage(CostumesMessageTextBlock, "Введите название костюма.", true);
            return null;
        }

        var style = NullIfEmpty(CostumeStyleTextBox.Text);
        if (style is null)
        {
            ShowMessage(CostumesMessageTextBlock, "Введите стиль костюма.", true);
            return null;
        }

        if (!TryReadInt(CostumeSizeTextBox.Text, out var size) || size <= 0)
        {
            ShowMessage(CostumesMessageTextBlock, "Размер должен быть целым числом больше 0.", true);
            return null;
        }

        if (!TryReadInt(CostumeDailyPriceTextBox.Text, out var dailyPrice) || dailyPrice <= 0)
        {
            ShowMessage(CostumesMessageTextBlock, "Цена за сутки должна быть целым числом больше 0.", true);
            return null;
        }

        return new Costume
        {
            Name = name,
            Style = style,
            Size = size,
            Color = NullIfEmpty(CostumeColorTextBox.Text),
            DailyPrice = dailyPrice,
            IsAvailable = CostumeIsAvailableCheckBox.IsChecked == true,
        };
    }

    private void ClearCostumeForm()
    {
        CostumeNameTextBox.Text = string.Empty;
        CostumeStyleTextBox.Text = string.Empty;
        CostumeSizeTextBox.Text = string.Empty;
        CostumeColorTextBox.Text = string.Empty;
        CostumeDailyPriceTextBox.Text = string.Empty;
        CostumeIsAvailableCheckBox.IsChecked = false;
    }

    private void CostumesListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CostumesListBox.SelectedItem is not Costume costume)
        {
            return;
        }

        CostumeNameTextBox.Text = costume.Name;
        CostumeStyleTextBox.Text = costume.Style;
        CostumeSizeTextBox.Text = costume.Size.ToString();
        CostumeColorTextBox.Text = costume.Color;
        CostumeDailyPriceTextBox.Text = costume.DailyPrice.ToString();
        CostumeIsAvailableCheckBox.IsChecked = costume.IsAvailable;
        CostumesMessageTextBlock.Text = string.Empty;
    }

    private void AddCostumeButton_Click(object? sender, RoutedEventArgs e)
    {
        var newCostume = ReadCostumeForm();
        if (newCostume is null)
        {
            return;
        }

        try
        {
            using var db = new AppDbContext();
            db.Costumes.Add(newCostume);
            db.SaveChanges();
        }
        catch (Exception exception)
        {
            ShowMessage(CostumesMessageTextBlock, $"Не удалось сохранить: {GetErrorText(exception)}", true);
            return;
        }

        ClearCostumeForm();
        RefreshAll();
        ShowMessage(CostumesMessageTextBlock, "Костюм добавлен.", false);
    }

    private void SaveCostumeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CostumesListBox.SelectedItem is not Costume selected)
        {
            ShowMessage(CostumesMessageTextBlock, "Выберите костюм в списке.", true);
            return;
        }

        var formData = ReadCostumeForm();
        if (formData is null)
        {
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var costume = db.Costumes.Find(selected.Id);
            if (costume is null)
            {
                ShowMessage(CostumesMessageTextBlock, "Костюм не найден: возможно, он уже удалён.", true);
                RefreshAll();
                return;
            }

            costume.Name = formData.Name;
            costume.Style = formData.Style;
            costume.Size = formData.Size;
            costume.Color = formData.Color;
            costume.DailyPrice = formData.DailyPrice;
            costume.IsAvailable = formData.IsAvailable;
            db.SaveChanges();
        }
        catch (Exception exception)
        {
            ShowMessage(CostumesMessageTextBlock, $"Не удалось сохранить: {GetErrorText(exception)}", true);
            return;
        }

        RefreshAll();
        ShowMessage(CostumesMessageTextBlock, "Изменения сохранены.", false);
    }

    private void DeleteCostumeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CostumesListBox.SelectedItem is not Costume selected)
        {
            ShowMessage(CostumesMessageTextBlock, "Выберите костюм в списке.", true);
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var costume = db.Costumes.Find(selected.Id);
            if (costume is null)
            {
                ShowMessage(CostumesMessageTextBlock, "Костюм не найден: возможно, он уже удалён.", true);
                RefreshAll();
                return;
            }

            db.Costumes.Remove(costume);
            db.SaveChanges();
        }
        catch (Exception exception)
        {
            ShowMessage(CostumesMessageTextBlock, $"Не удалось удалить: {GetErrorText(exception)}", true);
            return;
        }

        ClearCostumeForm();
        RefreshAll();
        ShowMessage(CostumesMessageTextBlock, "Костюм удалён.", false);
    }

    // ---------------------------------------------------------------
    // Аренды
    // ---------------------------------------------------------------

    private Rental? ReadRentalForm()
    {
        if (RentalClientComboBox.SelectedItem is not Client client)
        {
            ShowMessage(RentalsMessageTextBlock, "Выберите клиента.", true);
            return null;
        }

        if (RentalCostumeComboBox.SelectedItem is not Costume costume)
        {
            ShowMessage(RentalsMessageTextBlock, "Выберите костюм.", true);
            return null;
        }

        var startDate = RentalStartDatePicker.SelectedDate;
        if (startDate is null)
        {
            ShowMessage(RentalsMessageTextBlock, "Выберите дату начала аренды.", true);
            return null;
        }

        if (!TryReadInt(RentalDaysTextBox.Text, out var days) || days <= 0)
        {
            ShowMessage(RentalsMessageTextBlock, "Количество суток должно быть целым числом больше 0.", true);
            return null;
        }

        if (!TryReadInt(RentalTotalPriceTextBox.Text, out var totalPrice) || totalPrice < 0)
        {
            ShowMessage(RentalsMessageTextBlock, "Итоговая сумма должна быть целым числом, не меньше 0.", true);
            return null;
        }

        var returnDate = RentalReturnDatePicker.SelectedDate;
        if (returnDate is not null && returnDate.Value.Date < startDate.Value.Date)
        {
            ShowMessage(RentalsMessageTextBlock, "Дата возврата не может быть раньше даты начала.", true);
            return null;
        }

        // Записываем Id, а не сами объекты: объекты загружены другим контекстом.
        return new Rental
        {
            ClientId = client.Id,
            CostumeId = costume.Id,
            StartDate = startDate.Value,
            ReturnDate = returnDate,
            Days = days,
            TotalPrice = totalPrice,
            IsReturned = RentalIsReturnedCheckBox.IsChecked == true,
        };
    }

    private void ClearRentalForm()
    {
        RentalClientComboBox.SelectedItem = null;
        RentalCostumeComboBox.SelectedItem = null;
        RentalStartDatePicker.SelectedDate = null;
        RentalReturnDatePicker.SelectedDate = null;
        RentalDaysTextBox.Text = string.Empty;
        RentalTotalPriceTextBox.Text = string.Empty;
        RentalIsReturnedCheckBox.IsChecked = false;
    }

    private void AddRentalButton_Click(object? sender, RoutedEventArgs e)
    {
        var newRental = ReadRentalForm();
        if (newRental is null)
        {
            return;
        }

        try
        {
            using var db = new AppDbContext();
            db.Rentals.Add(newRental);
            db.SaveChanges();
        }
        catch (Exception exception)
        {
            ShowMessage(RentalsMessageTextBlock, $"Не удалось сохранить: {GetErrorText(exception)}", true);
            return;
        }

        ClearRentalForm();
        RefreshAll();
        ShowMessage(RentalsMessageTextBlock, "Аренда добавлена.", false);
    }

    private void RentalsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // После обновления списка выбор сбрасывается: тогда ничего не делаем.
        if (RentalsListBox.SelectedItem is not Rental rental)
        {
            return;
        }

        RentalClientComboBox.SelectedItem = RentalClientComboBox.Items
            .OfType<Client>()
            .FirstOrDefault(client => client.Id == rental.ClientId);
        RentalCostumeComboBox.SelectedItem = RentalCostumeComboBox.Items
            .OfType<Costume>()
            .FirstOrDefault(costume => costume.Id == rental.CostumeId);
        RentalStartDatePicker.SelectedDate = rental.StartDate;
        RentalReturnDatePicker.SelectedDate = rental.ReturnDate;
        RentalDaysTextBox.Text = rental.Days.ToString();
        RentalTotalPriceTextBox.Text = rental.TotalPrice.ToString();
        RentalIsReturnedCheckBox.IsChecked = rental.IsReturned;
        RentalsMessageTextBlock.Text = string.Empty;
    }

    private void SaveRentalButton_Click(object? sender, RoutedEventArgs e)
    {
        if (RentalsListBox.SelectedItem is not Rental selected)
        {
            ShowMessage(RentalsMessageTextBlock, "Выберите аренду в списке.", true);
            return;
        }

        var formData = ReadRentalForm();
        if (formData is null)
        {
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var rental = db.Rentals.Find(selected.Id);
            if (rental is null)
            {
                ShowMessage(RentalsMessageTextBlock, "Аренда не найдена: возможно, она уже удалена.", true);
                RefreshAll();
                return;
            }

            rental.ClientId = formData.ClientId;
            rental.CostumeId = formData.CostumeId;
            rental.StartDate = formData.StartDate;
            rental.ReturnDate = formData.ReturnDate;
            rental.Days = formData.Days;
            rental.TotalPrice = formData.TotalPrice;
            rental.IsReturned = formData.IsReturned;
            db.SaveChanges();
        }
        catch (Exception exception)
        {
            ShowMessage(RentalsMessageTextBlock, $"Не удалось сохранить: {GetErrorText(exception)}", true);
            return;
        }

        ClearRentalForm();
        RefreshAll();
        ShowMessage(RentalsMessageTextBlock, "Изменения сохранены.", false);
    }

    private void ReturnRentalButton_Click(object? sender, RoutedEventArgs e)
    {
        if (RentalsListBox.SelectedItem is not Rental selected)
        {
            ShowMessage(RentalsMessageTextBlock, "Выберите аренду в списке.", true);
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var rental = db.Rentals.Find(selected.Id);
            if (rental is null)
            {
                ShowMessage(RentalsMessageTextBlock, "Аренда не найдена: возможно, она уже удалена.", true);
                RefreshAll();
                return;
            }

            if (rental.IsReturned)
            {
                ShowMessage(RentalsMessageTextBlock, "Этот костюм уже возвращён.", true);
                return;
            }

            // Дата возврата не может быть раньше даты начала аренды.
            var today = DateTime.Today;
            rental.IsReturned = true;
            rental.ReturnDate = today < rental.StartDate.Date ? rental.StartDate.Date : today;

            var costume = db.Costumes.Find(rental.CostumeId);
            if (costume is not null)
            {
                costume.IsAvailable = true;
            }

            db.SaveChanges();
        }
        catch (Exception exception)
        {
            ShowMessage(RentalsMessageTextBlock, $"Не удалось сохранить: {GetErrorText(exception)}", true);
            return;
        }

        ClearRentalForm();
        RefreshAll();
        ShowMessage(RentalsMessageTextBlock, "Костюм возвращён.", false);
    }

    private void DeleteRentalButton_Click(object? sender, RoutedEventArgs e)
    {
        if (RentalsListBox.SelectedItem is not Rental selected)
        {
            ShowMessage(RentalsMessageTextBlock, "Выберите аренду в списке.", true);
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var rental = db.Rentals.Find(selected.Id);
            if (rental is null)
            {
                ShowMessage(RentalsMessageTextBlock, "Аренда не найдена: возможно, она уже удалена.", true);
                RefreshAll();
                return;
            }

            db.Rentals.Remove(rental);
            db.SaveChanges();
        }
        catch (Exception exception)
        {
            ShowMessage(RentalsMessageTextBlock, $"Не удалось удалить: {GetErrorText(exception)}", true);
            return;
        }

        RefreshAll();
        ShowMessage(RentalsMessageTextBlock, "Аренда удалена.", false);
    }
}
