namespace MISA.HUST._21H._2022.API.Entities
{ 
    /// <summary>
    /// Thông tin vị trí
    /// </summary>
    public class Position
    {
        public Guid PositionID { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }
}
