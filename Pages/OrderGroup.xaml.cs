using ButchersCashier.Models;
using System.Collections.ObjectModel;

namespace ButchersCashier.Pages;

public partial class OrderGroup : ContentPage
{

    public string Name { get; set; }
    public ObservableCollection<OrderRow> Orders { get; set; } = new();

}