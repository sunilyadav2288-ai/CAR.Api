namespace CAR.Api.Services.Interfaces;

public interface ISqlValidationService
{
    bool IsValid(string sql, out string error);
}