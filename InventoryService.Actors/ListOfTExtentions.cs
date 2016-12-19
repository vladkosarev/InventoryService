using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace InventoryService.Actors
{
    public static class ListOfTExtentions
    {
        public static string ToDelimitedText<T>(this List<T> instance,
            string delimiter,
            bool trimTrailingNewLineIfExists = false)
            where T : class
        {
            var itemCount = instance.Count;
            if (itemCount == 0) return string.Empty;

            var properties = GetPropertiesOfType<T>();
            var propertyCount = properties.Length;
            var outputBuilder = new StringBuilder();
           
            for (var itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                var listItem = instance[itemIndex];
                AppendListItemToOutputBuilder(outputBuilder, listItem, properties, propertyCount, delimiter, itemIndex==0);

                AddNewLineIfRequired(trimTrailingNewLineIfExists, itemIndex, itemCount, outputBuilder);
            }

            var output = TrimTrailingNewLineIfExistsAndRequired(outputBuilder.ToString(), trimTrailingNewLineIfExists);
            return output;
        }

        private static void AddDelimiterIfRequired(StringBuilder outputBuilder, int propertyCount, string delimiter,
            int propertyIndex)
        {
            var isLastProperty = (propertyIndex + 1 == propertyCount);
            if (!isLastProperty)
            {
                outputBuilder.Append(delimiter);
            }
        }

        private static void AddNewLineIfRequired(bool trimTrailingNewLineIfExists, int itemIndex, int itemCount,
            StringBuilder outputBuilder)
        {
            var isLastItem = (itemIndex + 1 == itemCount);
            if (!isLastItem || !trimTrailingNewLineIfExists)
            {
                outputBuilder.Append(Environment.NewLine);
            }
        }

        private static void AppendListItemToOutputBuilder<T>(StringBuilder outputBuilder,
            T listItem,
            IReadOnlyList<PropertyInfo> properties,
            int propertyCount,
            string delimiter, bool isFirstLine)
            where T : class
        {

            if (isFirstLine)
            {
            for (var propertyIndex = 0; propertyIndex < properties.Count; propertyIndex += 1)
              {
                var property = properties[propertyIndex];
                var propertyValue = property.Name;
                outputBuilder.Append(propertyValue);

                AddDelimiterIfRequired(outputBuilder, propertyCount, delimiter, propertyIndex);
              }
                outputBuilder.Append(Environment.NewLine);
            }
           
            for (var propertyIndex = 0; propertyIndex < properties.Count; propertyIndex += 1)
            {
                var property = properties[propertyIndex];
                var propertyValue = property.GetValue(listItem);
                outputBuilder.Append(propertyValue);

                AddDelimiterIfRequired(outputBuilder, propertyCount, delimiter, propertyIndex);
            }
        }

        private static PropertyInfo[] GetPropertiesOfType<T>() where T : class
        {
            var itemType = typeof(T);
            var properties = itemType.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
            return properties;
        }

        private static string TrimTrailingNewLineIfExistsAndRequired(string output, bool trimTrailingNewLineIfExists)
        {
            if (!trimTrailingNewLineIfExists || !output.EndsWith(Environment.NewLine)) return output;

            var outputLength = output.Length;
            var newLineLength = Environment.NewLine.Length;
            var startIndex = outputLength - newLineLength;
            output = output.Substring(startIndex, newLineLength);
            return output;
        }
    }
}