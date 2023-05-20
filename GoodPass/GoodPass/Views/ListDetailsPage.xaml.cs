using CommunityToolkit.WinUI.UI.Controls;
using GoodPass.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace GoodPass.Views;

public sealed partial class ListDetailsPage : Page
{
    #region Properties
    public ListDetailsViewModel ViewModel
    {
        get;
    }
    #endregion

    #region Constructor
    public ListDetailsPage()
    {
        App.ListDetailsVM = new ListDetailsViewModel();
        ViewModel = App.ListDetailsVM;
        InitializeComponent();

        
    }
    #endregion

    #region Handlers
    private void OnViewStateChanged(object sender, ListDetailsViewState e)
    {
        if (e == ListDetailsViewState.Both)
        {
            ViewModel.EnsureItemSelected();
        }
        if (App.containsInfobar)
        {
            Grid.SetRow(App.infoBar, 2);
            ContentArea.Children.Add(App.infoBar);
        }
    }

    private void ListDetailsViewControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (App.containsInfobar)
        {
            Grid.SetRow(App.infoBar, 2);
            ContentArea.Children.Add(App.infoBar);
        }
    }
    #endregion
}