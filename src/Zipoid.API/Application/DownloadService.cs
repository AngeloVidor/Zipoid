using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Io;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Zipoid.API.Application
{
    public class DownloadService : Domain.IDownload
    {
        public async Task DownloadAudioAsync(string videoUrl, Guid userId)
        {
            var youtube = new YoutubeClient();

            var video = await youtube.Videos.GetAsync(videoUrl);
            var safeTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));

            var tracksFolder = Path.Combine(@"C:\Users\angel\Desktop\Zipoid\src\Zipoid.API\tmp", userId.ToString(), "tracks");
            if (!Directory.Exists(tracksFolder))
                Directory.CreateDirectory(tracksFolder);

            var outputFilePath = Path.Combine(tracksFolder, $"{safeTitle}.mp3");

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            var stream = await youtube.Videos.Streams.GetAsync(audioStreamInfo);

            await using var fileStream = File.Create(outputFilePath);
            await stream.CopyToAsync(fileStream);

            Console.WriteLine($"Áudio salvo em: {outputFilePath}");
        }
    }
}