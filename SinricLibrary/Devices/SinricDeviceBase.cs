using SinricLibrary.json;

namespace SinricLibrary.Devices
{
    public abstract class SinricDeviceBase
    {
        public string DeviceId { get; set; }

        internal abstract void ProcessMessage(SinricClient client, SinricMessage message);
    }
}
