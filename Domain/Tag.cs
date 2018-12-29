using System.Collections.Generic;

namespace conduit_api.Domain
{
    public class Tag
    {
        public int TagId { get; set; }
        public List<ArticleTag> ArticleTags { get; set; }
    }
}