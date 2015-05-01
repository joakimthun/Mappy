using Mappy;

namespace Application
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new BlogContext())
            {
                var result = context.ExectuteQuery<Comment>("select * from Post");
            }
        }
    }

    class BlogContext : DbContext
    {
        public BlogContext() : base("Default")
        {
        }
    }

    class Post
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }

    class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int PostId { get; set; }
    }
}
