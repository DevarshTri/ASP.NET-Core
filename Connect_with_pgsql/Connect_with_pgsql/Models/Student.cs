using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect_with_pgsql.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        [Column("Student_Name",TypeName ="varchar(100)")]
        public string Name { get; set; }
    }
}
