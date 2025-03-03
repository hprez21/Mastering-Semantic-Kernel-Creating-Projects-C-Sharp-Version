using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SKPluginsDemo.Models
{
    public class SpeakerModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("volume_level")]
        [Description("The current volume level of the speaker, ranging from 0 to 100.")]
        public int? VolumeLevel { get; set; }

        [JsonPropertyName("is_muted")]
        public bool? IsMuted { get; set; }

        [JsonPropertyName("playback_status")]
        public PlaybackStatus? PlaybackStatus { get; set; }

        [JsonPropertyName("connected_device")]
        public string? ConnectedDevice { get; set; }

        [JsonPropertyName("battery_level")]
        [Description("The battery level of the speaker as a percentage")]
        public int? BatteryLevel { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PlaybackStatus
    {
        Stopped,
        Playing,
        Paused,
        Buffering
    }

}
