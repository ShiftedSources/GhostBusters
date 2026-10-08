using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace GhostEye.App.Controls;

public partial class InfoTip : UserControl, IComponentConnector
{
	public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(InfoTip));

	public static readonly DependencyProperty SummaryProperty = DependencyProperty.Register("Summary", typeof(string), typeof(InfoTip));

	public static readonly DependencyProperty DetailsProperty = DependencyProperty.Register("Details", typeof(string), typeof(InfoTip));

	public static readonly DependencyProperty FooterProperty = DependencyProperty.Register("Footer", typeof(string), typeof(InfoTip));

	public string? Header
	{
		get
		{
			return (string)GetValue(HeaderProperty);
		}
		set
		{
			SetValue(HeaderProperty, value);
		}
	}

	public string? Summary
	{
		get
		{
			return (string)GetValue(SummaryProperty);
		}
		set
		{
			SetValue(SummaryProperty, value);
		}
	}

	public string? Details
	{
		get
		{
			return (string)GetValue(DetailsProperty);
		}
		set
		{
			SetValue(DetailsProperty, value);
		}
	}

	public string? Footer
	{
		get
		{
			return (string)GetValue(FooterProperty);
		}
		set
		{
			SetValue(FooterProperty, value);
		}
	}

	public InfoTip()
	{
		InitializeComponent();
	}
}
