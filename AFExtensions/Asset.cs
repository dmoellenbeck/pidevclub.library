// Copyright 2016 OSIsoft, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OSIsoft.AF;
using OSIsoft.AF.Asset;

/************************************************************************************************************************
 * Note about recursion.
 * 
 * Element recursion can be quite slow the FIRST pass since it may require an RPC to the Asset Server FOR EACH element.
 * Subsequent element recusion should then be faster.
 * 
 * Unlike element recursion, attribute recursion should be quite fast requiring AT MOST one trip to the server
 * to fully load any attributes not previously loaded.  If the element has already been fully loaded, no
 * additional trips to the server are needed for attribute recursion.
 ************************************************************************************************************************/

namespace PIDevClub.Library.AFExtensions
{
    public static class Asset
    {
        #region "Attribute"

        /// <summary>
        /// Indicates whether an <see cref="AFAttribute"/> simply uses the "PI Point" data reference.  
        /// This is much faster than <see cref="IsPIPointValid"/> as it does not attempt to validate that the point exists.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static bool UsesPIPointDR(this AFAttribute attribute) => attribute.DataReferencePlugIn != null && attribute.DataReferencePlugIn.Name == "PI Point";

        /// <summary>
        /// Indicates whether an <see cref="AFAttribute"/> has a validated <see cref="PIPoint"/>.
        /// This is much slower than <see cref="UsesPIPointDR"/> because it takes time to validate the point exists.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static bool IsPIPointValid(this AFAttribute attribute) => attribute.PIPoint != null && attribute.PIPoint.ID != 0;

        /// <summary>
        /// Returns a flattened <see cref="AFAttributeList"/> of all generations of attributes for the specified <see cref="AFBaseElement"/>.
        /// This may be used for bulk data calls which specifically require an <see cref="AFAttributeList"/> rather than generic list or enumerable collection.
        /// </summary>
        /// <param name="element">A <see cref="AFBaseElement"/> which could be a <see cref="AFElement"/>, <see cref="AFNotification"/>, or a <see cref="AFEventFrame"/>.</param>
        /// <returns></returns>
        public static AFAttributeList GetFlatAttributeList(this AFBaseElement element) => new AFAttributeList(GetAllAttributeGenerations(element.Attributes));

        /// <summary>
        /// Returns a flattened <see cref="IEnumerable<AFAttribute>"/> collection of all generations of all attributes belonging to the specified <see cref="AFBaseElement"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static IEnumerable<AFAttribute> GetAllAttributeGenerations(this AFBaseElement element) => GetAllAttributeGenerations(element.Attributes);

        /// <summary>
        /// Returns a flattened <see cref="IEnumerable<AFAttribute>"/> collection of all generations of all attributes including those specified as inputs.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static IEnumerable<AFAttribute> GetAllAttributeGenerations(this IEnumerable<AFAttribute> attributes)
        {
            if (attributes == null)
                yield break;
            foreach (AFAttribute attribute in attributes)
            {
                yield return attribute;
                if (attribute.HasChildren)
                {
                    // Recursion:
                    var descendants = GetAllAttributeGenerations(attribute.Attributes);
                    foreach (AFAttribute descendant in descendants)
                    {
                        yield return descendant;
                    }
                }
            }
        }

        #endregion // Attribute

        #region "Attribute Template"

        /// <summary>
        /// Indicates whether an <see cref="AFAttributeTemplate"/> uses the "PI Point" data reference.  
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public static bool UsesPIPointDR(this AFAttributeTemplate template) => template.DataReferencePlugIn != null && template.DataReferencePlugIn.Name == "PI Point";


        /// <summary>
        /// Returns a flattened <see cref="IEnumerable<AFAttributeTemplate>"/> collection of all generations of all attribute templates belonging to the specified <see cref="AFElementTemplate"/>.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public static IEnumerable<AFAttributeTemplate> GetAllAttributeTemplateGenerations(this AFElementTemplate template) => GetAllAttributeTemplateGenerations(template.AttributeTemplates);

        /// <summary>
        /// Returns a flattened <see cref="IEnumerable<AFAttributeTemplate>"/> collection of all generations of all attribute templates including those specified as inputs.
        /// </summary>
        /// <param name="templates"></param>
        /// <returns></returns>
        public static IEnumerable<AFAttributeTemplate> GetAllAttributeTemplateGenerations(this IEnumerable<AFAttributeTemplate> templates)
        {
            if (templates == null)
                yield break;
            foreach (var template in templates)
            {
                yield return template;
                if (template.HasChildren)
                {
                    // Recursion:
                    var descendants = GetAllAttributeTemplateGenerations(template.AttributeTemplates);
                    foreach (var descendant in descendants)
                    {
                        yield return descendant;
                    }
                }
            }
        }

        #endregion // Attribute Template

    }
}
