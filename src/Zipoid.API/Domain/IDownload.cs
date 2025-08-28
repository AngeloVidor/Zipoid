using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zipoid.API.Domain
{
    public interface IDownload
    {
        Task DownloadAudioAsync(string videoUrl, Guid userId);
    }
}