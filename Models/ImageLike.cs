using System;
using System.ComponentModel.DataAnnotations;

namespace UC_Web_Assessment.Models
{
    public class ImageLike
    {
        public int Id { get; set; }
        
        public int AIImageId { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        public DateTime LikedDate { get; set; } = DateTime.Now;

        // Foreign key relationship
        public AIImage AIImage { get; set; }
    }
}