using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercises.API.Models
{
    public class Instructor 
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 3, ErrorMessage = "Slack Handle should be 3 - 12 characters")]
        public string SlackHandle { get; set; }
        public int CohortId { get; set; }
        public Cohort Cohort { get; set; }
        public Instructor()
        {
            Cohort = new Cohort();
        }
    }
}
