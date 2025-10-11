using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 
using Microsoft.AspNetCore.Http; 

namespace UC_Web_Assessment.Models 
{
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

        // Like system
        public int LikeCount { get; set; } = 0;

        // Navigation for likes (one-to-many)
        public ICollection<ImageLike> Likes { get; set; } = new List<ImageLike>();

        [NotMapped]
        public IFormFile ImageFile { get; set; }  
    }

    // Model for tracking likes
    public class ImageLike
    {
        public int Id { get; set; }
        public int AIImageId { get; set; }
        public string UserId { get; set; }
        public DateTime LikedDate { get; set; } = DateTime.Now;

        // Foreign key relationship
        public AIImage AIImage { get; set; }
    }
}