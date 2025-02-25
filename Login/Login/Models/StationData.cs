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
    
    public class Customers
    {
        public int Id { get; set; }
        public string Customername { get; set; }
    }    
    public class Modelos
    {
        public int Id { get; set; }
        public string Modelname { get; set; }
    }

    public class DashboardViewModel
    {
        public List<StationData> StationDataList { get; set; }
        public List<Customers> CustomersList { get; set; }
        public List<Modelos> ModelsList { get; set; }
    }
}
