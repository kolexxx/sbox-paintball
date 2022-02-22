using Sandbox;
using System.Collections.Generic;

namespace Paintball;

public static class ListExtensions
{
	public static void Shuffle<T>( this IList<T> list )
	{
		int n = list.Count;
		while ( n > 1 )
		{
			n--;
			int k = Rand.Int( 0, n );
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}
}
