using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Utils;

public class DatabaseContextFactory<T>(Func<T> factory) : IDbContextFactory<T> where T : DbContext {
	public T CreateDbContext() => factory();
}
