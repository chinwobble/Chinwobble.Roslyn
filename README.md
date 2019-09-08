# Chinwobble.Roslyn

Roslyn-powered build-time code generation to generate the boilerplate needed to wire up [WPF DependencyProperty](https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/dependency-properties-overview) and [Xamarin BindableProperty](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/xaml/bindable-properties) at build time, accessible from intellisense on save.

Simply, [install the required packages](#installing) add the `[GenerateNotifyPropertyChanged]` to your ViewModels and `[GenerateDependencyProperty]` to your views.

## Usage

Mark your View / ViewModel as partial and add the appropriate attribute.

### Views

````csharp
[GenerateDependencyProperty]
public partial class ChartControl : System.Windows.Controls.Control
{
    public static readonly DependencyProperty YScalePrecisionProperty =
        DependencyProperty.Register(
            name: "YScalePrecision",
            propertyType: typeof(int),
            ownerType: typeof(ChartControl));
}

The compiler will automatically generate _wrapper_ properties for each BindableProperty and DependencyProperty on save.
```csharp
partial class ChartControl
{
    public int YScalePrecision
    {
        get
        {
            return (int)GetValue(YScalePrecisionProperty);
        }

        set
        {
            SetValue(YScalePrecisionProperty, value);
        }
    }
}
````

### View Models

Mark your class as partial and add the attribute `[GenerateNotifyPropertyChanged]`.

```csharp
[GenerateNotifyPropertyChanged]
public partial class MainViewModel : BindableObject
{
    private string _title;
    private string _name;
}
```

This package will automatically generate this boilerplate for you.

```csharp
partial class MainViewModel
{
    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }
}
```

## Installing

To use the the [StronglyTypedId NuGet package](https://www.nuget.org/packages/StronglyTypedId) you must add three packages:

- [Chinwobble.Roslyn](https://www.nuget.org/packages/Chinwobble.Roslyn)
- The `dotnet-codegen` .NET Core tool

To install the packages, add the references to your _csproj_ file so that it looks something like the following:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.2</TargetFramework>
  </PropertyGroup>

  <!-- Add these packages-->
  <ItemGroup>
    <PackageReference Include="Chinwobble.Roslyn" Version="0.1.2" />
    <DotNetCliToolReference Include="dotnet-codegen" Version="0.5.13" />
  </ItemGroup>
  <!-- -->

</Project>
```

Restore the tools using `dotnet restore`.

> Note this package and dotnet-codegen are **build time** dependencies - no extra dll's are added to your project's output! It's as though you wrote standard C# code yourself!

##

This package requires your project to anything compatible with .NET Standard 2.0
The code generation DotNetCliTool (`dotnet-codegen`) is only supported in SDK-format _csproj_ projects
