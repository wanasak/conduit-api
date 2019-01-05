using conduit_api.Domain;

namespace conduit_api.Features.Comments
{
    public class CommentEnvelope
    {
        public CommentEnvelope(Comment comment)
        {
            Comment = comment;
        }

        public Comment Comment { get; }
    }
}