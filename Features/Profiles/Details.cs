using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace conduit_api.Features.Profiles
{
    public class Details
    {
        public class Command : IRequest<ProfileEnvelope>
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

        public class CommandHandler : IRequestHandler<Command, ProfileEnvelope>
        {
            private readonly IProfileReader _profileReader;

            public CommandHandler(IProfileReader profileReader)
            {
                _profileReader = profileReader;
            }

            public async Task<ProfileEnvelope> Handle(Command message, CancellationToken cancellationToken)
            {
                return await _profileReader.ReadProfile(message.Username);
            }
        }
    }
}