
using System;
using System.IO;
using System.Collections.Generic;

namespace Interpreter.Tool;

public class GenerateAst
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: generate_ast <output directory>");
            Environment.Exit(64);
        }

        string outputDir = args[0];

        DefineAst(outputDir, "Expr", new List<string>
        {
            "Assign   : Token name, Expr value",
            "Binary   : Expr left, Token operator, Expr right",
            "Grouping : Expr expression",
            "Literal  : object? value",
            "Logical  : Expr left, Token operator, Expr right",
            "Unary    : Token operator, Expr right",
            "Variable : Token name"
        });
        DefineAst(outputDir, "Stmt", new List<string>
        {
        "Block      : List<Stmt> statements",
        "Expression : Expr expression",
        "If         : Expr condition, Stmt thenBranch," +
                        " Stmt elseBranch",
        "Print      : Expr expression",
        "Var        : Token name, Expr? initializer",
        "While      : Expr condition, Stmt body"
        });
        }

    private static void DefineAst(
        string outputDir, string baseName, List<string> types)
    {
        Directory.CreateDirectory(outputDir);

        string path = Path.Combine(outputDir, baseName + ".cs");

        using StreamWriter writer = new StreamWriter(path, false);

        writer.WriteLine("namespace Interpreter.Lox;");
        writer.WriteLine();
        writer.WriteLine("public abstract class " + baseName);
        writer.WriteLine("{");

        DefineVisitor(writer, baseName, types);

        foreach (string type in types)
        {
            string className = type.Split(':')[0].Trim();
            string fields = type.Split(':')[1].Trim();

            DefineType(writer, baseName, className, fields);
        }

        writer.WriteLine(
            "    public abstract R Accept<R>(Visitor<R> visitor);");

        writer.WriteLine("}");
    }

    private static void DefineVisitor(
        StreamWriter writer, string baseName, List<string> types)
    {
        writer.WriteLine("    public interface Visitor<R>");
        writer.WriteLine("    {");

        foreach (string type in types)
        {
            string typeName = type.Split(':')[0].Trim();

            writer.WriteLine(
                $"        R Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
        }

        writer.WriteLine("    }");
        writer.WriteLine();
    }

    private static void DefineType(
        StreamWriter writer, string baseName,
        string className, string fieldList)
    {
        writer.WriteLine(
            $"    public class {className} : {baseName}");
        writer.WriteLine("    {");

        string[] fields = fieldList.Split(", ");

        // Constructor
        writer.WriteLine(
            $"        public {className}({fieldList.Replace("operator", "@operator")})");
        writer.WriteLine("        {");

        foreach (string field in fields)
        {
            string[] parts = field.Split(' ');
            string name = parts[1];
            string parameter = name == "operator" ? "@operator" : name;
            string property = char.ToUpper(name[0]) + name.Substring(1);

            writer.WriteLine($"            {property} = {parameter};");
        }

        writer.WriteLine("        }");
        writer.WriteLine();

        // Properties
        foreach (string field in fields)
        {
            string[] parts = field.Split(' ');
            string fieldType = parts[0];
            string name = parts[1];

            string property = char.ToUpper(name[0]) + name.Substring(1);

            writer.WriteLine(
                $"        public {fieldType} {property} {{ get; }}");
        }

        writer.WriteLine();

        // Visitor pattern
        writer.WriteLine(
            $"        public override R Accept<R>(Visitor<R> visitor)");
        writer.WriteLine("        {");
        writer.WriteLine(
            $"            return visitor.Visit{className}{baseName}(this);");
        writer.WriteLine("        }");

        writer.WriteLine("    }");
        writer.WriteLine();
    }
}
