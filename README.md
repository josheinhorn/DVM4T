# DVM4T
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
	-Models should be marked with a ViewModelAttribute (
	-Models should have a parameterless default constructor
	-Property values should have a parameterless default constructor
	-Property values must have a setter
	
In general, almost every part of the framework can be modified by implementing custom versions of classes that need to change and injecting then into the proper Constructor. For example, instead of create new objects with the default constructor, one could extend the current ViewModelResolver and implement their own object resolving. This is however usually not recommended due to a large number of runtime optimizations that have been developed in the base implementations.
	
This project also comes with a configurable code generator (console application) that builds the domain models and adds basic attributes based on Tridion Schemas. See the DD4T configuration XML file for an example.
