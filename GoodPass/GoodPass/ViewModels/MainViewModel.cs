﻿using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoodPass.Contracts.Services;
using Microsoft.UI.Xaml.Navigation;

namespace GoodPass.ViewModels;

public class MainViewModel : ObservableRecipient
{
    #region Properties
    private bool _isBackEnabled;

    public INavigationService NavigationService
    {
        get;
    }

    public ICommand Login_UnLockCommand
    {
        get;
    }

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }
    #endregion

    #region Constructor and Basic Handlers
    public MainViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        Login_UnLockCommand = new RelayCommand(Login_UnLock);
    }

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = NavigationService.CanGoBack;

    public void Login_UnLock() => NavigationService.NavigateTo(typeof(ListDetailsViewModel).FullName!);
    // 带参数跳转
    public void Login_UnLock(bool isWarning) => NavigationService.NavigateTo(typeof(ListDetailsViewModel).FullName!);
    #endregion
}
