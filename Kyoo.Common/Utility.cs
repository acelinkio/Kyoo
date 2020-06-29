using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Kyoo.Models;
using Kyoo.Models.Attributes;
using Microsoft.VisualBasic;

namespace Kyoo
{
	public static class Utility
	{
		public static string ToSlug(string str)
		{
			if (str == null)
				return null;

			str = str.ToLowerInvariant();
			
			string normalizedString = str.Normalize(NormalizationForm.FormD);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char c in normalizedString)
			{
				UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
					stringBuilder.Append(c);
			}
			str = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

			str = Regex.Replace(str, @"\s", "-", RegexOptions.Compiled);
			str = Regex.Replace(str, @"[^\w\s\p{Pd}]", "", RegexOptions.Compiled);
			str = str.Trim('-', '_');
			str = Regex.Replace(str, @"([-_]){2,}", "$1", RegexOptions.Compiled);
			return str;
		}
		
		
		public static void SetImage(Show show, string imgUrl, ImageType type)
		{
			switch(type)
			{
				case ImageType.Poster:
					show.Poster = imgUrl;
					break;
				case ImageType.Logo:
					show.Logo = imgUrl;
					break;
				case ImageType.Background:
					show.Backdrop = imgUrl;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		public static IEnumerable<T> MergeLists<T>(IEnumerable<T> first,
			IEnumerable<T> second, 
			Func<T, T, bool> isEqual = null)
		{
			if (first == null)
				return second;
			if (second == null)
				return first;
			if (isEqual == null)
				return first.Concat(second).ToList();
			List<T> list = first.ToList();
			return list.Concat(second.Where(x => !list.Any(y => isEqual(x, y)))).ToList();
		}

		public static T Complete<T>(T first, T second)
		{
			Type type = typeof(T);
			foreach (PropertyInfo property in type.GetProperties())
			{
				if (!property.CanRead || !property.CanWrite)
					continue;
				
				object value = property.GetValue(second);
				object defaultValue = property.PropertyType.IsValueType
					? Activator.CreateInstance(property.PropertyType) 
					: null;
				
				if (value?.Equals(defaultValue) == false)
					property.SetValue(first, value);
			}

			return first;
		}

		public static T Merge<T>(T first, T second)
		{
			if (first == null)
				return second;
			if (second == null)
				return first;
			
			Type type = typeof(T);
			foreach (PropertyInfo property in type.GetProperties().Where(x => x.CanRead && x.CanWrite))
			{
				if (Attribute.GetCustomAttribute(property, typeof(NotMergableAttribute)) != null)
					continue;
				
				object oldValue = property.GetValue(first);
				object newValue = property.GetValue(second);
				object defaultValue = property.PropertyType.IsValueType
					? Activator.CreateInstance(property.PropertyType) 
					: null;
				
				if (oldValue?.Equals(defaultValue) != false)
					property.SetValue(first, newValue);
				else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType)
				         && property.PropertyType != typeof(string))
				{
					property.SetValue(first, RunGenericMethod(
						typeof(Utility), 
						"MergeLists",
						GetEnumerableType(property.PropertyType),
						new []{ oldValue, newValue, null}));
				}
			}

			if (first is IOnMerge merge)
				merge.OnMerge(second);
			return first;
		}

		public static T Nullify<T>(T obj)
		{
			Type type = typeof(T);
			foreach (PropertyInfo property in type.GetProperties())
			{
				if (!property.CanWrite)
					continue;
				
				object defaultValue = property.PropertyType.IsValueType 
					? Activator.CreateInstance(property.PropertyType) 
					: null;
				property.SetValue(obj, defaultValue);
			}

			return obj;
		}

		public static object RunGenericMethod(
			[NotNull] Type owner, 
			[NotNull] string methodName,
			[NotNull] Type type,
			IEnumerable<object> args)
		{
			if (owner == null)
				throw new ArgumentNullException(nameof(owner));
			if (methodName == null)
				throw new ArgumentNullException(nameof(methodName));
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			MethodInfo method = owner.GetMethod(methodName);
			if (method == null)
				throw new NullReferenceException($"A method named {methodName} could not be found on {owner.FullName}");
			return method.MakeGenericMethod(type).Invoke(null, args?.ToArray());
		}
		
