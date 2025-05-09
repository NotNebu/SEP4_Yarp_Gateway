namespace MLService.Models.Prediction;

public class Rfc_PredictionRequest
{
    public string TypeofModel { get; set; }
    public string NameOfModel { get; set; }
    public Rfc_PredictionData Data { get; set; }
}