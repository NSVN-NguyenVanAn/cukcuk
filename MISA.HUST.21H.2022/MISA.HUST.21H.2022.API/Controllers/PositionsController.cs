using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.HUST._21H._2022.API.Entities;
using MySqlConnector;

namespace MISA.HUST._21H._2022.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        [HttpGet]
        ///API lấy danh sách tất cả vị trí
        ///<returns> Danh sách vị trí </returns>
        ///Created by An
        public IActionResult GetAllPositions()
        {


            try
            {
                //khởi tạo kết nối tới DB
                string connectionString = "Server = localhost; Port = 3306; Database = hust.21h.2022.nvan; Uid = root; Pwd = 04092001";
                var mySqlConnenction = new MySqlConnection(connectionString);
                //câu lệnh select
                var getAllPositionsCommand = "SELECT * FROM positions;";

                //Thực hiện gọi vào DB để chạy câu lệnh select
                var positions = mySqlConnenction.Query<Position>(getAllPositionsCommand);

                //Xử lý kết quả trả về từ DB
                if (positions != null)
                {
                    //Trả về dữ liệu cho client
                    return StatusCode(StatusCodes.Status200OK, positions);
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "e002");
                }
            }

            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode(StatusCodes.Status400BadRequest, "e001");
            }
        }

    }
}
