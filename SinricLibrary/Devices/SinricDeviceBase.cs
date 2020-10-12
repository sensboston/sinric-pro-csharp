using SinricLibrary.json;

namespace SinricLibrary.Devices
{
    public abstract class SinricDeviceBase
    {
        public string DeviceId { get; private set; }
        public abstract string DeviceType { get; protected set; }

        internal abstract void ProcessMessage(SinricClient client, SinricMessage message);

        protected SinricDeviceBase(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
