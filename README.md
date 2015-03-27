# DVM4T
## Overview
Domain View Models For Tridion - a framework for creating domain view models for a Tridion .NET web application. DD4T Nuget package can be found here https://www.nuget.org/packages/DVM4T.DD4T/

This framework aims to solve the problem of creating Domain Models when using SDL Tridion. It is based primarily on .NET Custom Attributes, though there is support for fluent syntax property binding. DVM4T at its core is just a base for concrete implementations. Each implementation must imlpement the basic Content Model (e.g. Components, Fields, Pages, etc.) and Custom Attributes for decorating the Domain Models. The framework takes care of actually building and populating models and comes with a number of base Attribute classes for implementations to easily extend including:

	-FieldAttributeBase
	-NestedModelFieldAttributeBase (for complex nested types)
	-ComponentAttributeBase
	-PageAttributeBase
	-ComponentPresentationsAttributeBase
	-ComponentTemplateAttributeBase
	-PageTemplateAttributeBase

The first implementation (included here) uses DD4T to map Tridion data to the DVM4T Content Model. There are a large number of custom attributes inheriting from the various Attribute base classes.

Some basic requirements:

	-Models should be marked with a ViewModelAttribute
	-Models should have a parameterless default constructor
	-Property values should have a parameterless default constructor
	-Property values must have a setter
	
In general, almost every part of the framework can be modified by implementing custom versions of classes that need to change and injecting then into the proper Constructor. For example, instead of create new objects with the default constructor, one could extend the current ViewModelResolver and implement their own object resolving. This is however usually not recommended due to a large number of runtime optimizations that have been developed in the base implementations.
	
This project also comes with a configurable code generator (console application) that builds the domain models and adds basic attributes based on Tridion Schemas. See the DD4T configuration XML file for an example.

## DD4T Implementation
The only current implementation uses DD4T to bind Tridion data to models and the NuGet package can be found [here](https://www.nuget.org/packages/DVM4T.DD4T/).

Here are a few examples that demonstrates a number of the attributes that are offered and how they are used:

````C#
[ViewModel("ContentContainer", true)]
public class ContentContainer : ViewModelBase
{
    [TextField]
    public IEnumerable<string> Title { get; set; }

    [LinkedComponentField(FieldName = "content", LinkedComponentTypes = new Type[] { typeof(GeneralContent) })]
    public List<GeneralContent> ContentList { get; set; }

    [EmbeddedSchemaField(EmbeddedModelType = typeof(EmbeddedLink))]
    public List<EmbeddedLink> Links { get; set; }

    [LinkedComponentField(LinkedComponentTypes = new Type[] { typeof(Image) })]
    public Image Image { get; set; }

    [TextField(FieldName = "view", IsTemplateMetadata = true)]
    public string ViewName { get; set; }
}

    [ViewModel("EmbeddedLink", true)]
    public class EmbeddedLink : ViewModelBase
    {
        [TextField]
        public string LinkText { get; set; }

        [ResolvedUrlField(FieldName = "internalLink")]
        public string ResolvedInternalLink { get; set; }

        [TextField]
        public string ExternalLink { get; set; }

        [KeywordKeyField(FieldName = "openInNewWindow")]
        public string Target { get; set; }
    }

    [ViewModel("Image", true)]
    public class Image : ViewModelBase
    {
        [MultimediaUrl]
        public string Url { get; set; }

        [Multimedia]
        public IMultimediaData Multimedia { get; set; }

        [TextField(FieldName = "alt", IsMetadata = true)]
        public string AltText { get; set; }
    }
    [ViewModel("GeneralContent", true, ViewModelKeys = new string[] { "BasicGeneralContent", "Test" })]
    public class GeneralContent : ViewModelBase
    {
        [TextField]
        public string Title { get; set; }

        [RichTextField(FieldName = "body", InlineEditable = true)]
        public MvcHtmlString Body { get; set; }

        [KeywordKeyField(IsBooleanValue = true)]
        public bool ShowOnTop { get; set; }

        [NumberField(FieldName = "numberOfColumns")]
        public double Columns { get; set; }

        [TextField(FieldName = "view", IsTemplateMetadata = true)]
        public string ViewName { get; set; }

        [KeywordField(KeywordType = typeof(Color))]
        public Color BackgroundColor { get; set; }

        [EnumField(AllowMultipleValues = true)]
        public List<StateType> State { get; set; }
    }

    [KeywordViewModel(new string[] {"Colors"})]
    public class Color : ViewModelBase
    {
        [KeywordData]
        public IKeywordData Keyword { get; set; }

        [TextField(FieldName = "rgbValue", IsMetadata = true)]
        public string RgbValue { get; set; }

        [NumberField(FieldName = "decimalValue", IsMetadata = true)]
        public double NumericValue { get; set; }
    }
````

All of these Attributes and their arguments are fully documented in the code, though there are a number of important overriding things to know and rules to follow"
+	View Models must have a public parameterless constructor
+	View Models must be marked with a View Model Attribute (an Attribute that implements `IModelAttribute`)
+	Properties must be marked with a Property Attribute (an Attribute that implements `IPropertyAttribute`)
+	Only the first View Model and Property Attributes found on a Model or Property will be used
+	Members decorated with a Property Attribute must be a Property and must have a `set` method
+	Multi-value Properties are supported through Types that implement `ICollection<T>` or are an Array (`T[]`). You do not need to specify which is a multi-value field.
+	Nullable types are currently not supported
+	













