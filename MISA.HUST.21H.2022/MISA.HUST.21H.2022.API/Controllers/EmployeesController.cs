using Dapper;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MISA.HUST._21H._2022.API.Entities;
using MISA.HUST._21H._2022.API.Entities.DTO;
using MySqlConnector;

namespace MISA.HUST._21H._2022.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        [HttpGet]
        ///API lấy danh sách nhân viên
        ///<returns> Danh sách nhân viên </returns>
        ///Created by An
        public IActionResult GetAllEmployees()
        {

            try
            {
                //khởi tạo kết nối tới DB
                string connectionString = "Server = localhost; Port = 3306; Database = hust.21h.2022.nvan; Uid = root; Pwd = 04092001";
                var mySqlConnenction = new MySqlConnection(connectionString);
                //câu lệnh select
                var getAllEmployeesCommand = "SELECT * FROM employee;";

                //Thực hiện gọi vào DB để chạy câu lệnh select
                var employees = mySqlConnenction.Query<Employee>(getAllEmployeesCommand);

                //Xử lý kết quả trả về từ DB
                if (employees != null)
                {
                    //Trả về dữ liệu cho client
                    return StatusCode(StatusCodes.Status200OK, employees);
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

        /// <summary>
        /// API lấy thông tin nhân viên
        /// </summary>
        /// <param name="employeeID"> ID nhân viên</param>
        /// <returns>Thông tin chi tiết 1 nhân viên</returns>
        [HttpGet]
        [Route("{employeeID}")]
        public IActionResult GetEmployeeByID([FromRoute] Guid employeeID)
        {
            try
            {
                //khởi tạo kết nối tới DB
                string connectionString = "Server = localhost; Port = 3306; Database = hust.21h.2022.nvan; Uid = root; Pwd = 04092001";
                var mySqlConnenction = new MySqlConnection(connectionString);
                //câu lệnh select by id
                var getEmployeeCommand = "SELECT * FROM employee WHERE EmployeeID = @EmployeeID;";

                //Chuẩn bị tham số đầu vào cho câu lệnh select
                var param = new DynamicParameters();
                param.Add("@EmployeeID", employeeID);

                //Thực hiện gọi vào DB để chạy câu lệnh select
                var employee = mySqlConnenction.QueryFirstOrDefault<Employee>(getEmployeeCommand, param);

                //Xử lý kết quả trả về từ DB
                if (employee != null)
                {
                    //Trả về dữ liệu cho client
                    return StatusCode(StatusCodes.Status200OK, employee);
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

        /// <summary>
        /// API lọc danh sách nhân viên có điều kiện tìm kiếm và phân trang
        /// </summary>
        /// <param name="keyword">Từ khoá muốn tìm kiếm(mã nv, tên nv, sdt)</param>
        /// <param name="positionID">ID vị trí</param>
        /// <param name="departmentID">ID phòng ban</param>
        /// <param name="limit">số bản ghi trong 1 trang</param>
        /// <param name="offset">Vị trí bản ghi bắt đầu lấy dữ liệu</param>
        /// <returns>Danh sách nhân viên</returns>
        [HttpGet]
        [Route("filter")]
        public IActionResult FilterEmployees(
            [FromQuery] string keyword,
            [FromQuery] Guid positionID,
            [FromQuery] Guid departmentID,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1)
        {

            try
            {
                //khởi tạo kết nối tới DB
                string connectionString = "Server = localhost; Port = 3306; Database = hust.21h.2022.nvan; Uid = root; Pwd = 04092001";
                var mySqlConnenction = new MySqlConnection(connectionString);
                //Chuẩn bị tên Procedure
                string procedureName = "Proc_employee_GetPaging";

                //Tham số đầu vào cho cho procedure
                var param = new DynamicParameters();
                param.Add(" v_Offset", (pageNumber - 1) * pageSize);
                param.Add(" v_Limit", pageSize);
                param.Add(" v_Sort", "ModifiedDate DESC");

                //xây dựng lệnh where
                var or = new List<string>();
                var and = new List<string>();
                string whereClause = "";
                if (keyword != null)
                {
                    or.Add($"EmployeeCode LIKE '%{keyword}%'");
                    or.Add($"EmployeeName LIKE '%{keyword}%'");
                    or.Add($"PhoneNumber LIKE '%{keyword}%'");
                }
                if (or.Count > 0)
                {
                    whereClause = $"({string.Join(" OR ", or)})";
                }
                if (positionID != null)
                {
                    and.Add($"PositionID LIKE '%{positionID}%'");
                }
                if (departmentID != null)
                {
                    and.Add($"DepartmentID LIKE '%{departmentID}%'");
                }
                if (and.Count > 0)
                {
                    whereClause += $" AND ({string.Join(" AND ", and)})";
                }

                param.Add("v_Where", whereClause);

                //Thực hiện gọi vào DB để chạy câu lệnh 
                var multiResults = mySqlConnenction.QueryMultiple(procedureName, param, commandType: System.Data.CommandType.StoredProcedure);

                //Xử lý kết quả trả về từ DB
                if (multiResults != null)
                {
                    var employees = multiResults.Read<Employee>().ToList();
                    var totalCount = multiResults.Read<long>().Single();
                    return StatusCode(StatusCodes.Status200OK, new PagingData<Employee>()
                    {
                        Data = employees,
                        TotalCount = totalCount
                    });
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

        /// <summary>
        /// API tạo mã nhân viên mới tự động tăng
        /// </summary>
        /// <returns>mã nhân viên mới</returns>
        [HttpGet("new-code")]
        public IActionResult GetAutoIncrementEmployeeCode()
        {
            try
            {
                //khởi tạo kết nối tới DB
                string connectionString = "Server = localhost; Port = 3306; Database = hust.21h.2022.nvan; Uid = root; Pwd = 04092001";
                var mySqlConnenction = new MySqlConnection(connectionString);
                //câu lệnh select
                var getAutoIncrementEmployeeCode = "SELECT MAX(EmployeeCode) FROM employee;";

                //Thực hiện gọi vào DB để chạy câu lệnh select
                string maxEmployeeCode = mySqlConnenction.QueryFirstOrDefault<string>(getAutoIncrementEmployeeCode);
                //convert mã nhân viên + 1
                string newEmployeeCode = "NV" + (Int64.Parse(maxEmployeeCode.Substring(2)) + 1).ToString();

                //Xử lý kết quả trả về từ DB
                
                //Trả về dữ liệu cho client
                return StatusCode(StatusCodes.Status200OK, newEmployeeCode);
                
            }

            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode(StatusCodes.Status400BadRequest, "e001");
            }
        }
        /// <summary>
        /// API thêm mới 1 nhân viên
        /// </summary>
        /// <param name="employee">Đối tượng nhân viên cần thêm</param>
        /// <returns>ID của nhân viên vừa thêm mới</returns>

        [HttpPost]
        public IActionResult InsertEmployee([FromBody] Employee employee)
        {
            try
            {
                //khởi tạo kết nối tới DB
                string connectionString = "Server = localhost; Port = 3306; Database = hust.21h.2022.nvan; Uid = root; Pwd = 04092001";
                var mySqlConnenction = new MySqlConnection(connectionString);
                //câu lệnh insert
                var insertEmployeeCommand = "INSERT INTO employee (EmployeeID, EmployeeCode, EmployeeName, DateOfBirth, Gender, IdentityNumber, IdentityIssuePlace, IdentityIssuedDate, Email, PhoneNumber, PositionID, PositionName, DepartmentID, DepartmentName, TaxCode, Salary, JoiningDate, WorkStatus, CreatedDate, CreatedBy, ModifiedDate, ModifiedBy) VALUES ( @EmployeeID, @EmployeeCode, @EmployeeName, @DateOfBirth, @Gender, @IdentityNumber, @IdentityIssuePlace, @IdentityIssuedDate, @Email, @PhoneNumber, @PositionID, @PositionName, @DepartmentID, @DepartmentName, @TaxCode, @Salary, @JoiningDate, @WorkStatus, @CreatedDate, @CreatedBy, @ModifiedDate, @ModifiedBy);";

                //Tham số đầu vào cho cho insert
                var employeeID = Guid.NewGuid();
                var param = new DynamicParameters();
                param.Add("EmployeeID", employeeID);
                param.Add("EmployeeCode", employee.EmployeeCode);
                param.Add("EmployeeName", employee.EmployeeName);
                param.Add("DateOfBirth", employee.DateOfBirth);
                param.Add("Gender", employee.Gender);
                param.Add("IdentityNumber", employee.IdentityNumber);
                param.Add("IdentityIssuePlace", employee.IdentityIssuePlace);
                param.Add("IdentityIssuedDate", employee.IdentityIssuedDate);
                param.Add("Email", employee.Email);
                param.Add("PhoneNumber", employee.PhoneNumber);
                param.Add("PositionID", employee.PositionID);
                param.Add("PositionName", employee.PositionName);
                param.Add("DepartmentID", employee.DepartmentID);
                param.Add("DepartmentName", employee.DepartmentName);
                param.Add("TaxCode", employee.TaxCode);
                param.Add("Salary", employee.Salary);
                param.Add("JoiningDate", employee.JoiningDate);
                param.Add("WorkStatus", employee.WorkStatus);
                param.Add("CreatedDate", employee.CreatedDate);
                param.Add("CreatedBy", employee.CreatedBy);
                param.Add("ModifiedDate", employee.ModifiedDate);
                param.Add("ModifiedBy", employee.ModifiedBy);

                //Thực hiện gọi vào DB để chạy insert với tham số đầu vào trên
                int numberOfAffectedRows = mySqlConnenction.Execute(insertEmployeeCommand, param);

                //Xử lý kết quả trả về từ DB
                if (numberOfAffectedRows > 0)
                {
                    //Trả về dữ liệu cho client
                    return StatusCode(StatusCodes.Status200OK, employeeID);
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "e002");
                }

            }
            catch (MySqlException mySqlException)
            {
                if (mySqlException.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "e003");
                }
                return StatusCode(StatusCodes.Status400BadRequest, "e001");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode(StatusCodes.Status400BadRequest, "e001");
            }

        }


        /// <summary>
        /// API sửa 1 nhân viên
        /// </summary>
        /// <param name="employee">Đối tượng nhân viên cần sửa</param>
        /// <param name="employeeID">ID của nhân viên cần sửa</param>
        /// <returns>ID của nhân viên vừa sửa mới</returns>

        [HttpPut]
        [Route("{EmployeeID}")]
        public IActionResult UpdateEmployee([FromBody] Employee employee, [FromRoute] Guid employeeID)
        {
            try
            {
                //khởi tạo kết nối tới DB
                string connectionString = "Server = localhost; Port = 3306; Database = hust.21h.2022.nvan; Uid = root; Pwd = 04092001";
                var mySqlConnenction = new MySqlConnection(connectionString);
                //câu lệnh update
                string updateEmployeesCommand = "UPDATE employee SET " +
                    "EmployeeCode = @EmployeeCode," +
                    "EmployeeName = @EmployeeName," +
                    "DateOfBirth = @DateOfBirth," +
                    "Gender = @Gender," +
                    "IdentityNumber = @IdentityNumber," +
                    "IdentityIssuePlace = @IdentityNumber," +
                    "IdentityIssuedDate = @IdentityIssuedDate," +
                    "Email = @Email," +
                    "PhoneNumber = @PhoneNumber," +
                    "PositionID = @PositionID," +
                    "PositionName = @ositionName," +
                    "DepartmentID = @DepartmentID," +
                    "DepartmentName = DepartmentName," +
                    "TaxCode = @TaxCode," +
                    "Salary = @Salary," +
                    "JoiningDate = @JoiningDate," +
                    "WorkStatus = @WorkStatus," +
                    "CreatedDate = @CreatedDate," +
                    "CreatedBy = @CreatedBy," +
                    "ModifiedDate = @ModifiedDate," +
                    "ModifiedBy = @ModifiedBy " +
                    "WHERE EmployeeID = @EmployeeID;";

                var param = new DynamicParameters();
                param.Add("EmployeeID", employeeID);
                param.Add("EmployeeCode", employee.EmployeeCode);
                param.Add("EmployeeName", employee.EmployeeName);
                param.Add("DateOfBirth", employee.DateOfBirth);
                param.Add("Gender", employee.Gender);
                param.Add("IdentityNumber", employee.IdentityNumber);
                param.Add("IdentityIssuePlace", employee.IdentityIssuePlace);
                param.Add("IdentityIssuedDate", employee.IdentityIssuedDate);
                param.Add("Email", employee.Email);
                param.Add("PhoneNumber", employee.PhoneNumber);
                param.Add("PositionID", employee.PositionID);
                param.Add("PositionName", employee.PositionName);
                param.Add("DepartmentID", employee.DepartmentID);
                param.Add("DepartmentName", employee.DepartmentName);
                param.Add("TaxCode", employee.TaxCode);
                param.Add("Salary", employee.Salary);
                param.Add("JoiningDate", employee.JoiningDate);
                param.Add("WorkStatus", employee.WorkStatus);
                param.Add("CreatedDate", employee.CreatedDate);
                param.Add("CreatedBy", employee.CreatedBy);
                param.Add("ModifiedDate", employee.CreatedBy);
                param.Add("ModifiedBy", employee.CreatedBy);

                //Thực hiện gọi vào DB để chạy update với tham số đầu vào trên
                int numberOfAffectedRows = mySqlConnenction.Execute(updateEmployeesCommand, param);

                //Xử lý kết quả trả về từ DB
                if (numberOfAffectedRows > 0)
                {
                    //Trả về dữ liệu cho client
                    return StatusCode(StatusCodes.Status200OK, employeeID);
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


        /// <summary>
        /// API xoá 1 nhân viên
        /// </summary>
        /// <param name="employeeID">ID của nhân viên cần xoá</param>
        /// <returns>ID của nhân viên vừa xoá</returns>

        [HttpDelete]
        [Route("{EmployeeID}")]
        public IActionResult DeleteEmployee([FromRoute] Guid employeeID)
        {
            try
            {
                //khởi tạo kết nối tới DB
                string connectionString = "Server = localhost; Port = 3306; Database = hust.21h.2022.nvan; Uid = root; Pwd = 04092001";
                var mySqlConnenction = new MySqlConnection(connectionString);
                //câu lệnh select
                var deleteEmployeeCommand = "DELETE FROM employee WHERE EmployeeID = @EmployeeID;";

                //Chuẩn bị tham số đầu vào cho câu lệnh Delete
                var param = new DynamicParameters();
                param.Add("@EmployeeID", employeeID);

                //Thực hiện gọi vào DB để chạy câu lệnh delete
                int numberOfAffectedRows = mySqlConnenction.Execute(deleteEmployeeCommand, param);

                //Xử lý kết quả trả về từ DB
                if (numberOfAffectedRows > 0)
                {
                    //Trả về dữ liệu cho client
                    return StatusCode(StatusCodes.Status200OK, employeeID);
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

