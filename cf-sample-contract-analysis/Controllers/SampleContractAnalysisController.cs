using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using cf.Models;
using Microsoft.AspNetCore.Mvc;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace cf;

[ApiController]
[Route("/")]
public class SampleContractAnalysisController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public SampleContractAnalysisController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("AnalyzeDocument", Name = "AnalyzeDocument")]
    public async Task<SampleAnalysisResults> AnalyzeDocument([FromBody] AnalysisRequest analysisRequest)
    {
        // Model ID Options:
        //      "prebuilt-document" => prebuilt general document model
        //      "prebuilt-contract" => prebuilt contract model
        
        var result = await GetResult(analysisRequest.ModelId, analysisRequest.DocumentId);
        var cleanedResult = CleanResult(result);
        await SaveResultsToJsonFile(cleanedResult, "SampleAnalysisResults2.json");
        
        return cleanedResult;
    }

    private DocumentAnalysisClient GetClient()
    {
        var serviceEndpoint = _configuration.GetValue<string>("CognitiveService:Endpoint");
        var serviceCredential = _configuration.GetValue<string>("CognitiveService:Key");
        var serviceClient = new DocumentAnalysisClient(new Uri(serviceEndpoint), new AzureKeyCredential(serviceCredential));

        return serviceClient;
    }

    private string GetFilePath(int fileId)
    {
        var filePath = fileId switch
        {
            1 => "/Users/kaden/dev/cf/sample-contract-analysis/cf-sample-contract-analysis/files/Simple Contract.pdf",
            2 =>
                "/Users/kaden/dev/cf/sample-contract-analysis/cf-sample-contract-analysis/files/Sample_Utah_Real_Estate_Contract.pdf",
            3 =>
                "/Users/kaden/dev/cf/sample-contract-analysis/cf-sample-contract-analysis/files/AmendmentToContract.pdf",
            4 =>
                "/Users/kaden/dev/cf/sample-contract-analysis/cf-sample-contract-analysis/files/FarmAndRanchContract.pdf",
            _ => string.Empty
        };

        return filePath;
    }

    private async Task<AnalyzeResult> GetResult(string modelId, int fileId)
    {
        var client = GetClient();
        var filePath = GetFilePath(fileId);

        await using var stream = new FileStream(filePath, FileMode.Open);
        AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, modelId, stream);
        return operation.Value;
    }

    private SampleAnalysisResults CleanResult(AnalyzeResult result)
    {
        var analysisResults = new SampleAnalysisResults();

        foreach (DocumentKeyValuePair kvp in result.KeyValuePairs)
        {
            if (kvp.Value == null)
            {
                var pair = new SampleKeyValuePairs()
                {
                    Key = "Found key with no value",
                    Value = kvp.Key.Content
                };
        
                analysisResults.SampleKeyValuePairsList.Add(pair);
            }
            else
            {
                var pair = new SampleKeyValuePairs()
                {
                    Key = "Found key-value pair",
                    Value = $"{kvp.Key.Content} : {kvp.Value.Content}"
                };
        
                analysisResults.SampleKeyValuePairsList.Add(pair);
            }
        }
        
        foreach (DocumentParagraph kvp in result.Paragraphs)
        {
            analysisResults.Paragraphs.Add(kvp.Content);
        }

        return analysisResults;
    }

    private async Task SaveResultsToJsonFile(SampleAnalysisResults results, string fileName)
    {
        await using FileStream createStream = System.IO.File.Create(fileName);
        await JsonSerializer.SerializeAsync(createStream, results);
        await createStream.DisposeAsync();
    }
}