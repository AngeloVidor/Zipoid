using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zipoid.API.Application.Security;
using Zipoid.API.Domain;

namespace Zipoid.API.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class MainController : ControllerBase
    {
        private readonly ICoordinator _coordinator;
        private readonly ITrackMetadataCacher _trackMetadataCacher;

        public MainController(ICoordinator coordinator, ITrackMetadataCacher trackMetadataCacher)
        {
            _coordinator = coordinator;
            _trackMetadataCacher = trackMetadataCacher;
        }

        [HttpPost("process-playlist")]
        public async Task<IActionResult> ProcessPlaylist([FromBody] string playlistUrl)
        {
            if (!HttpContext.Items.TryGetValue("userId", out var userIdObj) || userIdObj is not Guid userId || userId == Guid.Empty)
                return BadRequest("User is not authenticated.");

            await _coordinator.CoordinateAsync(playlistUrl, userId);
            return Ok("Playlist processing started.");
        }

        [HttpGet("tracks")]
        public async Task<IActionResult> GetAllTracks()
        {
            var tracks = await _trackMetadataCacher.GetMetadataAsync();
            return Ok(tracks);
        }
    }

}