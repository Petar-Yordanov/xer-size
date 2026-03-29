using CommunityToolkit.Mvvm.ComponentModel;

namespace XerSize.ViewModels.Base;

public abstract class ValidatableViewModelBase : ObservableValidator
{
    public bool CanSubmit => !HasErrors;

    protected void RefreshValidationState()
    {
        ValidateAllProperties();
        OnPropertyChanged(nameof(CanSubmit));
    }
}
