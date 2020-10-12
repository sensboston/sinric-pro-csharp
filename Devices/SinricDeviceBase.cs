using Sinric.json;

namespace Sinric.Devices
{
    public abstract class SinricDeviceBase
    {
        public string DeviceId { get; set; }

        internal abstract void ProcessMessage(SinricClient client, SinricMessage message);
    }
}
