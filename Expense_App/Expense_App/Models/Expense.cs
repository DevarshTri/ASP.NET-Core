namespace Expense_App.Models
{
    public class Expense
    {
        public int Expense_Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Expense_type { get; set; }
        public string Expense_Description   { get; set; }
        public string Expense_Amount { get; set; }
    }
}
