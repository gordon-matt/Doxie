#nullable enable
using System.Diagnostics;
using System.Text;

namespace Doxie.Core.Solutions;

/// <summary>Runs <c>dotnet build</c> so a project can produce an XML documentation file.</summary>
public static class DotNetBuilder
{
    /// <summary>
    /// Builds a project or solution in Release with <c>GenerateDocumentationFile</c> turned on.
    /// The working directory is the file's folder. Standard output and standard error are both
    /// captured.
    /// </summary>
    /// <param name="projectOrSolutionPath">Project or solution to build.</param>
    /// <returns>Whether the process exited with code 0, and the combined log.</returns>
    /// <exception cref="InvalidOperationException"><c>dotnet</c> could not be started.</exception>
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

/// <summary>The outcome of <see cref="DotNetBuilder.Build"/>.</summary>
/// <param name="Succeeded"><see langword="true"/> when the process exited with code 0.</param>
/// <param name="Log">Standard output and standard error, in arrival order. Empty when the build printed nothing.</param>
public sealed record BuildResult(bool Succeeded, string Log);
