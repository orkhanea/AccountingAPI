namespace Accounting.Model.DTOs
{
    public class TransactionDTO
    {
        public string Note { get; set; }

        public DateTime SendDate { get; set; }

        public DateTime RetiredDate { get; set; }

        public DateTime CraeatedDate { get; set; }

        public double NETAmount { get; set; }

        public double TAXAmount { get; set; }

        public string SenderCompany { get; set; }

        public string RecieverCompany { get; set; }
    }
}
