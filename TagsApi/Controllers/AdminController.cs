using Microsoft.AspNetCore.Mvc;
using TagsApi.Services;

namespace TagsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ITagService _service;
        public AdminController(ITagService service) => _service = service;


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var n = await _service.RefreshAsync();
            return Ok(new { imported = n });
        }
    }
}
