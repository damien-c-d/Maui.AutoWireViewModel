using System.Reflection;

namespace Maui.AutoWireViewModel;

public static class ViewModelLocator
{
    private static IServiceProvider _services;

        public static MauiApp UseViewModelLocator(this MauiApp app)
    {
        _services = app.Services ?? throw new InvalidOperationException("UseViewModelLocator must be called after ConfigureServices");

        return app;
    }

    public static readonly BindableProperty AutoWireViewModelProperty =
        BindableProperty.CreateAttached("AutoWireViewModel", typeof(bool), typeof(ViewModelLocator), false, propertyChanged: OnAutoWireViewModelChanged);

    public static bool GetAutoWireViewModel(BindableObject bindable) => (bool)bindable.GetValue(AutoWireViewModelProperty);

    public static void SetAutoWireViewModel(BindableObject bindable, bool value) => bindable.SetValue(AutoWireViewModelProperty, value);

    private static void OnAutoWireViewModelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not Element element)
        {
            return;
        }

        var viewType = element.GetType();

        if (viewType?.FullName is null)
        {
            return;
        }

        var viewModelName = viewType.FullName.Replace("View", "ViewModel");

        if (!viewModelName.EndsWith("ViewModel"))
        {
            viewModelName = $"{viewModelName}ViewModel";
        }

        var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;

        var viewModelTypeName = $"{viewModelName}, {viewAssemblyName}";

        var viewModelType = Type.GetType(viewModelTypeName);

        if (viewModelType is null)
        {
            return;
        }

        var viewModel = _services.GetService(viewModelType);

        if (viewModel is null)
        {
            return;
        }

        element.BindingContext = viewModel;
    }
}
