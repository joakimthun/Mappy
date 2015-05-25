using Mappy;
using Mappy.LazyLoading;
using Mappy.Queries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new BlogContext())
            {
                var query = new SqlQuery<Post>()
                    .Include(x => x.User)
                    .Where(x => x.Id == 1 || x.Id == 2 || x.Id == 3 || x.Id == 4);

                var result = context.Repository(query);
            
                foreach (var post in result)
                {
                    Console.WriteLine("Post:");
                    Console.WriteLine("{0} {1} {2} {3} {4}", post.Id, post.Text, post.CreatedDate, post.Published, post.UserId);
            
                    Console.WriteLine("User:");
                    Console.WriteLine("{0} {1} {2}", post.User.Id, post.User.FirstName, post.User.LastName);
                    if (post.Comments != null)
                    {
                        Console.WriteLine("Comments:");
                        foreach (var comment in post.Comments)
                        {
                            Console.WriteLine("     {0} {1} {2} {3} {4} {5}", comment.Id, comment.Text, comment.CreatedDate, comment.Published, comment.PostId, comment.UserId);
                            if (comment.User != null)
                            {
                                Console.WriteLine("     User:");
                                Console.WriteLine("     {0} {1} {2}", comment.User.Id, comment.User.FirstName, comment.User.LastName);
                            }
                        }
                    }
            
                    Console.WriteLine();
                }
            }

            var t = new Test();

            Console.ReadKey();
        }
    }

    public class BlogContext : DbContext
    {
        public BlogContext() : base("Default")
        {
            //Configure<Post>(x => x.HasPrimaryKey(e => e.Id));
            //Configure<Post>(x => x.HasMany(e => e.Comments).HasForeignKey(c => c.PostId));
        }
    }

    public class User : IMappyEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Post : IMappyEntity
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool Published { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }

    public class Comment : IMappyEntity
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool Published { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }

    public class Test
    {
        private ICollection<Comment> _comments;

        public ICollection<Comment> MyProperty
        {
            get
            {
                if (MyProperty != null)
                {
                    return MyProperty;
                }

                var context = new BlogContext();
                MyProperty = context.Repository<Comment>().ToList();

                return MyProperty;
            }

            set
            {
                this._comments = value;
            }
        }
    }
}
