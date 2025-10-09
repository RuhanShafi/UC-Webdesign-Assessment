// Models/AIImage.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace UC_Web_Assessment.Models 
{
    // Model structure based on PDF (Page 22) and requirements (CreatorId)
    public class AIImage
    {
        public int Id { get; set; } // Primary Key

        [Required]
        [StringLength(100)]
        public required string Title { get; set; }

        public required string Description { get; set; }

        [Required]
        public required string ImagePath { get; set; } // Path to the uploaded image

        public DateTime CreatedDate { get; set; } = DateTime.Now; 

        // Required for Member-level authorization (Who created this?)
        public required string CreatorId { get; set; } 
    }
}