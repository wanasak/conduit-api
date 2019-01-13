using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using conduit_api.Infrastructure;
using conduit_api.Infrastructure.Errors;
using conduit_api.Infrastructure.Security;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace conduit_api.Features.Users
{
    public class Details
    {
        public class Command : IRequest<UserEnvelope>
        {
            public string Username { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Username).NotNull().NotEmpty();
            }
        }

        public class CommandHandler : IRequestHandler<Command, UserEnvelope>
        {
            private readonly ConduitContext _context;
            private readonly IMapper _mapper;
            private readonly IJwtTokenGenerator _jwtTokenGenerator;

            public CommandHandler(ConduitContext context, IMapper mapper, IJwtTokenGenerator jwtTokenGenerator)
            {
                _context = context;
                _mapper = mapper;
                _jwtTokenGenerator = jwtTokenGenerator;
            }

            public async Task<UserEnvelope> Handle(Command message, CancellationToken cancellationToken)
            {
                var person = await _context.Persons
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Username == message.Username, cancellationToken);

                if (person == null)
                {
                    throw new RestException(HttpStatusCode.NotFound, new { User = Constants.NOT_FOUND });
                }

                var user = _mapper.Map<Domain.Person, User>(person);
                user.Token = await _jwtTokenGenerator.CreateToken(person.Username);

                return new UserEnvelope(user);
            }
        }
    }
}