using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using DVM4T.Base;
using System.IO;
using DVM4T.Attributes;
using System.CodeDom.Compiler;

namespace DVM4T.ModelGen
{
    public class CodeGenerator
    {
        private ICodeGenConfiguration configuration;
        private string providerName;
        private string namespaceName;
        public CodeGenerator(ICodeGenConfiguration configuration, string namespaceName,
            string providerName = "CSharp")
        {
            this.configuration = configuration;
            this.providerName = providerName;
            this.namespaceName = namespaceName;
        }
        public void GenerateModelCode(ViewModel model, DirectoryInfo saveDir)
        {
            //TODO: Make code more modular, add more comments

            CodeCompileUnit compileUnit = new CodeCompileUnit();
            CodeNamespace globalNamespace = new CodeNamespace();
            globalNamespace.Imports.AddRange(
                configuration.Namespaces.Select(x => new CodeNamespaceImport(x)).ToArray());
            //Add global using statements
            compileUnit.Namespaces.Add(globalNamespace);

            CodeTypeDeclaration modelClass = new CodeTypeDeclaration(model.Name);
            // Sets the member attributes for the type to public
            modelClass.Attributes = MemberAttributes.Public;
            // Set the base class which the model inherits from
            modelClass.BaseTypes.Add(model.BaseClass.Name);
            //Add the View Model Attribute
            modelClass.CustomAttributes.Add(new CodeAttributeDeclaration("ViewModel",
                new CodeAttributeArgument(new CodePrimitiveExpression(model.SchemaName)), //Schema Name
                new CodeAttributeArgument(new CodePrimitiveExpression(true)) //Is Default
                ));
            foreach (var field in model.FieldProperties)
            {
                if (configuration.FieldAttributeTypes.ContainsKey(field.FieldType))
                {
                    CodeAttributeDeclaration fieldAttribute = new CodeAttributeDeclaration(
                        configuration.FieldAttributeTypes[field.FieldType].Name, //Field Attribute Name
                        new CodeAttributeArgument(new CodePrimitiveExpression(field.FieldName))); //Schema Field Name
                   
                    if (field.FieldType == FieldType.Linked)
                    {
                        //Field is a Linked Component, we need to assemble the list of possible Types
                        var modelTypes = new List<CodeExpression>();
                        //Populate list of Types
                        foreach (var modelName in field.LinkedComponentTypeNames)
                        {
                            modelTypes.Add(new CodeTypeOfExpression(modelName));
                        }
                        if (modelTypes.Count > 0)
                        {
                            //Create Array of Types and pass in the list of Types
                            CodeArrayCreateExpression codeArrayCreate = new CodeArrayCreateExpression("Type",
                            modelTypes.ToArray());
                            //Add the array of Types to the Attribute arguments
                            fieldAttribute.Arguments.Add(
                                new CodeAttributeArgument(configuration.LinkedComponentTypesAttributeParameterName,
                                    codeArrayCreate));
                        }
                    }
                    else if (field.FieldType == FieldType.Embedded
                        && !string.IsNullOrEmpty(field.EmbeddedTypeName))
                    {
                        //Add the required Type argument for embedded fields
                        fieldAttribute.Arguments.Add(
                            new CodeAttributeArgument(new CodeTypeOfExpression(field.EmbeddedTypeName)));
                    }
                    if (field.IsMultiValue)
                    {
                        //Add boolean value for multiple values
                        fieldAttribute.Arguments.Add(new CodeAttributeArgument("AllowMultipleValues",
                            new CodePrimitiveExpression(true)));
                    }
                    if (field.IsMetadata)
                    {
                        //Add boolean for metadata fields
                        fieldAttribute.Arguments.Add(new CodeAttributeArgument("IsMetadata",
                            new CodePrimitiveExpression(true)));
                    }
                    //Create and populate the model Property
                    CodeMemberField property = new CodeMemberField { Name = field.PropertyName }; //Use CodeMemberField as a hack to add a blank getter/setter
                    property.Attributes = MemberAttributes.Public;
                    property.Type = GetPropertyType(model.BaseClass, field);
                    property.CustomAttributes.Add(fieldAttribute); //Add the Field Attribute
                    property.Name += " { get; set; }"; //Hack to add empty get/set
                    modelClass.Members.Add(property);
                }
            }
            if (configuration.ModelAttributeTypes.ContainsKey(model.ModelType))
            {
                var attrs = configuration.ModelAttributeTypes[model.ModelType];
                //loop through all the "extra" model properties in the config that don't necessarily have a direct link to the Schema i.e. multimedia
                foreach (var attr in attrs)
                {
                    CodeAttributeDeclaration modelAttribute = new CodeAttributeDeclaration(attr.Name); //Field Attribute Name
                    CodeMemberField property = new CodeMemberField { Name = attr.DefaultPropertyName }; //Use CodeMemberField as a hack to add a blank getter/setter
                    property.Attributes = MemberAttributes.Public;
                    property.Type = new CodeTypeReference(attr.ReturnTypeName);
                    property.CustomAttributes.Add(modelAttribute); //Add the Field Attribute
                    property.Name += " { get; set; }"; //Hack to add empty get/set
                    modelClass.Members.Add(property);
                }
            }
            var ns = new CodeNamespace(namespaceName);
            ns.Types.Add(modelClass);
            compileUnit.Namespaces.Add(ns);

            if (!saveDir.Exists) saveDir.Create();
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(saveDir.FullName, model.ContainingFolder));
            if (!dir.Exists) dir.Create();
            string filePath = Path.Combine(dir.FullName, model.Name + ".cs");
            CreateFile(filePath, compileUnit);
        }

