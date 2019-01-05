using conduit_api.Domain;

namespace conduit_api.Features.Articles
{
    public class ArticleEnvelope
    {
        public ArticleEnvelope(Article article)
        {
            Article = article;
        }

        public Article Article { get; }

    }
}