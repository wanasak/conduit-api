using System.Net;
using System.Threading;
using System.Threading.Tasks;
using conduit_api.Infrastructure;
using conduit_api.Infrastructure.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace conduit_api.Features.Comments
{
    public class List
    {
        public class Command : IRequest<CommentsEnvelope>
        {
            public Command(string slug)
            {
                Slug = slug;
            }

            public string Slug { get; }
        }

        public class CommandHandler : IRequestHandler<Command, CommentsEnvelope>
        {
            private readonly ConduitContext _context;

            public CommandHandler(ConduitContext context)
            {
                _context = context;
            }

            public async Task<CommentsEnvelope> Handle(Command message, CancellationToken cancellationToken)
            {
                var article = await _context.Articles
                    .Include(x => x.Comments)
                        .ThenInclude(x => x.Author)
                    .FirstOrDefaultAsync(x => x.Slug == message.Slug, cancellationToken);

                if (article == null)
                {
                    throw new RestException(HttpStatusCode.NotFound, new { Article = Constants.NOT_FOUND });
                }

                return new CommentsEnvelope(article.Comments);
            }
        }
    }
}