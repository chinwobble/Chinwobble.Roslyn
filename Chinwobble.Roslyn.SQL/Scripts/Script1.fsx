#load @"../../.paket/load/net47/main.group.fsx"

#load "../SqlClient/Extensions.fs"
#load "../SqlClient/Shared.fs"
#load "../SqlClient/Configuration.fs"
#load "../SqlClient/DataTable.fs"
#load "../SqlClient/DynamicRecord.fs"
#load "../SqlClient/ISqlCommand.fs"
#load "../SqlClient/Runtime.fs"

#load "../SqlClientExtensions.fs"
#load "../DesignTimeConnectionString.fs"
#load "../DesignTime.fs"

// test output columns

// test extract parameters 

open System.Data.SqlClient
open FSharp.Data.Extensions
open FSharp.Data.SqlClient.Extensions
open FSharp.Data.SqlClient

let testInputParameters(connStr, designTimeSqlStatement, allParametersOptional) = 
    let conn = new SqlConnection(connStr)
    use closeConn = conn.UseLocally()
    conn.CheckVersion()
    conn.LoadDataTypesMap()
    DesignTime.ExtractParameters(conn, designTimeSqlStatement, allParametersOptional)


let TestOutputColumns(connStr, designTimeSqlStatement, allParametersOptional) = 
    let conn = new SqlConnection(connStr)
    use closeConn = conn.UseLocally()
    conn.CheckVersion()
    conn.LoadDataTypesMap()
    let parameters = DesignTime.ExtractParameters(conn, designTimeSqlStatement, allParametersOptional)
    DesignTime.GetOutputColumns(conn, designTimeSqlStatement, parameters, isStoredProcedure = false)

let connStr = "server=localhost;integrated security=true;database=3dss_dev"

let testCommandText = "select * from company where id = @id"

testInputParameters(connStr, testCommandText, false)

TestOutputColumns(connStr, testCommandText, false)