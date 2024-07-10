using CsvHelper.Configuration.Attributes;

namespace STSL.SmartLocker.Utils.MSIProcessor.DTO;

public class MSIRecordDTO
{
    [Name("MSI Number")]
    public int MSINumber { get; set; }

    [Name("Object ID")]
    public int ObjectID { get; set; }

    [Name("ESR Number")]
    public string? ESRNumber { get; set; }

    [Name("First Name")]
    public required string FirstName { get; set; }

    [Name("Last Name")]
    public required string LastName { get; set; }

    [Name("Job Title")]
    public string? JobTitle { get; set; }

    [Name("Staff Email")]
    public string? StaffEmail { get; set; }

    [Name("Mobile Number")]
    public string? MobileNumber { get; set; }

    [Name("Building")]
    public string? Building { get; set; }

    [Name("Department")]
    public string? Department { get; set; }

    [Name("Type of Staff")]
    public string? TypeOfStaff { get; set; }

    [Name("Start Date")]
    public DateTime? StartDate {  get; set; }

    [Name("Termination Date")]
    public DateTime? TerminationDate { get; set; }
}
