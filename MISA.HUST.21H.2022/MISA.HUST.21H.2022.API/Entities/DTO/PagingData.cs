﻿namespace MISA.HUST._21H._2022.API.Entities.DTO
{
    /// <summary>
    /// Dữ liệu trả về từ API lọc nhân  viên
    /// </summary>
    public class PagingData
    {
        /// <summary>
        /// Danh sách nhân viên
        /// </summary>
        public List<Employee> Data { get; set; }
        /// <summary>
        /// Tổng số bản ghi thoả mãn điều kiện
        /// </summary>
        public int TotalCount { get; set; }
    }
}
