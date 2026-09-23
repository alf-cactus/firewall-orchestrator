using System.Net;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace FWO.Data
{
    public class NetworkZoneDeviceIpRange
    {
        [JsonProperty("dev_id"), JsonPropertyName("dev_id")]
        public int DeviceId { get; set; }

        [JsonProperty("ip_range_id"), JsonPropertyName("ip_range_id")]
        public int IpRangeId { get; set; }
        [JsonProperty("order_to_root"), JsonPropertyName("order_to_root")]
        public int? OrderToRoot { get; set; }
        [JsonProperty("order_to_internet"), JsonPropertyName("order_to_internet")]
        public int? OrderToInternet { get; set; }

    }
}
