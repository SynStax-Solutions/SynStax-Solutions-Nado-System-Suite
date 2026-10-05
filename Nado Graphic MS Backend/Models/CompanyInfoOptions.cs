namespace Nado_Graphics_MS_Backend.Models
{

    //This class holds the company's own details, this is used when generating quote PDFs.
    //It is bound from the "CompanyInfo" section of appsettings.json
    public class CompanyInfoOptions
    {
        //The company's name
        public string Name { get; set; } = string.Empty;

        // First line of company address
        public string AddressLine1 { get; set; } = string.Empty;

        //Second line of company address
        public string AddressLine2 { get; set; } = string.Empty;

        //Country that the company resides in
        public string Country { get; set; } = string.Empty;

        //The company's contact phone number
        public string Phone { get; set; } = string.Empty;
        
        //The name of the bank used for the company's banking details
        public string BankName { get; set; } = string.Empty;

        //The company's bank account number
        public string AccountNumber { get; set; } = string.Empty;

        //The branch code for the company's bank account
        public string BranchCode { get; set; } = string.Empty;

        //The company's registered office address
        public string RegisteredOffice { get; set; } = string.Empty;
    }
}
//---------------------------------------------------END OF FILE-------------------------------------------