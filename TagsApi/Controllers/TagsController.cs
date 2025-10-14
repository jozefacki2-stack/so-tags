using Microsoft.AspNetCore.Mvc;
using TagsApi.Services;

namespace TagsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _service;
        public TagsController(ITagService service) => _service = service;


        /// <summary>Stronicowane tagi z sortowaniem.</summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] string sortBy = "name", [FromQuery] string order = "asc")
            => Ok(await _service.GetPageAsync(page, pageSize, sortBy.ToLower(), order.ToLower()));
    }
}
