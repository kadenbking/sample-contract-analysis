namespace cf.Models;

public class SampleAnalysisResults
{
    public SampleAnalysisResults()
    {
        SampleKeyValuePairsList = new List<SampleKeyValuePairs>();
        Paragraphs = new List<string>();
    }

    public IList<SampleKeyValuePairs> SampleKeyValuePairsList { get; set; }

    public IList<string> Paragraphs { get; set; }
}