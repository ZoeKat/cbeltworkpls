using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cbeltworkpls.Models
{
    public class users {

        [Key]
        public int idusers {get;set;}

        [Required]
        [MinLength(2)]
        public string first_name {get;set;}

        [Required]
        [MinLength(2)]
        public string last_name {get;set;}

        [Required]
        [EmailAddress]
        public string email {get;set;}

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string password {get;set;}

        public List<participants> participants {get;set;}
        public users() {
            participants = new List<participants>();
        }
    }

    public class activities {

        [Key]
        public int idactivities {get;set;}

        [Required]
        [MinLength(2)]
        public string actname {get;set;}

        [Required]
        public DateTime date {get;set;}

        [Required]
        public int durationint {get;set;}

        [Required]
        public string durationtype {get;set;}

        [Required]
        public string coordname {get;set;}

        public int coordid {get;set;}

        [Required]
        [MinLength(10)]
        public string description {get;set;}

        public List<participants> participants {get;set;}
        public activities() {
            participants = new List<participants>();
        }


    }

    public class participants {

        [Key]
        public int idparticipants {get;set;}

        public int idusers {get;set;}
        public users user {get;set;}

        public int idactivities {get;set;}
        public activities activity {get;set;}
    }
}