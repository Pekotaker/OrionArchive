using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrionArchive.Web.Services;

public interface IFileUploadService
{
    Task<string?> SaveImageAsync(IFormFile? file, ModelStateDictionary modelState, string fieldName);
}