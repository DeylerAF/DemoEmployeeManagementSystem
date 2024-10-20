﻿// Ignore Spelling: Fullname

using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.DTOs
{
    public class Register : AccountBase
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string? Fullname { get; set; }
        [DataType(DataType.Password)]
        [Required]
        public string? ConfirmPassword { get; set; }
    }
}
