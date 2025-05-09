namespace MLService.Models.Prediction;

public class LogisticPredictionRequest
{
    public string ModelName { get; set; }
    public LogisticInput Data { get; set; }
}