using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace conduit_api.Features.Profiles
{
    [Route("profiles")]
    public class ProfilesController : Controller
    {
        private readonly IMediator _mediator;

        public ProfilesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{username}")]
        public async Task<ProfileEnvelope> GetTask(string username)
        {
            return await _mediator.Send(new Details.Command() { Username = username });
        }
    }
}