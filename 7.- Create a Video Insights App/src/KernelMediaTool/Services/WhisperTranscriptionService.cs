using Azure.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0040


namespace KernelMediaTool.Services
{
    public class WhisperTranscriptionService
    {
        public async Task<string> TranscribeAudioAsync(string audioPath, string language, Kernel kernel)
        {
            var srtPath = audioPath.Replace(".mp3", ".srt");
            if(File.Exists(srtPath))
            {
                var content = File.ReadAllText(srtPath);
                return content;
            }

            var audioToTextService =
                kernel.GetRequiredService<IAudioToTextService>();

            var executionSettings =
                new OpenAIAudioToTextExecutionSettings
                {
                    Language = language,
                    Prompt = "This is a prompt. You should respect puntuation marks",
                    ResponseFormat = "srt",
                    Temperature = 0.3f
                };

            var audioFileStream = File.OpenRead(audioPath);
            var audioFileBinaryData =
                await BinaryData.FromStreamAsync(audioFileStream);
            AudioContent audioContent =
                new AudioContent(audioFileBinaryData, mimeType: null);

            var textContent =
                await audioToTextService.GetTextContentAsync(audioContent, executionSettings);
            var srtFilePath = audioPath.Replace(".mp3", ".srt");
            File.WriteAllText(srtFilePath, textContent.Text);
            return textContent.Text;

        }
    }
}
