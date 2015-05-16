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
                var query = new SqlQuery<Post>().Include(x => x.Comments).Include(x => x.User);
                var result = context.Repository<Post>(query);

                foreach (var post in result)
                {
                    Console.WriteLine("{0} {1} {2} {3} {4}", post.Id, post.Text, post.CreatedDate, post.Published, post.UserId);
                    if (post.Comments != null)
                    {
                        foreach (var comment in post.Comments)
                        {
                            Console.WriteLine("     {0} {1} {2} {3} {4} {5}", comment.Id, comment.Text, comment.CreatedDate, comment.Published, comment.PostId, comment.UserId);
                        }
                    }

                    Console.WriteLine();
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

    class User : IMappyEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    class Post : IMappyEntity
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool Published { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }

    class Comment : IMappyEntity
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool Published { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
    }
}
