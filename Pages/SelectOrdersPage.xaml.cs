using ButchersCashier.Models;

namespace ButchersCashier.Pages;

public partial class SelectOrdersPage : ContentPage
{
    public List<SelectableOrder> Items { get; set; }
    public event Action<List<OrderRow>> OnOrdersSelected;
    public List<OrderRow> SelectedOrders =>
        Items.Where(x => x.IsSelected).Select(x => x.Order).ToList();

    public SelectOrdersPage(List<OrderRow> orders)
    {
        InitializeComponent();

        Items = orders
            .Select(o => new SelectableOrder { Order = o })
            .ToList();

        OrdersList.ItemsSource = Items;
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        var selected = Items
            .Where(x => x.IsSelected)
            .Select(x => x.Order)
            .ToList();

        OnOrdersSelected?.Invoke(selected);

        await Navigation.PopModalAsync();
    }
}