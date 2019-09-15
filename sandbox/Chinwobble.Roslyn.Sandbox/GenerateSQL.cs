using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSharp.Data.SqlClient;
using System.Data.SqlClient;

namespace Chinwobble.Roslyn.Sandbox
{
    public class ConnectionStrings
    {
        public const string connStr = "";
    }

    [GenerateSql(
        statement: @"select 
id 
, date
, guid from log where id = @id

",
        conn: ConnectionStrings.connStr)]
    public partial class GetAllLogsQuery
    {
        public GetAllLogsQuery()
        {
        }
    }
}
