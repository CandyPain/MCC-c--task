using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");

        var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

        var context = new MyDbContext(loggerFactory);
        context.Database.EnsureCreated();
        InitializeData(context);

        Console.WriteLine("All posts:");
        var data = context.BlogPosts.Select(x => x.Title).ToList();
        Console.WriteLine(JsonSerializer.Serialize(data));
            
            
        Console.WriteLine("How many comments each user left:");
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Ivan: 4
        // Petr: 2
        // Elena: 3
        var CountCommentsPerUser = context.BlogComments.GroupBy(a=>a.UserName).Select(c=> new {userName = c.Key, count = c.Count()}).ToList();
        Console.WriteLine(JsonSerializer.Serialize(CountCommentsPerUser));

        Console.WriteLine("Posts ordered by date of last comment. Result should include text of last comment:");
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Post2: '2020-03-06', '4'
        // Post1: '2020-03-05', '8'
        // Post3: '2020-02-14', '9'
        var OrderedPosts = context.BlogPosts
            .Select(p => new
            {
                LastComment = p.Comments.OrderByDescending(c => c.CreatedDate).FirstOrDefault()
            })
            .OrderByDescending(p => p.LastComment.CreatedDate)
            .Select(p => new
            {
                LastCommentDate = p.LastComment.CreatedDate,
                LastCommentText = p.LastComment.Text
            })
            .ToList();
        Console.WriteLine(JsonSerializer.Serialize(OrderedPosts)); //ПРИМЕЧАНИЕ. ДВА SELECT СДЕЛАНО С ЦЕЛЬЮ ОПТИМИЗАЦИИ, ДЛЯ ТОГО ЧТОБЫ НЕ ВЫСЧИТЫВАТЬ LASTCOMMENT ПОВТОРНО


        Console.WriteLine("How many last comments each user left:");
        // 'last comment' is the latest Comment in each Post
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Ivan: 2
        // Petr: 1
        var LastCommentsPerUser = context.BlogPosts
        .Select(p => p.Comments.OrderByDescending(c => c.CreatedDate).First())
        .GroupBy(c => c.UserName)
        .Select(a => new { UserName = a.Key, Count = a.Count() }).ToList();
        Console.WriteLine(JsonSerializer.Serialize(LastCommentsPerUser));


        // Console.WriteLine(
        //     JsonSerializer.Serialize(BlogService.NumberOfCommentsPerUser(context)));
        // Console.WriteLine(
        //     JsonSerializer.Serialize(BlogService.PostsOrderedByLastCommentDate(context)));
        // Console.WriteLine(
        //     JsonSerializer.Serialize(BlogService.NumberOfLastCommentsLeftByUser(context)));
    }

    private static void InitializeData(MyDbContext context)
    {
        context.BlogPosts.Add(new BlogPost("Post1")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("1", new DateTime(2020, 3, 2), "Petr"),
                new BlogComment("2", new DateTime(2020, 3, 4), "Elena"),
                new BlogComment("8", new DateTime(2020, 3, 5), "Ivan"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post2")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("3", new DateTime(2020, 3, 5), "Elena"),
                new BlogComment("4", new DateTime(2020, 3, 6), "Ivan"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post3")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("5", new DateTime(2020, 2, 7), "Ivan"),
                new BlogComment("6", new DateTime(2020, 2, 9), "Elena"),
                new BlogComment("7", new DateTime(2020, 2, 10), "Ivan"),
                new BlogComment("9", new DateTime(2020, 2, 14), "Petr"),
            }
        });
        context.SaveChanges();
    }
}