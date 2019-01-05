using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using conduit_api.Domain;
using conduit_api.Infrastructure;
using conduit_api.Infrastructure.Errors;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace conduit_api.Features.Articles
{
    public class Edit
    {
        public class ArticleData
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Body { get; set; }
            public string[] TagList { get; set; }
        }

        public class Command : IRequest<ArticleEnvelope>
        {
            public ArticleData Article { get; set; }
            public string Slug { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Article).NotNull();
            }
        }

        public class Handler : IRequestHandler<Command, ArticleEnvelope>
        {
            private readonly ConduitContext _context;

            public Handler(ConduitContext context)
            {
                this._context = context;
            }

            public async Task<ArticleEnvelope> Handle(Command message, CancellationToken cancellationToken)
            {
                var article = await _context.Articles
                    .Include(x => x.ArticleTags)
                    .Where(x => x.Slug == message.Slug)
                    .FirstOrDefaultAsync(cancellationToken);

                if (article == null)
                {
                    throw new RestException(HttpStatusCode.NotFound, new { Article = Constants.NOT_FOUND });
                }

                article.Description = message.Article.Description ?? article.Description;
                article.Body = message.Article.Body ?? article.Body;
                article.Title = message.Article.Title ?? article.Title;
                article.Slug = article.Title.GenerateSlug();

                var articleTagList = message.Article.TagList ?? Enumerable.Empty<string>();

                var tagToCreate = await GetTagsToCreate(articleTagList);

                await _context.Tags.AddRangeAsync(tagToCreate, cancellationToken);

                await _context.SaveChangesAsync();

                var articleTagsToCreate = GetArticleTagsToCreate(article, articleTagList);
                var articleTagsToDelete = GetArticleTagsToDelete(article, articleTagList);

                if (_context.ChangeTracker.Entries().First(x => x.Entity == article).State == EntityState.Modified
                    || articleTagsToCreate.Count() > 0 || articleTagsToDelete.Count() > 0)
                {
                    article.UpdatedAt = DateTime.UtcNow;
                }

                await _context.ArticleTags.AddRangeAsync(articleTagsToCreate, cancellationToken);

                _context.ArticleTags.RemoveRange(articleTagsToDelete);

                await _context.SaveChangesAsync();

                return new ArticleEnvelope(await _context.Articles.GetAllData()
                    .Where(x => x.Slug == article.Slug)
                    .FirstOrDefaultAsync(cancellationToken));
            }

            private async Task<List<Tag>> GetTagsToCreate(IEnumerable<string> articleTagList)
            {
                var tagToCreate = new List<Tag>();

                foreach (var tag in articleTagList)
                {
                    var t = await _context.Tags.FindAsync(tag);
                    if (t == null)
                    {
                        t = new Tag
                        {
                            TagId = tag
                        };
                        tagToCreate.Add(t);
                    }
                }

                return tagToCreate;
            }

            static List<ArticleTag> GetArticleTagsToCreate(Article article, IEnumerable<string> articleTagList)
            {
                var articleTagToCreate = new List<ArticleTag>();

                foreach (var tag in articleTagList)
                {
                    var at = article.ArticleTags.FirstOrDefault(t => t.TagId == tag);
                    if (at == null)
                    {
                        at = new ArticleTag
                        {
                            Article = article,
                            ArticleId = article.ArticleId,
                            TagId = tag,
                            Tag = new Tag { TagId = tag }
                        };
                        articleTagToCreate.Add(at);
                    }
                }

                return articleTagToCreate;
            }

            static List<ArticleTag> GetArticleTagsToDelete(Article article, IEnumerable<string> articleTagList)
            {
                var articleagsToDelete = new List<ArticleTag>();

                foreach (var tag in article.ArticleTags)
                {
                    var at = articleTagList.FirstOrDefault(t => t == tag.TagId);
                    if (at == null)
                    {
                        articleagsToDelete.Add(tag);
                    }
                }

                return articleagsToDelete;
            }
        }
    }
}