        private void CreateFile(string filePath, CodeCompileUnit compileUnit)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider(providerName);
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            options.BlankLinesBetweenMembers = true;
            options.IndentString = "    ";
            StringBuilder sb = new StringBuilder();
            using (StringWriter stringWriter = new StringWriter(sb))
            {
                provider.GenerateCodeFromCompileUnit(
                    compileUnit, stringWriter, options);
                //this is another hack to remove the trailing ';' added after each Property
                string contents = stringWriter.GetStringBuilder().Replace("};","}").ToString(); 
                using (StreamWriter fileWriter = new StreamWriter(filePath))
                {
                    fileWriter.Write(contents);
                }
            }
        }

        private CodeTypeReference GetPropertyType(Type baseType, FieldProperty fieldProp)
        {
            var config = configuration.FieldAttributeTypes[fieldProp.FieldType];            
            //return new CodeTypeReference(fieldProp.IsMultiValue ? config.MultiExpectedReturnTypeName : config.SingleExpectedReturnTypeName);

            CodeTypeReference singleType = new CodeTypeReference(config.ReturnTypeName);
            CodeTypeReference result;
            if (fieldProp.FieldType == FieldType.Embedded)
            {
                singleType = new CodeTypeReference(fieldProp.EmbeddedTypeName);
            }
            else if (fieldProp.FieldType == FieldType.Linked && fieldProp.LinkedComponentTypeNames.Count == 1)
            {
                //If there's only one allowed linked type, make it the generic type
                singleType = new CodeTypeReference(fieldProp.LinkedComponentTypeNames[0]);
            }
            if (fieldProp.IsMultiValue)
            {
                if (fieldProp.FieldType == FieldType.Embedded || fieldProp.FieldType == FieldType.Linked)
                {
                    //use ViewModelList
                    result = new CodeTypeReference("ViewModelList"); //Have to use this otherwise couldn't use proper generic types
                    result.TypeArguments.Add(singleType);
                }
                else
                {
                    result = new CodeTypeReference(config.MultiExpectedReturnTypeName); //Assuming all other types implement IList
                }   
            }
            else
            {
                result = singleType;
            }
            return result;
        }
    }
}
