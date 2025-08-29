using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace Zipoid.API.Domain
{
    public interface IDownload
    {
        Task<Download> DownloadAudioAsync(string videoUrl, Guid userId);
    }
}