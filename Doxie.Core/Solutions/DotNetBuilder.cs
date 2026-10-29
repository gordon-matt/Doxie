#nullable enable
using System.Diagnostics;
using System.Text;

namespace Doxie.Core.Solutions;

public static class DotNetBuilder
{
    public static BuildResult Build(string projectOrSolutionPath)
    {
        var start = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectOrSolutionPath}\" -c Release --nologo /p:GenerateDocumentationFile=true",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(projectOrSolutionPath) ?? Environment.CurrentDirectory
        };

        using Process process = Process.Start(start) ?? throw new InvalidOperationException("dotnet could not be started.");
        var log = new StringBuilder();
        process.OutputDataReceived += (_, eventArgs) => Append(log, eventArgs.Data);
        process.ErrorDataReceived += (_, eventArgs) => Append(log, eventArgs.Data);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        return new BuildResult(process.ExitCode == 0, log.ToString());
    }

    private static void Append(StringBuilder log, string? line)
    {
        if (line == null)
        {
            return;
        }

        lock (log)
        {
            log.AppendLine(line);
        }
    }
}

public sealed record BuildResult(bool Succeeded, string Log);
