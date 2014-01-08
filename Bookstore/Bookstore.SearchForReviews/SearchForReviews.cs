using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.SearchForReviews
{
    using Bookstore.Data;
    using System.Xml;

    using Bookstore.Logger.Model;

    public static class SearchForReviews
    {
        public static void Main(string[] args)
        {
            string fileName = "../../search-results.xml";
            using (XmlTextWriter writer =
                new XmlTextWriter(fileName, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.IndentChar = '\t';
                writer.Indentation = 1;

                writer.WriteStartDocument();
                writer.WriteStartElement("result-set");

                ProcessSearchQueries(writer);

                writer.WriteEndDocument();
            }
        }

        public static void ProcessSearchQueries(XmlTextWriter writer)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("../../reviews-queries.xml");
            foreach (XmlNode query in xmlDoc.SelectNodes("/review-queries/query"))
            {
                var logContext = new Logger.Data.LoggerEntities();

                IList<Review> reviews = new List<Review>();
                string searchType = query.Attributes["type"].Value;
                
                if (searchType == "by-period")
                {
                    string startDate = query.GetChildText("start-date");
                    string endDate = query.GetChildText("end-date");

                    reviews = BookstoreDAL.SearchReviews(startDate, endDate);
                }
                else if (searchType == "by-author")
                {
                    string authorName = query.GetChildText("author-name");
                    reviews = BookstoreDAL.SearchReviews(authorName);
                }

                WriteReviews(writer, reviews);
            }
        }

        private static void WriteReviews(XmlTextWriter writer, IList<Review> reviews)
        {
            //writer.WriteStartElement("result-set");
            foreach (var review in reviews)
            {
                writer.WriteStartElement("review");

                if (review.DateOfCreation != null)
                {
                    writer.WriteElementString("date", review.DateOfCreation.Value.ToString("dd-MMM-yyyy"));
                }

                if (review.ReviewContent != null)
                {
                    writer.WriteElementString("content", review.ReviewContent);
                }

                if (review.Book != null)
                {
                    writer.WriteStartElement("book");
                    writer.WriteElementString("title", review.Book.Title);

                    if (review.Book.Authors.Count > 0)
                    {
                        string authors = string.Join(", ", review.Book.Authors.Select(a => a.Name).OrderBy(a => a));
                        writer.WriteElementString("authors", authors);
                    }

                    if (review.Book.ISBN != null)
                    {
                        writer.WriteElementString("isbn", review.Book.ISBN);
                    }

                    writer.WriteElementString("url", review.Book.OfficialWebsite);
                    writer.WriteEndElement();
                }
               
                writer.WriteEndElement();
            }

            //writer.WriteEndElement();
        }

        private static string GetChildText(this XmlNode node, string xpath)
        {
            XmlNode childNode = node.SelectSingleNode(xpath);
            if (childNode == null)
            {
                return null;
            }

            return childNode.InnerText.Trim();
        }
    }
}