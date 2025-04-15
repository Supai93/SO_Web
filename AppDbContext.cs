public class AppDbContext : DbContext
{
	public override async Task<int> SaveChangesAsync()
	{
		LogModifiedEntities();

		return await base.SaveChangesAsync();
	}

	private void LogModifiedEntities()
	{
		var objContext = ((IObjectContextAdapter)this).ObjectContext;
		var objManager = objContext.ObjectStateManager;

		var entries = ChangeTracker.Entries()
		              .Where(e => e.State == EntityState.Added || e.State == EntityState.Deleted || e.State == EntityState.Modified)
		              .ToArray();

		foreach (var entry in entries)
		{
			Type entityType = ObjectContext.GetObjectType(entry.Entity.GetType());
			object _object = entry.Entity;


			List<string> keys = new List<string>();
			var keyInfos = entityType
			               .GetProperties()
			               .Where(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(KeyAttribute)));

			foreach (var info in keyInfos)
			{
				keys.Add($"{info.Name}: {info.GetValue(_object)}");
			}

			Func<Type, bool> isCommonType = (t) => {
				return t.IsPrimitive || new[] { typeof(string), typeof(DateTime), typeof(decimal) }.Contains(t);
			};
			List<string> nonKeys = new List<string>();
			var nonKeyInfos = entityType
			                  .GetProperties(BindingFlags.Public | BindingFlags.Instance)
			                  .Where(p => p.CanRead && !p.CustomAttributes.Any(attr => attr.AttributeType == typeof(KeyAttribute) || attr.AttributeType == typeof(NotMappedAttribute)))
			                  .Where(p => isCommonType(p.PropertyType));

			foreach (var info in nonKeyInfos)
			{
				var value = info.GetValue(_object);
				if (value is DateTime)
				{
					DateTime valDate = (DateTime)value;
					if (valDate.TimeOfDay == TimeSpan.Zero) value = valDate.ToString("MM/dd/yyyy");
				}

				nonKeys.Add($"{info.Name}: {value}");
			}

			string state = entry.State.ToString().ToUpper();
			StringBuilder logDesc = new StringBuilder();

			switch (entry.State)
			{
			case EntityState.Added:
			case EntityState.Deleted:
				logDesc.Append($"[ {string.Join(", ", keys)} ] {{ {string.Join(", ", nonKeys)} }}");

				break;
			case EntityState.Modified:
				List<string> changes = new List<string>();
				var objState = objManager.GetObjectStateEntry(_object);
				foreach (string propName in objState.GetModifiedProperties())
				{
					var original = objState.OriginalValues[propName];
					var current = objState.CurrentValues[propName];

					if (original is DateTime)
					{
						DateTime ogDate = (DateTime)original;
						if (ogDate.TimeOfDay == TimeSpan.Zero) original = ogDate.ToString("MM/dd/yyyy");
					}

					if (current is DateTime)
					{
						DateTime curDate = (DateTime)current;
						if (curDate.TimeOfDay == TimeSpan.Zero) current = curDate.ToString("MM/dd/yyyy");
					}
					changes.Add($"{propName}: {{ {original} => {current} }}");
				}

				logDesc.Append($"[ {string.Join(", ", keys)} ] CHANGES {{ {string.Join(", ", changes)} }}");

				break;
			}

			var logger = UnityConfig.Container.Resolve<ILoggingService>();
			logger.LogInfo(logDesc.ToString(), nameof(entityType), state);
		}
	}
}
