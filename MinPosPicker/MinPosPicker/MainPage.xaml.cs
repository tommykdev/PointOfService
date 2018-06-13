// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MinPosPicker
{
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        public Type DeviceClass = null;
        public string PreferredDeviceName = "";
        public string PreferredDeviceId = "";

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            PosDeviceWatchers.StartWatchers(Dispatcher);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SetCurrentPage(typeof(PickPreferred));
        }

        Type CurrentPageClass = typeof(MainPage);
        public void SetCurrentPage(Type pageclass)
        {
            if (pageclass == typeof(MainPage))
            {
                ContentFrame.Visibility = Visibility.Collapsed;
            }
            else
            {
                ContentFrame.Visibility = Visibility.Visible;
                ContentFrame.Navigate(pageclass);
            }
            CurrentPageClass = pageclass;
        }

    }
}
