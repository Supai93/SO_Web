public static class UnityConfig
{
	public static void RegisterTypes(IUnityContainer container)
	{
		// Application DbContext
		container.RegisterType<AppDbContext>(new PerRequestLifetimeManager());
		
		container.RegisterType<ILoggingService, SystemLoggerService>();
		container.RegisterFactory<ILogger>("LoggerSystem", c =>
		{
			var dbContext = c.Resolve<AppDbContext>();

			var logger = new LoggerConfiguration()
			.Enrich.FromLogContext()
			.WriteTo.Sink(new AppLogSink(dbContext))
			.CreateLogger();

			return logger;
		},FactoryLifetime.Scoped);
	}
}
