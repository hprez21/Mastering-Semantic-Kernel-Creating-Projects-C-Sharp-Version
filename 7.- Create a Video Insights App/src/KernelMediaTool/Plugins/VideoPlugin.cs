using KernelMediaTool.Services;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelMediaTool.Plugins
{
    public class VideoPlugin
    {
        FFMPegUtils _ffmpegUtils;
        public VideoPlugin(FFMPegUtils ffmpegUtils)
        {
            _ffmpegUtils = ffmpegUtils;
        }

        [KernelFunction]
        public async Task<string> ExtractAduioFromVideo(string inputPath)
        {
            if(!File.Exists(inputPath))
            {
                return "The file does not exist.";
            }
            if(! await _ffmpegUtils.TryExtractAudio(inputPath))
            {
                return "Failed to extract audio";
            }
            return $"Audio extract successfully at {inputPath.Replace(".mp4", ".mp3")}";
        }
        [KernelFunction]
        [Description("Cut the original video file from a start time to an end time")]
        public async Task<string> CutVideo(string inputPath, string newVideoName,
            TimeSpan startTime, TimeSpan endTime, Kernel kernel, bool burnSubtitles = false)
        {
            return await _ffmpegUtils.CutVideo(inputPath, newVideoName, startTime, endTime, kernel, burnSubtitles);
        }
    }
}
