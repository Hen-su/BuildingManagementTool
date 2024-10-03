namespace BuildingManagementTool.Models
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model, HttpContext httpContext);
    }
}
