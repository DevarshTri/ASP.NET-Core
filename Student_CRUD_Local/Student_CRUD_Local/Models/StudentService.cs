namespace Student_CRUD_Local.Models
{
    public class StudentService
    {
        public List<Student> Students = new List<Student>();

        public List<Student> GetStudents()
        {
            return Students;
        }
    }
}
