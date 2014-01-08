namespace Bookstore.SimpleImporter
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Transactions;
    using System.Xml;

    using Bookstore.Data;

    public static class SimpleImporter
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
                
                try
                {
                    xmlDoc.Load("../../simple-books.xml");
                }
                catch (XmlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
                string xPathQuery = "/catalog/book";

                XmlNodeList booksList = xmlDoc.SelectNodes(xPathQuery);
                foreach (XmlNode bookNode in booksList)
                {
                    string title = bookNode.GetChildText("title");
                    if (title == null)
                    {
                        throw new Exception("The book title is missing!!!");
                    }

                    string authorName = bookNode.GetChildText("author");

                    if (authorName == null)
                    {
                        throw new Exception("The book author is missing!!!");
                    }

                    string isbn = bookNode.GetChildText("isbn");

                    string price = bookNode.GetChildText("price");

                    string website = bookNode.GetChildText("web-site");

                    //Console.WriteLine("{0} {1} {2} {3} {4}", title, authorName, isbn, price, website);
                    BookstoreDAL.AddSimpleBook(title, authorName, isbn, price, website);
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