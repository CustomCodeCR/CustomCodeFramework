using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CustomCodeFramework.Postgres.EntityFramework.Interceptors;

public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor { }
