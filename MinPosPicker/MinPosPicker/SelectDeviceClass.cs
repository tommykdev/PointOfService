// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Windows.UI.Xaml;
using System.ComponentModel;

namespace MinPosPicker
{
    public class SelectDeviceClass : DependencyObject, INotifyPropertyChanged
    {
        string _DeviceId;
        public string DeviceId
        {
            get { return _DeviceId; }
            set
            {
                if (_DeviceId != value)
                {
                    _DeviceId = value;
                    OnPropertyChanged("DeviceId");
                }
            }
        }
        string _DeviceName;
        public string DeviceName
        {
            get { return _DeviceName; }
            set
            {
                if (_DeviceName != value)
                {
                    _DeviceName = value;
                    OnPropertyChanged("DeviceName");
                }
            }
        }

        Type _DeviceClass;
        public Type DeviceClass
        {
            get { return _DeviceClass; }
            set
            {
                if (_DeviceClass != value)
                {
                    _DeviceClass = value;
                    OnPropertyChanged("DeviceClass");
                }
            }
        }

        public string DeviceClassStr
        {
            get
            {
                return ShortClassName(DeviceClass);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public static string ShortClassName(Type devClass)
        {
            string str = devClass.ToString();
            return str.Substring(str.LastIndexOf('.') + 1);
        }

    }

}
