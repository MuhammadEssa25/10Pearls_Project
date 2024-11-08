    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

    namespace task_management.Models
    {
        public class User
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; } = string.Empty;

            [Required]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;

            public string Role { get; set; } = "User";

    }
    }