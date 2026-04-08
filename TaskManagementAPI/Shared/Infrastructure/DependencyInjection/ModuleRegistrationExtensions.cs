
namespace TaskManagementAPI.Shared.Infrastructure.DependencyInjection;

/// <summary>
/// Base interface for module registration patterns.
/// Modules implement this interface to register their services and endpoints.
/// </summary>
public interface IModuleRegistration
{
    /// <summary>
    /// Registers module services in the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    void RegisterServices(IServiceCollection services);

    /// <summary>
    /// Maps module endpoints in the application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    void MapEndpoints(IApplicationBuilder app);
}

/// <summary>
/// Extension methods for registering modules in the application.
/// </summary>
public static class ModuleRegistrationExtensions
{
    /// <summary>
    /// Registers a module's services in the DI container.
    /// </summary>
    /// <typeparam name="TModule">The module registration type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddModule<TModule>(this IServiceCollection services)
        where TModule : IModuleRegistration, new()
    {
        var module = new TModule();
        module.RegisterServices(services);
        return services;
    }

    /// <summary>
    /// Maps a module's endpoints in the application.
    /// </summary>
    /// <typeparam name="TModule">The module registration type.</typeparam>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder MapModule<TModule>(this IApplicationBuilder app)
        where TModule : IModuleRegistration, new()
    {
        var module = new TModule();
        module.MapEndpoints(app);
        return app;
    }
}
