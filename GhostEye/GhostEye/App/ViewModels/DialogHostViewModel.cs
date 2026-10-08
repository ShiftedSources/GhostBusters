using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;

namespace GhostEye.App.ViewModels;

public sealed class DialogHostViewModel : ObservableObject
{
	private TaskCompletionSource<bool>? _completion;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? primaryCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? cancelCommand;

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsOpen
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsOpen);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsOpen);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string Title
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Title);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Title);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string Message
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Message);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Message);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string PrimaryText
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PrimaryText);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PrimaryText);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string CancelText
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.CancelText);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.CancelText);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsDestructive
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsDestructive);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsDestructive);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool HasCancel
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.HasCancel);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.HasCancel);
			}
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand PrimaryCommand => primaryCommand ?? (primaryCommand = new RelayCommand(Primary));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand CancelCommand => cancelCommand ?? (cancelCommand = new RelayCommand(Cancel));

	public DialogHostViewModel()
	{
		Title = string.Empty;
		Message = string.Empty;
		PrimaryText = "OK";
		CancelText = AppServices.Localizer["common.cancel"];
	}

	public Task<bool> ConfirmAsync(string title, string message, string? primaryText = null, string? cancelText = null, bool destructive = false)
	{
		_completion?.TrySetResult(result: false);
		Title = title;
		Message = message;
		PrimaryText = primaryText ?? AppServices.Localizer["common.confirm"];
		CancelText = cancelText ?? AppServices.Localizer["common.cancel"];
		IsDestructive = destructive;
		HasCancel = true;
		IsOpen = true;
		_completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		return _completion.Task;
	}

	public Task AlertAsync(string title, string message, string? primaryText = null)
	{
		_completion?.TrySetResult(result: false);
		Title = title;
		Message = message;
		PrimaryText = primaryText ?? AppServices.Localizer["common.gotIt"];
		IsDestructive = false;
		HasCancel = false;
		IsOpen = true;
		_completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		return _completion.Task;
	}

	[RelayCommand]
	private void Primary()
	{
		Close(result: true);
	}

	[RelayCommand]
	private void Cancel()
	{
		Close(result: false);
	}

	private void Close(bool result)
	{
		IsOpen = false;
		TaskCompletionSource<bool>? completion = _completion;
		_completion = null;
		completion?.TrySetResult(result);
	}
}
