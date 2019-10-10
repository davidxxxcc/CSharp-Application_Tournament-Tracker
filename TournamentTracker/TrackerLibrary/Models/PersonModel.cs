using System;
namespace TrackerLibrary.Models
{
    public class PersonModel
    {
        /// <summary>
        /// The unique identifier for the person.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The first name of the person.
        /// </summary>
        public String FirstName { get; set; }

        /// <summary>
        /// The last name of the person.
        /// </summary>
        public String LastName { get; set; }

        /// <summary>
        /// The primary email address of the person.
        /// </summary>
        public String EmailAddress { get; set; }

        /// <summary>
        /// The primary cell phone of the person.
        /// </summary>
        public String CellphoneNumber { get; set; }


        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }


    }
}
