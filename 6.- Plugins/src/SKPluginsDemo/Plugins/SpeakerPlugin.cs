using Microsoft.SemanticKernel;
using SKPluginsDemo.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKPluginsDemo.Plugins
{
    public class SpeakerPlugin
    {
        List<SpeakerModel> _speakers;

        public SpeakerPlugin(List<SpeakerModel> speakers)
        {
            _speakers = speakers;
        }

        [KernelFunction("get_battery_levels")]
        [Description("Returns the battery levels of all speakers.")]
        public List<int?> GetBatteryLevels()
        {
            List<int?> batteryLevels = new List<int?>();
            foreach (var speaker in _speakers)
            {
                batteryLevels.Add(speaker.BatteryLevel);
            }
            return batteryLevels;
        }
        [KernelFunction("set_volume_speaker")]
        [Description("Sets the volume of a speaker.")]
        public void ChangeVolume(int id, int volume)
        {
            var speaker = _speakers.FirstOrDefault(s => s.Id == id);
            if (speaker != null)
            {
                speaker.VolumeLevel = volume;
            }
        }
        [KernelFunction("get_volume_speaker")]
        [Description("Get the volume of a speaker")]
        public List<int?> GetVolumeLevels()
        {
            List<int?> volumeLevels = new List<int?>();
            foreach (var speaker in _speakers)
            {
                volumeLevels.Add(speaker.VolumeLevel);
            }
            return volumeLevels;
        }
    }

}
