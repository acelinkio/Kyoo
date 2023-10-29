// Kyoo - A portable and vast media library solution.
// Copyright (c) Kyoo.
//
// See AUTHORS.md and LICENSE file in the project root for full license information.
//
// Kyoo is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.
//
// Kyoo is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Kyoo. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Kyoo.Abstractions.Models.Attributes;

namespace Kyoo.Abstractions.Models.Utils;

/// <summary>
/// The aditional fields to include in the result.
/// </summary>
/// <typeparam name="T">The type related to the new fields</typeparam>
public class Include<T>
{
	/// <summary>
	/// The aditional fields to include in the result.
	/// </summary>
	public ICollection<string> Fields { get; private init; } = ArraySegment<string>.Empty;

	public static Include<T> From(string? fields)
	{
		if (string.IsNullOrEmpty(fields))
			return new Include<T>();

		return new Include<T>
		{
			Fields = fields.Split(',').Select(x =>
			{
				PropertyInfo? prop = typeof(T).GetProperty(x, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				if (prop?.GetCustomAttribute<LoadableRelationAttribute>() == null)
					throw new ValidationException($"No loadable relation with the name {x}.");
				return prop.Name;
			}).ToArray()
		};
	}
}