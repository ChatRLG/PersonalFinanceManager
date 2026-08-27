using PersonalFinanceManager.Mobile.ViewModels;

namespace PersonalFinanceManager.Mobile.Views;

public partial class TransactionListPage : ContentPage
{
    private readonly TransactionListViewModel _vm;

    public TransactionListPage(TransactionListViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // LoadAsync also runs a foreground sync attempt first (see the ViewModel) —
        // this is what makes offline-created transactions reconcile promptly once
        // the app is reopened with connectivity restored.
        await _vm.LoadAsync();
    }
}
