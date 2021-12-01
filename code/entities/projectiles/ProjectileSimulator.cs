using Sandbox;
using System.Collections.Generic;

namespace PaintBall
{
	public partial class ProjectileSimulator
	{
		public List<Projectile> List { get; private set; }
		public Player Owner { get; private set; }

		public ProjectileSimulator( Player owner )
		{
			List = new();
			Owner = owner;
		}

		public void Add( Projectile projectile )
		{
			List.Add( projectile );
		}

		public void Remove( Projectile projectile )
		{
			List.Remove( projectile );
		}

		public void Clear()
		{
			foreach ( var projectile in List )
			{
				projectile.Delete();
			}

			List.Clear();
		}

		public void Simulate()
		{
			for ( int i = List.Count - 1; i >= 0; i-- )
			{
				var projectile = List[i];

				if ( !projectile.IsValid() )
				{
					List.RemoveAt( i );
					continue;
				}

				if ( Prediction.FirstTime )
					projectile.Simulate();
			}
		}
	}

	public static class ProjectileSimulatorExtensions
	{
		public static bool IsValid( this ProjectileSimulator simulator )
		{
			return simulator != null && (simulator.Owner?.IsValid() ?? false);
		}
	}
}
