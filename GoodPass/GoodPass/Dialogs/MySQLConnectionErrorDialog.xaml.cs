// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GoodPass.Dialogs;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MySQLConnectionErrorDialog : ContentDialog
{
    public MySQLConnectionErrorDialog()
    {
        this.InitializeComponent();
    }

    public MySQLConnectionErrorDialog(string message)
    {
        this.InitializeComponent();
        MySQLConnectionErrorTextbox.Text = message;
    }

    public MySQLConnectionErrorDialog(string title, string message)
    {
        this.InitializeComponent();
        Title = title;
        MySQLConnectionErrorTextbox.Text = message;
    }
}
