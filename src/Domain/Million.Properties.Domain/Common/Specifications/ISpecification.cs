using System.Linq.Expressions;

namespace Million.PropertiesService.Domain.Common.Specifications;

/// <summary>
/// Specification pattern interface for encapsulating query logic
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Expression that defines the criteria for the specification
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }
    
    /// <summary>
    /// List of include expressions for eager loading
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }
    
    /// <summary>
    /// List of include strings for eager loading
    /// </summary>
    List<string> IncludeStrings { get; }
    
    /// <summary>
    /// Order by expression
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }
    
    /// <summary>
    /// Order by descending expression
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }
    
    /// <summary>
    /// Number of results to take (for paging)
    /// </summary>
    int? Take { get; }
    
    /// <summary>
    /// Number of results to skip (for paging)
    /// </summary>
    int? Skip { get; }
    
    /// <summary>
    /// Whether paging is enabled
    /// </summary>
    bool IsPagingEnabled { get; }
}