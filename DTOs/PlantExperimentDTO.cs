namespace ApiGateway.DTOs
{
    public class PlantExperimentDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string PlantSpecies { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public List<PlantMeasurementsDTO> Measurements { get; set; } = new List<PlantMeasurementsDTO>();
    }
}