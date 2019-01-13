using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using conduit_api.Infrastructure;
using conduit_api.Infrastructure.Security;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace conduit_api.Features.Users
{
    public class Edit
    {
        public class UserData
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Bio { get; set; }
            public string Image { get; set; }
        }

        public class Command : IRequest<UserEnvelope>
        {
            public UserData User { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.User).NotNull();
            }
        }

        public class CommandHandler : IRequestHandler<Command, UserEnvelope>
        {
            private readonly ConduitContext _context;
            private readonly IPasswordHasher _passwordHasher;
            private readonly ICurrentUserAccessor _currentUserAccessor;
            private readonly IMapper _mapper;

            public CommandHandler(ConduitContext context, IPasswordHasher passwordHasher,
                ICurrentUserAccessor currentUserAccessor, IMapper mapper)
            {
                _context = context;
                _passwordHasher = passwordHasher;
                _currentUserAccessor = currentUserAccessor;
                _mapper = mapper;
            }


            public async Task<UserEnvelope> Handle(Command request, CancellationToken cancellationToken)
            {
                var currentUsername = _currentUserAccessor.GetCurrentUsername();
                var person = await _context.Persons.Where(x => x.Username == currentUsername).FirstOrDefaultAsync(cancellationToken);

                person.Username = request.User.Username ?? person.Username;
                person.Email = request.User.Email ?? person.Email;
                person.Bio = request.User.Bio ?? person.Bio;
                person.Image = request.User.Image ?? person.Image;

                if (!string.IsNullOrWhiteSpace(request.User.Password))
                {
                    var salt = Guid.NewGuid().ToByteArray();
                    person.Hash = _passwordHasher.Hash(request.User.Password, salt);
                    person.Salt = salt;
                }

                await _context.SaveChangesAsync(cancellationToken);

                return new UserEnvelope(_mapper.Map<Domain.Person, User>(person));
            }
        }
    }
}