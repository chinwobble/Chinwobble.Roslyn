using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSharp.Data.SqlClient;

namespace Chinwobble.Roslyn.Sandbox
{
    [GenerateSql(
        connStr,
        "select * from log where id = @id")]
    public partial class GetAllLogsQuery
    {
        public const string connStr = ""; // your db 
    }
}
