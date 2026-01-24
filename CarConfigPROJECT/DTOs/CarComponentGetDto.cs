namespace CarConfigPROJECT.DTOs
{
    public class CarComponentGetDto
    {
        public string Name { get; set; }

        public int Id { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; }
    }
}
