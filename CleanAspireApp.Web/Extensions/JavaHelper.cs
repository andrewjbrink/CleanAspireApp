using Microsoft.JSInterop;

namespace CleanAspireApp.Web.Extensions;

public class JavaHelper
{
    private readonly IJSRuntime _jsRuntime;


    public JavaHelper(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetItemAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public async Task<string?> GetItemAsync(string key)
    {
        try
        {
            var response = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            if (!string.IsNullOrEmpty(response))
            {
                return response;
            }
            return null;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving item from localStorage: {ex.Message}");
            throw;
        }
    }






    public async Task LoadMainModel()
    {
        var module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/main.js");
        await module.InvokeVoidAsync("initializeMapWrapper");
    }

}

