﻿using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using GoodPass.Contracts.ViewModels;
using GoodPass.Models;
using GoodPass.Services;

namespace GoodPass.ViewModels;

public class ListDetailsViewModel : ObservableRecipient, INavigationAware
{
    public void OnNavigatedFrom()
    {
    }

    private readonly GoodPassDataService _dataService;
    private GPData? _selectedData;
    public ObservableCollection<GPData> DataItems { get; private set; } = new ObservableCollection<GPData>();

    public ListDetailsViewModel(GoodPassDataService goodPassDataService)
    {
        _dataService = goodPassDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        DataItems.Clear();

        var datas = await _dataService.GetListDetailsDataAsync();

        foreach (var data in datas)
        {
            DataItems.Add(data);
        }
        EnsureItemSelected();
    }

    public GPData? SlectedData
    {
        get => _selectedData;
        set => SetProperty(ref _selectedData, value);
    }

    public void EnsureItemSelected()
    {
        if (SlectedData == null)
        {
            SlectedData = DataItems.First();
        }
    }

    public bool DeleteDataItem(GPData targetItem)
    {
        return DataItems.Remove(targetItem);
    }

    public void AddDataItem(GPData newData)
    {
        DataItems.Add(newData);
    }

    public bool ChangeItemPassword(GPData targetItem, string newPassword)
    {
        var index = DataItems.IndexOf(targetItem);
        var item = DataItems[index];
        var result = item.ChangePassword(newPassword);
        return result switch
        {
            "Success" => true,
            "SamePassword" => false,
            "Empty" => false,
            "Unknown Error" => false,
            _ => false,
        };
    }

    public bool ChangeItemUrl(GPData targetItem, string newUrl)
    {
        var index = DataItems.IndexOf(targetItem);
        var item = DataItems[index];
        return item.ChangeUrl(newUrl);
    }

    public bool ChangeItemAccountName(GPData targetItem, string newAccountName)
    {
        var index = DataItems.IndexOf(targetItem);
        var item = DataItems[index];
        return item.ChangeAccountName(newAccountName);
    }

    public bool ChangeItemPlatformName(GPData targetItem, string newPlatformName)
    {
        var index = DataItems.IndexOf(targetItem);
        if (index != -1)
        {
            var item = DataItems[index];
            var newItem = new GPData(newPlatformName, item.PlatformUrl, item.AccountName, item.EncPassword, DateTime.Now);
            DataItems.Add(newItem);
            index = DataItems.IndexOf(newItem);
            if (index != -1)
                return DataItems.Remove(item);
            else
                return false;
        }
        else
        {
            return false;
        }
    }
}
