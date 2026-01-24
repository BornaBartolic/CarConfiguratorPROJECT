namespace CarConfigPROJECT.DTOs
{
    public class CarComponentCompatibilityGetDto
    {
        public int Id { get; set; }

        public int CarComponentId1 { get; set; }
        public string CarComponent1Name { get; set; } = null!;

        public int CarComponentId2 { get; set; }
        public string CarComponent2Name { get; set; } = null!;
    }
}

