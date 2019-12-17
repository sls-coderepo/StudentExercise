using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercises.API.Models
{
    public class Cohort 
    {
        public int Id { get; set; }
        [Required]
        [StringLength(11, MinimumLength = 5, ErrorMessage = "Cohort Name should be between 5 - 11 charactors")]
        //[RegularExpression("^[day|evening|Day|Evening][0-9][0-9]$", ErrorMessage = "Cohort name should be in the format of [Day|Evening] [number]")]
        public string Name { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Instructor> Instructors { get; set; } = new List<Instructor>();
    }
}
