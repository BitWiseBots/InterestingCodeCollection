public interface IUpdateableModel : INotifyPropertyChanged
{
	ModelState State { get; set; }
	bool HasUnsavedChanges();
	void ResetState();
}
    
public enum ModelState
{
        New,
        Unmodified,
        Modified,
        Deleted
}

public abstract class UpdateableModel : IUpdateableModel
{
	private static readonly MethodInfo SetPropertyMethod;
	private static readonly MethodInfo ResetItemsExtensionMethod;
	private readonly Dictionary<string, Tuple<Expression, object>> _originalValues;
	private readonly List<string> _differingFields;
	private readonly Dictionary<string, object> _properties;

	private ModelState _state;

	static UpdateableModel()
	{
		SetPropertyMethod = typeof(UpdateableModel).GetMethod("SetProperty", BindingFlags.NonPublic | BindingFlags.Instance);
		ResetItemsExtensionMethod = typeof(UpdateableModelExtensions).GetMethod("ResetItemStates", BindingFlags.Public | BindingFlags.Static);
	}

	protected UpdateableModel(bool isNewModel)
	{
		_originalValues = new Dictionary<string, Tuple<Expression, object>>();
		_differingFields = new List<string>();
		_properties = new Dictionary<string, object>();
		if (isNewModel) return;

		State = ModelState.Unmodified;
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public ModelState State
	{
		get
		{
			return _state;
		}
		set
		{
			if (_state == value) return;
			_state = value;
			OnPropertyChanged();
		}
	}

	public bool HasUnsavedChanges()
	{
		return State != ModelState.Unmodified
			   || GetNestedUpdateableModels().Any(item => item.HasUnsavedChanges())
			   || GetNestedUpdateableModelCollections().Any(coll => coll.Any(item => item.HasUnsavedChanges()));
	}

	/// <summary>
	/// Reset State is meant to be called when discarding changes, it will reset the State value to Unmodified and set all modified values back to their original value.
	/// </summary>
	public void ResetState()
	{
		State = ModelState.Unmodified;

		var currentDifferingFields = new List<string>(_differingFields);

		foreach (var differingField in currentDifferingFields)
		{
			var type = GetFuncType(_originalValues[differingField].Item1);

			var genericPropertySetter = SetPropertyMethod.MakeGenericMethod(type);
			genericPropertySetter.Invoke(this,
				new[] { _originalValues[differingField].Item1, _originalValues[differingField].Item2 });
		}

		GetNestedUpdateableModels().ToList().ResetItemStates();

		//We need to call the extension method on the collection, so that it can remove the New items.
		//Since we can't know what implementation of IUpdateableModel the collection uses we need to use reflection
		var collections = GetNestedUpdateableModelCollections();
		foreach (var collection in collections)
		{
			var type = collection.GetType().GetTypeInfo().GenericTypeArguments[0];

			var genericExtensionMethod = ResetItemsExtensionMethod.MakeGenericMethod(type);
			genericExtensionMethod.Invoke(collection, new object[] { collection });
		}


	}

	protected bool SetProperty<T>(Expression<Func<T>> expression, T value)
	{
		var wasUpdated = false;
		var memberName = GetPropertyName(expression);
		var currValue = default(T);
		object objValue;

		if (_properties.TryGetValue(memberName, out objValue))
		{
			currValue = (T)objValue;
		}

		if (!Compare(currValue, value) || Compare(currValue, default(T)))
		{
			_properties[memberName] = value;

			wasUpdated = true;


			UpdateState(expression, value);


			OnPropertyChanged(memberName);
		}

		return wasUpdated;
	}

	protected void SetPropertyDefault<T>(Expression<Func<T>> expression, T value)
	{
		var memberName = GetPropertyName(expression);
		_properties[memberName] = value;
	}

	protected T GetProperty<T>(Expression<Func<T>> expression)
	{
		var memberName = GetPropertyName(expression);

		if (_properties.ContainsKey(memberName))
		{
			return (T)_properties[memberName];
		}
		return default(T);
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		var handler = PropertyChanged;
		handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private static string GetPropertyName(Expression expr)
	{
		var mbrExpr = GetMemberExpression(expr);

		return mbrExpr?.Member.Name ?? string.Empty;
	}

	private static Type GetFuncType(Expression expr)
	{
		var lambda = expr as LambdaExpression;
		var member = lambda?.Body as MemberExpression;
		return member?.Type;
	}

	private static MemberExpression GetMemberExpression(Expression expr)
	{
		var lambda = expr as LambdaExpression;
		return lambda?.Body as MemberExpression;
	}

	private void UpdateState<T>(Expression<Func<T>> expression, T value)
	{
		if (State == ModelState.New)
		{
			return;
		}

		var propertyName = GetPropertyName(expression);
		if (!_originalValues.ContainsKey(propertyName))
		{
			_originalValues.Add(propertyName, new Tuple<Expression, object>(expression, value));
		}

		else
		{
			if (!Compare(_originalValues[propertyName].Item2, value))
			{
				_differingFields.Add(propertyName);
			}
			else if (_differingFields.Contains(propertyName))
			{
				_differingFields.Remove(propertyName);
			}

			State = _differingFields.Count == 0
				? ModelState.Unmodified
				: ModelState.Modified;
		}
	}

	private IEnumerable<IUpdateableModel> GetNestedUpdateableModels()
	{
		return _properties.Values.OfType<IUpdateableModel>();
	}

	private IEnumerable<IEnumerable<IUpdateableModel>> GetNestedUpdateableModelCollections()
	{
		return _properties.Values.OfType<IEnumerable<IUpdateableModel>>();
	}

	private static bool Compare<T>(T x, T y)
	{
		return EqualityComparer<T>.Default.Equals(x, y);
	}
}
