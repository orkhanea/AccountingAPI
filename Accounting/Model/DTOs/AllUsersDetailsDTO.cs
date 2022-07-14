namespace Accounting.Model.DTOs
{
    public class AllUsersDetailsDTO
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime Bdate { get; set; }
        public DateTime CreatedDAte { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public double NettSalary { get; set; }
        public double GrossSalary { get; set; }
        public string Company { get; set; }
    }
}
