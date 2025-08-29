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
        private readonly IOnDemand _onDemandService;

        public MainController(ICoordinator coordinator, ITrackMetadataCacher trackMetadataCacher, IOnDemand onDemandService)
        {
            _coordinator = coordinator;
            _trackMetadataCacher = trackMetadataCacher;
            _onDemandService = onDemandService;
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

        [HttpGet("tracks/{key}")]
        public async Task<IActionResult> GetTrack(string key)
        {
            var track = await _trackMetadataCacher.GetTrackByKeyAsync(key);

            if (track == null)
                return NotFound($"Track with key '{key}' not found.");

            return Ok(track);
        }

        [HttpPost("onDemand")]
        public async Task<IActionResult> OnDemand([FromQuery] string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest("Key is required.");

            if (!HttpContext.Items.TryGetValue("userId", out var userIdObj) || userIdObj is not Guid userId || userId == Guid.Empty)
                return BadRequest("User is not authenticated.");

            try
            {
                await _onDemandService.HandleAsync(key, userId);
                return Ok($"Download process started for track '{key}'.");
            }
            catch (Exception ex)
            {
                // aqui dá pra logar o erro se quiser
                return StatusCode(500, $"Error processing track '{key}': {ex.Message}");
            }
        }
    }

}