		public static object RunGenericMethod(
			[NotNull] object instance, 
			[NotNull] string methodName,
			[NotNull] Type type,
			IEnumerable<object> args)
		{
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (methodName == null)
				throw new ArgumentNullException(nameof(methodName));
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			MethodInfo method = instance.GetType().GetMethod(methodName);
			if (method == null)
				throw new NullReferenceException($"A method named {methodName} could not be found on {instance.GetType().FullName}");
			return method.MakeGenericMethod(type).Invoke(instance, args?.ToArray());
		}

		public static Type GetEnumerableType([NoEnumeration] [NotNull] IEnumerable list)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			Type type = list.GetType().GetInterfaces().FirstOrDefault(t => typeof(IEnumerable).IsAssignableFrom(t) 
			                                                         && t.GetGenericArguments().Any()) ?? list.GetType();
			return type.GetGenericArguments().First();
		}
		
		public static Type GetEnumerableType([NotNull] Type listType)
		{
			if (listType == null)
				throw new ArgumentNullException(nameof(listType));
			if (!typeof(IEnumerable).IsAssignableFrom(listType))
				throw new InvalidOperationException($"The {nameof(listType)} parameter was not an IEnumerable.");
			Type type = listType.GetInterfaces().FirstOrDefault(t => typeof(IEnumerable).IsAssignableFrom(t) 
			                                                && t.GetGenericArguments().Any()) ?? listType;
			return type.GetGenericArguments().First();
		}

		public static IEnumerable<List<T>> BatchBy<T>(this List<T> list, int countPerList)
		{
			for (int i = 0; i < list.Count; i += countPerList)
				yield return list.GetRange(i, Math.Min(list.Count - i, countPerList));
		}
		
		public static IEnumerable<T[]> BatchBy<T>(this IEnumerable<T> list, int countPerList)
		{
			T[] ret = new T[countPerList];
			int i = 0;
			
			using IEnumerator<T> enumerator = list.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ret[i] = enumerator.Current;
				i++;
				if (i < countPerList)
					continue;
				i = 0;
				yield return ret;
			}
		}

		public static string GetMemberName<T>(Expression<Func<T, object>> key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			
			if (!(key.Body is MemberExpression member))
				throw new ArgumentException("Key should be a member of the object.");
			
			return member.Member.Name;
		}

		public static Expression<Func<T, bool>> ParseWhere<T>(Dictionary<string, string> where)
		{
			if (where == null || where.Count == 0)
				return null;
			
			ParameterExpression param = Expression.Parameter(typeof(T));
			Expression expression = null;

			foreach (KeyValuePair<string, string> cond in where)
			{
				string value = cond.Value;
				string operand = "eq";
				if (value.Contains(':'))
				{
					operand = value.Substring(0, value.IndexOf(':'));
					value = value.Substring(value.IndexOf(':') + 1);
				}

				PropertyInfo valueParam = typeof(T).GetProperty(cond.Key); // TODO get this property with case insensitive.
				// TODO throw if valueParam is null.
				MemberExpression property = Expression.Property(param, valueParam);
				ConstantExpression condValue = Expression.Constant(value); // TODO Cast this to the right type (take nullable into account).
				
				Expression condition = operand switch
				{
					"eq" => Expression.Equal(property, condValue),
					"not" => Expression.NotEqual(property, condValue),
					"lt" => Expression.LessThan(property, condValue),
					"lte" => Expression.LessThanOrEqual(property, condValue),
					"gt" => Expression.GreaterThan(property, condValue),
					"gte" => Expression.GreaterThanOrEqual(property, condValue),
					"like" => throw new NotImplementedException("Like not implemented yet"),
					_ => throw new ArgumentException($"Invalid operand: {operand}")	
				};

				if (expression != null)
					expression = Expression.AndAlso(expression, condition);
				else
					expression = condition;
			}
			
			return Expression.Lambda<Func<T, bool>>(expression!);
		}
	}
}