namespace Bookstore.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Validation;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    public class BookstoreDAL
    {
        public static void AddSimpleBook(string title, string authorName, string isbn, string price, string website)
        {
            BookstoreEntities context = new BookstoreEntities();
            Book newBook = new Book();
            newBook.Title = title;

            Author existingAuthor = CreateOrLoadAuthor(context, authorName);
            newBook.Authors.Add(existingAuthor);

            newBook.ISBN = isbn;

            if (price != null)
            {
                newBook.Price = decimal.Parse(price);
            }

            newBook.OfficialWebsite = website;
            
            context.Books.Add(newBook);
            context.SaveChanges();
        }

        private static Author CreateOrLoadAuthor(BookstoreEntities context, string authorName)
        {
            Author existingAuthor =
                (from a in context.Authors
                 where a.Name.ToLower() == authorName.ToLower()
                 select a).FirstOrDefault();
            if (existingAuthor != null)
            {
                return existingAuthor;
            }

            Author newAuthor = new Author();
            newAuthor.Name = authorName;
            context.Authors.Add(newAuthor);
            context.SaveChanges();

            return newAuthor;
        }

        public static void AddComplexBook(string title, List<string> authors, List<Tuple<string, string, string>> reviews, string isbn, string price, string website)
        {
            BookstoreEntities context = new BookstoreEntities();
            Book newBook = new Book();
            newBook.Title = title;

            if (authors.Count > 0)
            {
                foreach (var authorName in authors)
                {
                    Author author = CreateOrLoadAuthor(context, authorName);
                    newBook.Authors.Add(author);
                }
            }

            if (reviews.Count > 0)
            {
                foreach (var review in reviews)
                {
                    Review newReview = new Review();
                    newReview.ReviewContent = review.Item1;
                    
                    if (review.Item2 != null)
                    {
                        newReview.Author = CreateOrLoadAuthor(context, review.Item2);
                    }

                    newReview.DateOfCreation = DateTime.Parse(review.Item3);
                    newBook.Reviews.Add(newReview);
                }
            }
            
            if (isbn != null)
            {
                newBook.ISBN = isbn;
            }

            if (price != null)
            {
                newBook.Price = decimal.Parse(price);
            }

            if (website != null)
            {
                newBook.OfficialWebsite = website;   
            }

            context.Books.Add(newBook);
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Debug.WriteLine("Property: {0} Error: {1}",
                                   validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }
        }

        public static IList<Book> FindBooksByTitleAuthorAndIsbn(string title, string authorName, string isbn)
        {
            BookstoreEntities context = new BookstoreEntities();
            var booksQuery =
                from b in context.Books.Include("Reviews").Include("Authors")
                select b;

            if (authorName != null)
            {
                booksQuery = booksQuery.Where(
                    b => b.Authors.All(t => t.Name.ToLower() == authorName.ToLower()));
            }

            if (title != null)
            {
                booksQuery =
                    from b in context.Books
                    where b.Title.ToLower() == title.ToLower()
                    select b;
            }

            if (isbn != null)
            {
                booksQuery = booksQuery.Where(
                    b => b.ISBN == isbn);
            }

            booksQuery = booksQuery.OrderBy(b => b.Title);

            return booksQuery.ToList();
        }

        public static IList<Review> SearchReviews(string startDate, string endDate)
        {
            var fromDate = DateTime.Parse(startDate);
            var toDate = DateTime.Parse(endDate);
            BookstoreEntities context = new BookstoreEntities();
            var reviewsQuery =
                from r in context.Reviews
                    .Where(x => x.DateOfCreation >= fromDate && x.DateOfCreation <= toDate)
                select r;

            reviewsQuery = reviewsQuery.OrderBy(x => x.DateOfCreation).ThenBy(x => x.ReviewContent);

            return reviewsQuery.ToList();
        }

        public static IList<Review> SearchReviews(string authorName)
        {
            BookstoreEntities context = new BookstoreEntities();
            var reviewsQuery =
                from r in context.Reviews
                    .Where(x => x.Author.Name == authorName)
                select r;

            reviewsQuery = reviewsQuery.OrderBy(x => x.DateOfCreation).ThenBy(x => x.ReviewContent);

            return reviewsQuery.ToList();
        }
    }
}