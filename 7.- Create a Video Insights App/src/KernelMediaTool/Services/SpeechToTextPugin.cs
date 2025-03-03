using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelMediaTool.Services
{
    public class SpeechToTextPlugin
    {
        WhisperTranscriptionService _whisperService;
        public SpeechToTextPlugin(WhisperTranscriptionService whisperService)
        {
            _whisperService = whisperService;
        }

        [KernelFunction]
        [Description("Transcribe .mp3 files using Whisper. This method should only process .mp3 files, not video files")]
        public async Task<string> GetTranscription(string filePath, string language, Kernel kernel)
        {
            var transcript =
                await _whisperService.TranscribeAudioAsync(filePath, language, kernel);
            return transcript;
        }
    }
}
