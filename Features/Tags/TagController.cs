using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace conduit_api.Features.Tags
{
    [Route("tags")]
    public class TagController : Controller
    {
        private readonly IMediator _mediator;

        public TagController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<TagsEnvelope> Get()
        {
            return await _mediator.Send(new List.Query());
        }
    }
}