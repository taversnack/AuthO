namespace STSL.SmartLocker.Utils.MSIProcessor.DTO;

public class MSIOutputDTO
{
	public string? Problem { get; set; }

	public string? ProblemDetails { get; set; }

	public required string FirstName { get; set; }

	public required string LastName { get; set; }

	public string? Email { get; set; }

	public required string ObjectID { get; set; }

	public string? NewObjectID { get; set; }

	public bool IsTerminated {  get; set; }
	
	public DateTime? TerminationDate { get; set; }
}