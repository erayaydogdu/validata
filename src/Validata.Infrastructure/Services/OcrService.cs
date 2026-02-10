using System.Text.Json;
using Validata.Core.Interfaces;

namespace Validata.Infrastructure.Services;

public class OcrService
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string _apiKey;

    public OcrService(HttpClient httpClient, string endpoint, string apiKey)
    {
        _httpClient = httpClient;
        _endpoint = endpoint;
        _apiKey = apiKey;
    }

    public async Task<OcrResult> AnalyzeDocumentAsync(Stream documentStream, string mimeType)
    {
        using var content = new MultipartFormDataContent();
        var documentContent = new ByteArrayContent(await ReadFullyAsync(documentStream));
        documentContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
        content.Add(documentContent, "document", "document");

        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

        var response = await _httpClient.PostAsync($"{_endpoint}/vision/v3.2/read/analyze", content);
        response.EnsureSuccessStatusCode();

        var operationLocation = response.Headers.GetValues("Operation-Location").First();
        var operationId = operationLocation.Split('/').Last();

        var result = await PollForResultAsync(operationId);

        return result;
    }

    private async Task<OcrResult> PollForResultAsync(string operationId, int maxRetries = 10)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            await Task.Delay(1000);
            var response = await _httpClient.GetAsync($"{_endpoint}/vision/v3.2/read/analyzeResults/{operationId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return ParseResult(json);
            }
        }

        throw new TimeoutException("OCR processing timed out");
    }

    private OcrResult ParseResult(string json)
    {
        var docAnalysis = JsonSerializer.Deserialize<JsonElement>(json);
        var result = new OcrResult();

        if (docAnalysis.TryGetProperty("analyzeResult", out var analyzeResult))
        {
            if (analyzeResult.TryGetProperty("documents", out var documents) &&
                documents.EnumerateArray().FirstOrDefault() is { } document)
            {
                if (document.TryGetProperty("fields", out var fields))
                {
                    result.DocumentType = GetFieldString(fields, "docType");
                    result.FirstName = GetFieldString(fields, "firstName");
                    result.LastName = GetFieldString(fields, "lastName");
                    result.DateOfBirth = GetFieldDate(fields, "dateOfBirth");
                    result.DocumentNumber = GetFieldString(fields, "documentNumber");
                    result.ExpiryDate = GetFieldDate(fields, "expiryDate");
                    result.Nationality = GetFieldString(fields, "nationality");
                }
            }
        }

        return result;
    }

    private string GetFieldString(JsonElement fields, string fieldName)
    {
        if (fields.TryGetProperty(fieldName, out var field) &&
            field.TryGetProperty("valueString", out var value))
        {
            return value.GetString() ?? string.Empty;
        }
        return string.Empty;
    }

    private DateTime? GetFieldDate(JsonElement fields, string fieldName)
    {
        if (fields.TryGetProperty(fieldName, out var field) &&
            field.TryGetProperty("valueDate", out var value))
        {
            if (DateTime.TryParse(value.GetString(), out var date))
                return date;
        }
        return null;
    }

    private async Task<byte[]> ReadFullyAsync(Stream input)
    {
        using var ms = new MemoryStream();
        await input.CopyToAsync(ms);
        return ms.ToArray();
    }
}

public class OcrResult
{
    public string DocumentType { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public string Nationality { get; set; } = string.Empty;
}
