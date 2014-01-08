namespace Bookstore.Logger.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Log
    {
        [Key]
        public int Id { get; set; }

        public DateTime LogDate { get; set; }

        public string XmlQuery { get; set; }
    }
}
