using FFMpegCore;
using FFMpegCore.Arguments;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelMediaTool.Services
{
    public class FFMPegUtils
    {
        public async Task<bool> TryExtractAudio(string videoPath)
        {
            string tempAudioPath =
                videoPath.Replace(".mp4", "_temp.mp3");

            if (FFMpeg.ExtractAudio(videoPath, tempAudioPath))
            {
                var finalAudioPath = videoPath.Replace(".mp4", ".mp3");
                if(await ReduceSize(tempAudioPath, finalAudioPath))
                {
                    File.Delete(tempAudioPath);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> ReduceSize(string inputFile, string outputFile)
        {
            return await FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputFile, overwrite: true, options => options
                            .WithCustomArgument("-ar 16000 -ac 1 -b:a 32k"))
                .ProcessAsynchronously();
        }

        public async Task<string> CutVideo(string inputPath, string newVideoName, 
            TimeSpan startTime, TimeSpan endTime, Kernel kernel, bool burnSubtitles = false)
        {
            var outputPath =
                Path.Combine(Path.GetDirectoryName(inputPath), newVideoName);

            string ffmpegStartTime =
                startTime.ToString(@"hh\:mm\:ss");
            string ffmpegEndTime =
                endTime.ToString(@"hh\:mm\:ss");

            var result = await FFMpegArguments
                .FromFileInput(inputPath)
                .OutputToFile(outputPath, overwrite: true, options => options
                    .WithCustomArgument($"-ss {ffmpegStartTime} -to {ffmpegEndTime}"))
                .ProcessAsynchronously();

            if(burnSubtitles)
            {
                await TryExtractAudio(outputPath);
                var newAudioFile =
                    outputPath.Replace(".mp4", ".mp3");
                await kernel.InvokeAsync("SpeechToTextPlugin", "GetTranscription",
                    new() { { "filePath", $"{newAudioFile}" }, { "language", "en" } });

                result = await FFMpegArguments
                    .FromFileInput(outputPath)
                    .OutputToFile($"{outputPath}_sub.mp4",
                    overwrite: true,
                    options =>
                    {
                        options
                            .WithVideoFilters((videoOptions) =>
                            {
                                videoOptions
                                    .HardBurnSubtitle(SubtitleHardBurnOptions
                                        .Create(newAudioFile.Replace("mp3", "srt")));
                        
                            });
                    })
                    .ProcessAsynchronously();
            }


            if(result)
            {
                return $"Video cut successfully at {outputPath}";
            }
            else
            {
                return "Failed to cut video";
            }

        }

    }
}
