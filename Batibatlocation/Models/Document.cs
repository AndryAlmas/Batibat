using Batibatlocation.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Batibatlocation.Models
{
    public class Document
    {
        [Key]
        [ForeignKey("Client")]
        public int Id { get; set; }
        public byte[] DocumentFile { get; set; }
        public string DocumentType { get; set; }
        public DateTime UploadDate { get; set; }

        public int CategoryId { get; set; }

        [Required]
        [ForeignKey("CategoryId")]
        public virtual CategoryDocument Category { get; set; }

        public virtual Client Client { get; set; }


    }
}