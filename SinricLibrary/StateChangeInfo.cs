using System;
using System.Collections.Generic;
using System.Text;
using SinricLibrary.Devices;

namespace SinricLibrary
{
    public class BasicStateChangeInfo
    {
        public string OldState { get; set; }
        public string NewState { get; set; }
        public bool Success { get; set; } = true;
        public SinricDeviceBase Device { get; set; }
        public string Action { get; set; }
        public Type ActionType { get; set; }
    }
}
