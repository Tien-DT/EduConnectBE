using EduConnect.Application.DTOs.Request;

namespace EduConnect.Application.Services.Interface
{
    public interface IGeminiFunctionService
    {
        Task<string> ExecuteFunctionAsync(string functionName, Dictionary<string, object> args);
        FunctionDeclaration[] GetAvailableFunctions();
        bool IsFunctionSupported(string functionName);
    }
}
