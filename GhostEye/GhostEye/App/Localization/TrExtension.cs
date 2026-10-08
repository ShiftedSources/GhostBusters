using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace GhostEye.App.Localization;

public sealed class TrExtension : MarkupExtension
{
	[ConstructorArgument("key")]
	public string Key { get; set; }

	public TrExtension()
	{
		Key = string.Empty;
	}

	public TrExtension(string key)
	{
		Key = key;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return new Binding("[" + Key + "]")
		{
			Source = LocalizationSource.Instance,
			Mode = BindingMode.OneWay
		}.ProvideValue(serviceProvider);
	}
}
