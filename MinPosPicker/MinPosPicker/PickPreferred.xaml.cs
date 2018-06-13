// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Windows.Devices.PointOfService;
using Windows.Devices.Enumeration;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace MinPosPicker
{
    public sealed partial class PickPreferred : Page
    {
        string preferredDeviceId = "";
        string preferredDeviceName = "";
        Type[] classes = { typeof(BarcodeScanner), typeof(PosPrinter), typeof(CashDrawer), typeof(MagneticStripeReader), typeof(LineDisplay) };

        public PickPreferred()
        {
            this.InitializeComponent();
            SelectionList = new ObservableCollection<SelectDeviceClass>();
            EnumDevicesListBox.ItemsSource = SelectionList;
            SelectionList.CollectionChanged += SelectionList_CollectionChanged;
        }

        private void SelectionList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            EnumDevicesListBox.ItemsSource = SelectionList;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            MainPage.Current.PreferredDeviceName = preferredDeviceName;
            MainPage.Current.PreferredDeviceId = preferredDeviceId;
        }

        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DeviceClassTitle.Text = "";
            foreach (Type ty in classes)
            {
                if (DeviceClassTitle.Text.Length > 0) DeviceClassTitle.Text += ", ";
                DeviceClassTitle.Text += SelectDeviceClass.ShortClassName(ty);
            }

            preferredDeviceName = MainPage.Current.PreferredDeviceName;
            PreferredItemBox.Text = DeviceInfoToDisplayName();

            // optionally wait on list to be built out, if not watchers yet
            if (PosDeviceWatchers.PosWatchers == null)
            {
                BusySignal.IsActive = true;
                await CreateDeviceList_OneTime(classes);
                BusySignal.IsActive = false;
            }

            // setup of watchers to add/delete/update list
            PosDeviceWatchers.CombinedWatcherCallback = new DeviceClassWatcherCallbackRoutine(SelectionList);
        }

        private string DeviceInfoToDisplayName ()
        {
            return preferredDeviceName + "(" + preferredDeviceId + ")";
        }

        private void EnumDevicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            int index = listBox.SelectedIndex;
            if (listBox.SelectedItem != null)
            {
                string item = listBox.SelectedItem.ToString();
                if (item.Length > 0) PickDeviceFromList(index);
                PreferredItemBox.Text = DeviceInfoToDisplayName();
            }
        }

        void PickDeviceFromList(int ix)
        {
            if (SelectionList != null)
            {
                if (ix >= 0 && ix < SelectionList.Count)
                {
                    preferredDeviceName = SelectionList[ix].DeviceName;
                    preferredDeviceId = SelectionList[ix].DeviceId;
                }
                else
                {
                    preferredDeviceName = "";
                    preferredDeviceId = "";
                }
            }
        }

        async Task<bool> CreateDeviceList_OneTime(Type[] deviceClasses)
        {
            foreach (Type ty in deviceClasses)
            {
                DeviceInformationCollection deviceList = null;
                if (ty == typeof(BarcodeScanner))
                {
                    deviceList = await DeviceInformation.FindAllAsync(BarcodeScanner.GetDeviceSelector());
                }
                else if (ty == typeof(PosPrinter))
                {
                    deviceList = await DeviceInformation.FindAllAsync(PosPrinter.GetDeviceSelector());
                }
                else if (ty == typeof(CashDrawer))
                {
                    deviceList = await DeviceInformation.FindAllAsync(CashDrawer.GetDeviceSelector());
                }
                else if (ty == typeof(MagneticStripeReader))
                {
                    deviceList = await DeviceInformation.FindAllAsync(MagneticStripeReader.GetDeviceSelector());
                }
                else if (ty == typeof(LineDisplay))
                {
                    deviceList = await DeviceInformation.FindAllAsync(LineDisplay.GetDeviceSelector());
                }
                AddToSelectionList(ty, deviceList);
            }
            return true;
        }
        void AddToSelectionList(Type ty, DeviceInformationCollection collection)
        {
            if (SelectionList == null) SelectionList = new ObservableCollection<SelectDeviceClass>();
            if (collection != null)
            {
                foreach (DeviceInformation di in collection)
                {
                    SelectDeviceClass sdc = new SelectDeviceClass();
                    sdc.DeviceClass = ty;
                    sdc.DeviceName = di.Name;
                    sdc.DeviceId = di.Id;
                    SelectionList.Add(sdc);
                }
            }
        }

        public ObservableCollection<SelectDeviceClass> SelectionList { get; set; }

    } // page

} // namespace
