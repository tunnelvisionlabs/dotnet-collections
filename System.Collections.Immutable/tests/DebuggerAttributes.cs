// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Diagnostics
{
    internal static class DebuggerAttributes
    {
        internal static object GetFieldValue(object obj, string fieldName)
        {
            return GetField(obj, fieldName).GetValue(obj);
        }

        internal static void ValidateDebuggerTypeProxyProperties(object obj)
        {
            // Get the DebuggerTypeProxyAttibute for obj
#if NET45PLUS
            var attrs = 
                obj.GetType().GetTypeInfo().CustomAttributes
                .Where(a => a.AttributeType == typeof(DebuggerTypeProxyAttribute))
                .ToArray();
#elif NET40PLUS
            var attrs = 
                obj.GetType().GetCustomAttributesData()
                .Where(a => a.Constructor.DeclaringType == typeof(DebuggerTypeProxyAttribute))
                .ToArray();
#else
            var attrs = obj.GetType().GetCustomAttributes(typeof(DebuggerTypeProxyAttribute), false);
#endif
            if (attrs.Length != 1)
            {
                throw new InvalidOperationException(
                    string.Format("Expected one DebuggerTypeProxyAttribute on {0}.", obj));
            }

#if NET40PLUS
            var cad = (CustomAttributeData)attrs[0];
#else
            var cad = (DebuggerTypeProxyAttribute)attrs[0];
#endif

            // Get the proxy type.  As written, this only works if the proxy and the target type
            // have the same generic parameters, e.g. Dictionary<TKey,TValue> and Proxy<TKey,TValue>.
            // It will not work with, for example, Dictionary<TKey,TValue>.Keys and Proxy<TKey>,
            // as the former has two generic parameters and the latter only one.
#if NET40PLUS
            Type proxyType = cad.ConstructorArguments[0].ArgumentType == typeof(Type) ?
                (Type)cad.ConstructorArguments[0].Value :
                Type.GetType((string)cad.ConstructorArguments[0].Value);
#else
            Type proxyType = cad.Target != null ? cad.Target : Type.GetType(cad.ProxyTypeName);
#endif

#if NET45PLUS
            var genericArguments = obj.GetType().GenericTypeArguments;
#else
            var genericArguments = obj.GetType().GetGenericArguments();
#endif
            if (genericArguments.Length > 0)
            {
                proxyType = proxyType.MakeGenericType(genericArguments);
            }

            // Create an instance of the proxy type, and make sure we can access all of the instance properties 
            // on the type without exception
            object proxyInstance = Activator.CreateInstance(proxyType, obj);
#if NET45PLUS
            var properties = proxyInstance.GetType().GetTypeInfo().DeclaredProperties;
#else
            var properties = proxyInstance.GetType().GetTypeInfo().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#endif
            foreach (var pi in properties)
            {
                pi.GetValue(proxyInstance, null);
            }
        }

        internal static void ValidateDebuggerDisplayReferences(object obj)
        {
            // Get the DebuggerDisplayAttribute for obj
#if NET45PLUS
            var attrs = 
                obj.GetType().GetTypeInfo().CustomAttributes
                .Where(a => a.AttributeType == typeof(DebuggerDisplayAttribute))
                .ToArray();
#elif NET40PLUS
            var attrs = 
                obj.GetType().GetCustomAttributesData()
                .Where(a => a.Constructor.DeclaringType == typeof(DebuggerDisplayAttribute))
                .ToArray();
#else
            var attrs = obj.GetType().GetCustomAttributes(typeof(DebuggerDisplayAttribute), false);
#endif
            if (attrs.Length != 1)
            {
                throw new InvalidOperationException(
                    string.Format("Expected one DebuggerDisplayAttribute on {0}.", obj));
            }

#if NET40PLUS
            var cad = (CustomAttributeData)attrs[0];

            // Get the text of the DebuggerDisplayAttribute
            string attrText = (string)cad.ConstructorArguments[0].Value;
#else
            string attrText = ((DebuggerDisplayAttribute)attrs[0]).Value;
#endif

            // Parse the text for all expressions
            var references = new List<string>();
            int pos = 0;
            while (true)
            {
                int openBrace = attrText.IndexOf('{', pos);
                if (openBrace < pos) break;
                int closeBrace = attrText.IndexOf('}', openBrace);
                if (closeBrace < openBrace) break;

                string reference = attrText.Substring(openBrace + 1, closeBrace - openBrace - 1).Replace(",nq", "");
                pos = closeBrace + 1;

                references.Add(reference);
            }
            if (references.Count == 0)
            {
                throw new InvalidOperationException(
                    string.Format("The DebuggerDisplayAttribute for {0} doesn't reference any expressions.", obj));
            }

            // Make sure that each referenced expression is a simple field or property name, and that we can
            // invoke the property's get accessor or read from the field.
            foreach (var reference in references)
            {
                PropertyInfo pi = GetProperty(obj, reference);
                if (pi != null)
                {
                    object ignored = pi.GetValue(obj, null);
                    continue;
                }

                FieldInfo fi = GetField(obj, reference);
                if (fi != null)
                {
                    object ignored = fi.GetValue(obj);
                    continue;
                }

                throw new InvalidOperationException(
                    string.Format("The DebuggerDisplayAttribute for {0} contains the expression \"{1}\".", obj, reference)); 
            }
        }

        private static FieldInfo GetField(object obj, string fieldName)
        {
            for (Type t = obj.GetType(); t != null; t = t.GetTypeInfo().BaseType)
            {
#if NET45PLUS
                FieldInfo fi = t.GetTypeInfo().GetDeclaredField(fieldName);
#else
                FieldInfo fi = t.GetTypeInfo().GetField(fieldName, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#endif
                if (fi != null)
                {
                    return fi;
                }
            }
            return null;
        }

        private static PropertyInfo GetProperty(object obj, string propertyName)
        {
            for (Type t = obj.GetType(); t != null; t = t.GetTypeInfo().BaseType)
            {
#if NET45PLUS
                PropertyInfo pi = t.GetTypeInfo().GetDeclaredProperty(propertyName);
#else
                PropertyInfo pi = t.GetTypeInfo().GetProperty(propertyName, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#endif
                if (pi != null)
                {
                    return pi;
                }
            }
            return null;
        }
    }
}
