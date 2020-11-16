using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Resources;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System.Collections.Generic;

namespace MySQLWeb.Resources
{
public class CustomValidationMetadataProvider : IValidationMetadataProvider
{
	private ResourceManager resourceManager;
	private Type resourceType;

	public CustomValidationMetadataProvider(string baseName, Type type)
	{
		resourceType = type;
		resourceManager = new ResourceManager(baseName,
			type.GetTypeInfo().Assembly);
	}

	public void CreateValidationMetadata(ValidationMetadataProviderContext context)
	{
		if( context.Key.ModelType.GetTypeInfo().IsValueType &&
			context.ValidationMetadata.ValidatorMetadata.Where(m => m.GetType() == typeof(RequiredAttribute)).Count() == 0)
			context.ValidationMetadata.ValidatorMetadata.Add(new RequiredAttribute());

		foreach(var attribute in context.ValidationMetadata.ValidatorMetadata){
			ValidationAttribute tAttr = attribute as ValidationAttribute;
			if(tAttr == null || tAttr.ErrorMessageResourceName != null)
				continue;
			//何故かEmailAddressAttributeはErrorMessageがデフォルトでnullにならない。
			if(tAttr.ErrorMessage != null && attribute as EmailAddressAttribute == null || string.IsNullOrEmpty(tAttr.ErrorMessage))
				continue;
			var name = tAttr.GetType().Name;
			if(resourceManager.GetString(name) != null){
				tAttr.ErrorMessageResourceType = resourceType;
				tAttr.ErrorMessageResourceName = name;
				tAttr.ErrorMessage = null;
			}
		}
	}
}
}