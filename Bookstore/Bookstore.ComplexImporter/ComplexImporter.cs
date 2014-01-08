namespace Bookstore.ComplexImporter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Transactions;
    using System.Xml;

    using Bookstore.Data;

    public static class ComplexImporter
    {
        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            TransactionScope tran = new TransactionScope(
            TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = IsolationLevel.RepeatableRead
                });
            using (tran)
            {

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("../../complex-books.xml");
                string booksQuery = "/catalog/book";

                XmlNodeList booksList = xmlDoc.SelectNodes(booksQuery);
                foreach (XmlNode bookNode in booksList)
                {
                    string title = bookNode.GetChildText("title");
                    if (title == null)
                    {
                        throw new Exception("The book title is missing!!!");
                    }

                    XmlNodeList authorsList = bookNode.SelectNodes("authors/author");

                    var authors = new List<string>();
                    if (authorsList.Count > 0)
                    {
                        foreach (XmlNode authorNode in authorsList)
                        {
                            authors.Add(authorNode.InnerText);
                        }
                    }

                    XmlNodeList reviewsList = bookNode.SelectNodes("reviews/review");

                    var reviews = new List<Tuple<string, string, string>>();

                    if (reviewsList.Count > 0)
                    {
                        foreach (XmlNode reviewNode in reviewsList)
                        {
                            reviews.Add(
                                new Tuple<string, string, string>(
                                    reviewNode.InnerText,
                                    reviewNode.Attributes["author"] != null ? reviewNode.Attributes["author"].Value : null,
                                    reviewNode.Attributes["date"] != null ? reviewNode.Attributes["date"].Value : DateTime.Now.ToString("dd-MMM-yyyy")));
                        }
                    }

                    string isbn = bookNode.GetChildText("isbn");

                    string price = bookNode.GetChildText("price");

                    string website = bookNode.GetChildText("web-site");

                    //Console.WriteLine("{0} \nAuthors: {1} \nReviews: {2} \n{3} \n{4} \n{5}", title, string.Join(", ", authors), string.Join(", ", reviews), isbn, price, website);
                    BookstoreDAL.AddComplexBook(title, authors, reviews, isbn, price, website);
                }

                tran.Complete();
            }
        }

        private static string GetChildText(this XmlNode node, string tagName)
        {
            XmlNode childNode = node.SelectSingleNode(tagName);
            if (childNode == null)
            {
                return null;
            }

            return childNode.InnerText.Trim();
        }
    }
}
