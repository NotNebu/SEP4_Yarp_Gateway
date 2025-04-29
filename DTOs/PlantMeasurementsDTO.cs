using System.Text.Json.Serialization;

namespace ApiGateway.DTOs
{
    public class PlantMeasurementsDTO
    {
        public long Id { get; set; }

        public PlantExperimentDTO Experiment { get; set; }

        [JsonPropertyName("experiment_id")]
        public long ExperimentId { get; set; }

        public double LuftTemperatur { get; set; }

        public double LuftFugtighed { get; set; }

        public double JordFugtighed { get; set; }

        public string LysIndstilling { get; set; }

        public double LysHøjesteIntensitet { get; set; }

        public double LysLavesteIntensitet { get; set; }

        public double LysGennemsnit { get; set; }

        public double AfstandTilHøjde { get; set; }

        public double VandTidFraSidste { get; set; }

        public double VandMængde { get; set; }

        public double VandFrekvens { get; set; }

        public DateTime Timestamp { get; set; }
    }
}