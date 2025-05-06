using Lib.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lib.Persistence;

public static class DataSeeder
{
    public static async Task SeedDataAsync(AppDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Categories if none exist
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
                {
                    new Category { Id = Guid.NewGuid(), Name = "Fiction" },
                    new Category { Id = Guid.NewGuid(), Name = "Non-Fiction" },
                    new Category { Id = Guid.NewGuid(), Name = "Science" },
                    new Category { Id = Guid.NewGuid(), Name = "History" },
                    new Category { Id = Guid.NewGuid(), Name = "Fantasy" }
                };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // Seed Books if none exist
        if (!await context.Books.AnyAsync())
        {
            var categories = await context.Categories.ToListAsync();
            var books = new List<Book>
                {
                    // Fiction
                    new Book
                    {
                        Id = Guid.NewGuid(),
                        Title = "Pride and Prejudice",
                        Author = "Jane Austen",
                        Description = "A romantic novel following Elizabeth Bennet's evolving relationship with Mr. Darcy.",
                        ISBN = "978-0141439518",
                        PublishedDate = new DateTime(1813, 1, 28),
                        Quantity = 3,
                        Available = 3,
                        CategoryId = categories.First(c => c.Name == "Fiction").Id
                    },
                    new Book
                    {
                        Id = Guid.NewGuid(),
                        Title = "The Great Gatsby",
                        Author = "F. Scott Fitzgerald",
                        Description = "A story of wealth, love, and the American Dream in the Roaring Twenties.",
                        ISBN = "978-0743273565",
                        PublishedDate = new DateTime(1925, 4, 10),
                        Quantity = 2,
                        Available = 2,
                        CategoryId = categories.First(c => c.Name == "Fiction").Id
                    },
                    // Non-Fiction
                    new Book
                    {
                        Id = Guid.NewGuid(),
                        Title = "Sapiens: A Brief History of Humankind",
                        Author = "Yuval Noah Harari",
                        Description = "Explores the history of Homo sapiens from the Stone Age to the modern era.",
                        ISBN = "978-0062316097",
                        PublishedDate = new DateTime(2014, 9, 9),
                        Quantity = 4,
                        Available = 4,
                        CategoryId = categories.First(c => c.Name == "Non-Fiction").Id
                    },
                    new Book
                    {
                        Id = Guid.NewGuid(),
                        Title = "Educated",
                        Author = "Tara Westover",
                        Description = "A memoir about a woman’s quest for knowledge raised in a strict, isolated family.",
                        ISBN = "978-0399590504",
                        PublishedDate = new DateTime(2018, 2, 20),
                        Quantity = 3,
                        Available = 3,
                        CategoryId = categories.First(c => c.Name == "Non-Fiction").Id
                    },
                    // Science
                    new Book
                    {
                        Id = Guid.NewGuid(),
                        Title = "A Brief History of Time",
                        Author = "Stephen Hawking",
                        Description = "Explains complex concepts of cosmology to the general reader.",
                        ISBN = "978-0553380163",
                        PublishedDate = new DateTime(1988, 3, 1),
                        Quantity = 2,
                        Available = 2,
                        CategoryId = categories.First(c => c.Name == "Science").Id
                    },
                    new Book
                    {
                        Id = Guid.NewGuid(),
                        Title = "The Selfish Gene",
                        Author = "Richard Dawkins",
                        Description = "Introduces the gene-centered view of evolution.",
                        ISBN = "978-0199291151",
                        PublishedDate = new DateTime(1976, 11, 1),
                        Quantity = 3,
                        Available = 3,
                        CategoryId = categories.First(c => c.Name == "Science").Id
                    },
                    // History
                    new Book
                    {
                        Id = Guid.NewGuid(),
                        Title = "Guns, Germs, and Steel",
                        Author = "Jared Diamond",
                        Description = "Explores how geography and environment shaped human history.",
                        ISBN = "978-0393317558",
                        PublishedDate = new DateTime(1997, 3, 1),
                        Quantity = 4,
                        Available = 4,
                        CategoryId = categories.First(c => c.Name == "History").Id
                    },
                    new Book
                    {
                        Id = Guid.NewGuid(),
                        Title = "The Rise and Fall of the Roman Empire",
                        Author = "Edward Gibbon",
                        Description = "A comprehensive history of the Roman Empire’s decline.",
                        ISBN = "978-0140437645",
                        PublishedDate = new DateTime(1776, 2, 17),
                        Quantity = 2,
                        Available = 2,
                        CategoryId = categories.First(c => c.Name == "History").Id
                    },
                    // Fantasy
                    new Book
                    {
                        Id = Guid.NewGuid(),
                        Title = "The Hobbit",
                        Author = "J.R.R. Tolkien",
                        Description = "Bilbo Baggins’ adventure with dwarves and a dragon in Middle-earth.",
                        ISBN = "978-0547928227",
                        PublishedDate = new DateTime(1937, 9, 21),
                        Quantity = 5,
                        Available = 5,
                        CategoryId = categories.First(c => c.Name == "Fantasy").Id
                    },
                    new Book
                    {
                        Id = Guid.NewGuid(),
                        Title = "Harry Potter and the Sorcerer's Stone",
                        Author = "J.K. Rowling",
                        Description = "Harry Potter discovers his magical heritage and attends Hogwarts.",
                        ISBN = "978-0590353427",
                        PublishedDate = new DateTime(1997, 6, 26),
                        Quantity = 4,
                        Available = 4,
                        CategoryId = categories.First(c => c.Name == "Fantasy").Id
                    }
                };

            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();
        }
    }
}
