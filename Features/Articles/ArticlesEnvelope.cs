using System.Collections.Generic;
using conduit_api.Domain;

namespace conduit_api.Features.Articles
{
    public class ArticlesEnvelope
    {
        public List<Article> Articles { get; set; }
        public int ArticleCount { get; set; }
    }
}