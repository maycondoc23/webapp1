namespace Login.Models
{
    public class StationFilterModel
    {
        public DateTime? Date { get; set; }
    }

    public class StationData
    {
        public string Station { get; set; }
        public int Count { get; set; }
    }
}
