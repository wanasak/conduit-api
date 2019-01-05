using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using conduit_api.Infrastructure;
using conduit_api.Infrastructure.Errors;
using Microsoft.EntityFrameworkCore;

namespace conduit_api.Features.Profiles
{
    public class ProfileReader : IProfileReader
    {
        private readonly ConduitContext _context;
        private readonly ICurrentUserAccessor _currentUserAccessor;
        private readonly IMapper _mapper;

        public ProfileReader(ConduitContext context, ICurrentUserAccessor currentUserAccessor, IMapper mapper)
        {
            _context = context;
            _currentUserAccessor = currentUserAccessor;
            _mapper = mapper;
        }

        public async Task<ProfileEnvelope> ReadProfile(string username)
        {
            var currentUserName = _currentUserAccessor.GetCurrentUsername();

            var person = await _context.Persons.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == currentUserName);

            if (person == null)
            {
                throw new RestException(HttpStatusCode.NotFound, new { Article = Constants.NOT_FOUND });
            }

            var profile = _mapper.Map<Domain.Person, Profile>(person);

            if (currentUserName != null)
            {
                var currentPerson = await _context.Persons
                    .Include(x => x.Followers)
                    .Include(x => x.Following)
                    .FirstOrDefaultAsync(x => x.Username == currentUserName);
            }

            return new ProfileEnvelope(profile);
        }
    }
}