using STSL.SmartLocker.Utils.Kiosk.Models;
using System;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Exceptions;

public class ApiProblemException : ApplicationException
{
    protected string _type { get; set; }
    protected string _title { get; set; }
    protected int? _status { get; set; }
    protected string _detail { get; set; }
    protected string _instance { get; set; }
    protected string _message { get; set; }

    public ApiProblemException(ProblemDetails problem)
        : base(string.IsNullOrWhiteSpace(problem.Title) ? "Unknown api problem" : problem.Title)
    {
        _type = problem.Type;
        _title = problem.Title;
        _status = problem.Status;
        _detail = problem.Detail;
        _instance = problem.Instance;
    }
}
