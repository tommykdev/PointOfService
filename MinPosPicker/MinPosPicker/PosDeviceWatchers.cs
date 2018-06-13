// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using Windows.Devices.PointOfService;

namespace MinPosPicker
{
    static public class PosDeviceWatchers
    {
        static Type[] classes = { typeof(BarcodeScanner), typeof(PosPrinter), typeof(CashDrawer), typeof(MagneticStripeReader), typeof(LineDisplay) };
        public static List<DeviceClassWatcher> PosWatchers;
        public static DeviceClassWatcherCallbackRoutine CombinedWatcherCallback = null;

        static public void StartWatchers (CoreDispatcher dispatcher)
        {
            PosWatchers = new List<DeviceClassWatcher>();
            foreach (Type ty in classes)
            {
                DeviceClassWatcher dcw = new DeviceClassWatcher(ty, GetPosSelector(ty), dispatcher);
                PosWatchers.Add(dcw);
            }
        }

        private static string GetPosSelector(Type ty)
        {
            if (ty == typeof(BarcodeScanner))
            {
                return BarcodeScanner.GetDeviceSelector();
            }
            if (ty == typeof(PosPrinter))
            {
                return PosPrinter.GetDeviceSelector();
            }
            if (ty == typeof(CashDrawer))
            {
                return CashDrawer.GetDeviceSelector();
            }
            if (ty == typeof(MagneticStripeReader))
            {
                return MagneticStripeReader.GetDeviceSelector();
            }
            if (ty == typeof(LineDisplay))
            {
                return LineDisplay.GetDeviceSelector();
            }
            return null;
        }

        static public ObservableCollection<DeviceInformation> FoundPosDevices(Type ty)
        {
            foreach (DeviceClassWatcher dcw in PosWatchers)
            {
                if (dcw.DeviceClass == ty)
                {
                    return dcw.FoundDeviceList;
                }
            }
            return null;
        }
    }

    public class DeviceClassWatcherCallbackRoutine
    {
        public enum ListOperation { Add, Delete, Update }
        public static ObservableCollection<SelectDeviceClass> SelectionList;
        public DeviceClassWatcherCallbackRoutine (ObservableCollection<SelectDeviceClass> selectionList)
        {
            SelectionList = selectionList;
        }
        public static async void ListUpdate(CoreDispatcher coreDispatcher, ListOperation oper, Type ty, DeviceInformation deviceInfo = null, string Id = null, string Name = null)
        {
            if (SelectionList == null) return;
            await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (SelectionList == null) return;
                    switch (oper)
                    {
                        case ListOperation.Add:
                            SelectDeviceClass sdc = new SelectDeviceClass();
                            sdc.DeviceClass = ty;
                            sdc.DeviceName = deviceInfo.Name;
                            sdc.DeviceId = deviceInfo.Id;
                            SelectionList.Add(sdc);
                            break;

                        case ListOperation.Delete:
                            for (int index = 0; index < SelectionList.Count; ++index)
                            {
                                if (SelectionList[index].DeviceId == Id)
                                {
                                    SelectionList.RemoveAt(index);
                                    break;
                                }
                            }
                            break;

                        case ListOperation.Update:
                            for (int index = 0; index < SelectionList.Count; ++index)
                            {
                                if (SelectionList[index].DeviceId == Id)
                                {
                                    // TBD
                                    break;
                                }
                            }
                            break;
                    }
                });
        }
    }


    public class DeviceClassWatcher
    {
        public Type DeviceClass;
        private DeviceWatcher deviceWatcher;
        private CoreDispatcher coreDispatcher;

        public DeviceClassWatcher(Type deviceClass, string deviceSelector, CoreDispatcher dispatcher)
        {
            DeviceClass = deviceClass;

            FoundDeviceList = new ObservableCollection<DeviceInformation>();
            coreDispatcher = dispatcher;

            deviceWatcher = DeviceInformation.CreateWatcher(deviceSelector);
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Start();
        }

        ~DeviceClassWatcher()
        {
            try
            {
                if (deviceWatcher.Status == DeviceWatcherStatus.Started) deviceWatcher.Stop();
            }
            catch (Exception) { }
            deviceWatcher = null;
        }

        public ObservableCollection<DeviceInformation> FoundDeviceList
        {
            get;
            private set;
        }

        public void Stop()
        {
            deviceWatcher.Added -= DeviceWatcher_Added;
            deviceWatcher.Removed -= DeviceWatcher_Removed;
            deviceWatcher.Updated -= DeviceWatcher_Updated;

            if (deviceWatcher.Status == DeviceWatcherStatus.Started)
            {
                deviceWatcher.Stop();
            }
        }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            await coreDispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    FoundDeviceList.Add(args);
                    DeviceClassWatcherCallbackRoutine.ListUpdate(coreDispatcher, DeviceClassWatcherCallbackRoutine.ListOperation.Add, DeviceClass, args);
                });
        }

        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            await coreDispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    for (int index = 0; index < FoundDeviceList.Count; ++index)
                    {
                        if (FoundDeviceList[index].Id == args.Id)
                        {
                            FoundDeviceList.RemoveAt(index);
                            DeviceClassWatcherCallbackRoutine.ListUpdate(coreDispatcher, DeviceClassWatcherCallbackRoutine.ListOperation.Delete, DeviceClass, null, args.Id);
                            break;
                        }
                    }
                });
        }

        private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            await coreDispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    for (int index = 0; index < FoundDeviceList.Count; ++index)
                    {
                        if (FoundDeviceList[index].Id == args.Id)
                        {
                            FoundDeviceList[index].Update(args);
                            DeviceClassWatcherCallbackRoutine.ListUpdate(coreDispatcher, DeviceClassWatcherCallbackRoutine.ListOperation.Update, DeviceClass, null, args.Id);
                            break;
                        }
                    }
                });
        }
    }
}
