namespace Doxie.Core.Tests.Samples;

/// <summary>
/// A documented sample. See <see cref="DocumentedSample.Limit"/>.
/// </summary>
public class DocumentedSample : ISample
{
    /// <summary>The highest accepted value.</summary>
    public const int Limit = 3;

    /// <summary>A public field.</summary>
    public string Title = string.Empty;

    /// <summary>Raised after the value changes.</summary>
    public event EventHandler? Changed;

    /// <inheritdoc/>
    public string Label => "sample";

    /// <inheritdoc/>
    public string Echo(string incoming) => incoming;

    /// <summary>Accepts a variable number of values.</summary>
    public void Take(params int[] values) => _ = values;
}

/// <summary>Implemented by <see cref="DocumentedSample"/>.</summary>
public interface ISample
{
    /// <summary>The current title.</summary>
    string Label { get; }

    /// <summary>
    /// Stores <paramref name="incoming"/>.
    /// </summary>
    /// <param name="incoming">The incoming value.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="incoming"/> is null.</exception>
    /// <returns>The stored value.</returns>
    string Echo(string incoming);
}

/// <summary>Looks up a value.</summary>
public interface IFinder<T>
{
    /// <summary>Finds the value for <paramref name="id"/>.</summary>
    T Find(int id);
}

/// <summary>A generic implementation.</summary>
public class Finder<T> : IFinder<T>
{
    /// <inheritdoc/>
    public T Find(int id) => default!;
}