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
                var query = new SqlQuery<Post>().Include(x => x.Comments);
                var result = context.Repository<Post>(query);

                foreach (var post in result)
                {
                    Console.WriteLine("{0} {1} {2} {3}", post.Id, post.Text, post.CreatedDate, post.Published);
                }
            }

            Console.ReadKey();
        }
    }

    class BlogContext : DbContext
    {
        public BlogContext() : base("Default")
        {
            //Configure<Post>(x => x.HasPrimaryKey(e => e.Id));
            //Configure<Post>(x => x.HasMany(e => e.Comments).HasForeignKey(c => c.PostId));
        }
    }

    class Post : IMappyEntity
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool Published { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }

    class Comment : IMappyEntity
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool Published { get; set; }
        public int PostId { get; set; }
    }
}
