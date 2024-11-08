using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace task_management.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        [Required]
        public string Priority { get; set; } = "Normal"; 

        [Required]
        public string Status { get; set; } = "Pending"; 

        [ForeignKey("AssignedToUser")]
        [Required]
        public int AssignedToUserId { get; set; }

        public User? AssignedToUser { get; set; } 
    }
}
