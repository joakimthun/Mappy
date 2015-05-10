using Mappy;
using Mappy.Queries;
using System;
using System.Collections.Generic;

namespace Application
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new BlogContext())
            {
                var result = context.Tables<Post>();
            }
        }
    }

    class BlogContext : DbContext
    {
        public BlogContext() : base("Default")
        {
            Configure<Post>(x => x.HasPrimaryKey(e => e.Id));
            Configure<Post>(x => x.HasMany(e => e.Comments).HasForeignKey(c => c.PostId));
        }
    }

    class Post
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool Published { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }

    class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool Published { get; set; }
        public int PostId { get; set; }
    }
}
