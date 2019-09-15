namespace FSharp.Data.SqlClient

open CodeGeneration.Roslyn
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open System.Threading.Tasks
open System.Threading
open System.Data.SqlClient
open FSharp.Data.SqlClient
open FSharp.Data
open Microsoft.CodeAnalysis.CSharp.Syntax
open System
open System.Diagnostics
open System.Linq

module internal Helpers = 
    let mapList<'t> (list: 't list) = 
        if list.IsEmpty 
            then None
            else Some list
    let tryGetClassSyntaxNode: (CSharpSyntaxNode -> ClassDeclarationSyntax Option) = function
        | :? ClassDeclarationSyntax as classDec -> Some classDec
        | _ -> None

[<AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)>]
[<Sealed>]
[<Conditional("CodeGeneration")>]
[<CodeGeneration.Roslyn.CodeGenerationAttribute(typeof<SqlGenerator>)>]
type GenerateSqlAttribute =
    inherit Attribute
    val ConnectionString: string
    val Statement: string
    new(conn: string, statement: string) =
        { 
          ConnectionString = conn
          Statement = statement
        }

and internal SqlGenerator =
    val ConnectionString: string
    val Statement: string
    new(attrib: AttributeData) =
        if attrib.ConstructorArguments.Length <> 2
        then
            {
              ConnectionString = String.Empty
              Statement = String.Empty
            }
        else
            { 
              ConnectionString = attrib.ConstructorArguments.[0].Value.ToString()
              Statement = attrib.ConstructorArguments.[1].Value.ToString()
            }
        
    interface CodeGeneration.Roslyn.ICodeGenerator with
        member this.GenerateAsync(context: TransformationContext, progress: System.IProgress<Diagnostic>, cancellationToken: CancellationToken): Task<SyntaxList<CSharp.Syntax.MemberDeclarationSyntax>> = 
            if this.ConnectionString = String.Empty || this.Statement = String.Empty 
            then
                Task.FromResult(SyntaxFactory.List<MemberDeclarationSyntax>())
            else 
            let a = new Microsoft.CodeAnalysis.DiagnosticDescriptor("SQL1", "test", "{0} connection: {1}", "SQL", DiagnosticSeverity.Warning, true, "", "", System.Array.Empty<string>())

            progress.Report(Diagnostic.Create(a, context.ProcessingNode.GetLocation(), "connecting", this.ConnectionString))

            let conn = new SqlConnection(this.ConnectionString)
            use closeConn = conn.UseLocally()
            progress.Report(Diagnostic.Create(a, context.ProcessingNode.GetLocation(), "connection open", this.ConnectionString))
            conn.CheckVersion()
            conn.LoadDataTypesMap()
            let parameters = DesignTime.ExtractParameters(conn, this.Statement, false)
            let output = DesignTime.GetOutputColumns(conn, this.Statement, parameters, false)

            let partialClass = 
                Helpers.tryGetClassSyntaxNode context.ProcessingNode
                |> Option.map (fun x -> SyntaxFactory.ClassDeclaration(x.Identifier.ValueText))
                |> Option.map (fun x -> x.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                |> Option.map (fun classDec -> 
                    let outputClassProperties = 
                        output
                        |> List.map (fun x -> sprintf "public %s %s { get; set; }" x.TypeInfo.ClrTypeFullName x.Name)
                    let resultClassDefinition = 
                        sprintf "public class Result { %s }" (System.String.Join("\n", outputClassProperties))
                    
                    let constructor = 
                        sprintf "public %s(string connectionString) => ConnectionString=connectionString;" classDec.Identifier.ValueText
                        
                    let connStrProperty = 
                        "public string ConnectionString { get; }"
                        
                    let executeMethod = 
                        parameters
                        |> List.map (fun x -> {| paramName = x.Name; TypeName = x.TypeInfo.ClrTypeFullName |})
                        |> fun methodParams -> 
                            let methodSig = sprintf "public async Task<System.Collections.Generic.IEnumerable<%s.Result>> Execute(%s)" classDec.Identifier.ValueText (System.String.Join(",", methodParams.Select(fun x -> sprintf "%s %s" x.TypeName x.paramName)))
                            let addParamsStr = 
                                methodParams 
                                |> List.map (fun methodParam -> (sprintf "parameters.Add(nameof(%s), %s);" methodParam.paramName methodParam.paramName))
                                |> (String.concat "")
                            sprintf """
                                %s
                                {
                                    using (var db = new SqlConnection(ConnectionString))
                                    {
                                        var parameters = new Dapper.DynamicParameters();
                                        %s
                                        return await Dapper.SqlMapper.QueryAsync<%s.Result>(db, @"%s", parameters);
                                    }
                                }
                                """ methodSig addParamsStr classDec.Identifier.ValueText this.Statement

                    [|resultClassDefinition; constructor; connStrProperty; executeMethod|]
                    |> Array.map SyntaxFactory.ParseMemberDeclaration
                    |> fun x -> classDec.AddMembers(x)
                )
            
            partialClass
            |> Option.map (fun x -> SyntaxFactory.List<MemberDeclarationSyntax>([|x|]))
            |> Option.defaultValue (SyntaxFactory.List())
            |> Task.FromResult
            
            
                
        