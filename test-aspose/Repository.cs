using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace test_aspose
{
	/// <summary> const composition </summary>
	internal class Repository
	{
		internal readonly ContextEmployeeMaterial[] Nodes;
		internal readonly Link[] Links;
		internal readonly int[] Connectivity;
		internal readonly int[][] Cycles;
		internal readonly List<Index> Roots;
		internal readonly SalaryGroup[] Cahce;

		public Repository(ContextEmployeeMaterial[] employees, Link[] links)
		{
			Links = RemoveLoops(links);
			Nodes = employees;
			Roots = new List<Index>(Nodes.Length);
			Cahce = new SalaryGroup[Nodes.Length];
			Connectivity = new int[Nodes.Length];

			FindConnectivity();
			FindRoots();
			SetCache();

			Cycles = FindCycles();
		}

		// public ContextEmployee GetEmployee(string name, DateTime accepted, int level)
		// {
		// 	var detached = new ContextEmployeeMaterial(name, accepted, level);
		// 	var index = Array.IndexOf(Nodes, detached);
		// 	return new ContextEmployeeAttached(this, index);
		// }

		private Link[] RemoveLoops(Link[] source)
		{
			var clean = new List<Link>(source.Length);
			for(var index = 0; index < source.Length; index++)
			{
				if((int)source[index].Chef != (int)source[index].Sub)
				{
					clean.Add(source[index]);
				}
			}
			return clean.ToArray();
		}

		private void FindConnectivity()
		{
			//! 6 lost in 2
			var visit = new HashSet<int>();
			var sub = 0;
			while(visit.Count < Nodes.Length)
			{
				sub++;
				var traverse = new Stack<Index>();
				for(var index = 0; index < Nodes.Length; index++)
				{
					if(!visit.Contains(index))
					{
						traverse.Push(index);
						break;
					}
				}

				while(traverse.Count > 0)
				{
					var indexCurrent = traverse.Pop();
					Connectivity[(int)indexCurrent] = sub;
					visit.Add((int)indexCurrent);
					for(var index = 0; index < Links.Length; index++)
					{
						if((int)Links[index].Chef == (int)indexCurrent)
						{
							var next = (int)Links[index].Sub;
							if(!visit.Contains(next))
							{
								traverse.Push(next);
							}
						}
					}
				}
			}
		}

		private void FindRoots()
		{
			Roots.Clear();
			var components = Connectivity.Distinct().ToArray();
			foreach(var component in components)
			{
				for(var indexAssumed = 0; indexAssumed < Nodes.Length; indexAssumed++)
				{
					if(Connectivity[indexAssumed] != component)
					{
						continue;
					}

					var indexChecking = 0;
					for(; indexChecking < Nodes.Length; indexChecking++)
					{
						if(Connectivity[indexChecking] != component)
						{
							continue;
						}

						if(IsReachable(indexAssumed, indexChecking) == -1)
						{
							break;
						}
					}

					if(indexChecking == Nodes.Length)
					{
						Roots.Add(indexAssumed);
					}
				}
			}
		}

		private int[][] FindCycles()
		{
			var cycles = new List<Stack<int>>();
			var weights = new int[Nodes.Length];
			foreach(var sub in Connectivity.Distinct())
			{
				for(var indexAssumed = 0; indexAssumed < Nodes.Length; indexAssumed++)
				{
					if(Connectivity[indexAssumed] != sub)
					{
						continue;
					}

					for(var index = 0; index < weights.Length; index++)
					{
						weights[index] = int.MaxValue;
					}
					weights[indexAssumed] = 0;

					var visit = new HashSet<int>();
					var traverse = new Queue<Index>();
					traverse.Enqueue(indexAssumed);
					while(traverse.Count > 0)
					{
						var indexCurrent = traverse.Dequeue();
						visit.Add((int)indexCurrent);
						for(var index = 0; index < Links.Length; index++)
						{
							if((int)Links[index].Chef == (int)indexCurrent)
							{
								var next = (int)Links[index].Sub;
								if(!visit.Contains(next))
								{
									traverse.Enqueue(next);
									weights[next] = weights[(int)indexCurrent] + 1;
								}

								if(next == indexAssumed)
								{
									var path = new Stack<int>();

									var min = int.MaxValue;
									var indexMin = -1;

									while(min != 0)
									{
										path.Push((int)indexCurrent);
										for(var indexReverse = 0; indexReverse < Links.Length; indexReverse++)
										{
											if((int)Links[indexReverse].Sub == (int)indexCurrent)
											{
												next = (int)Links[indexReverse].Chef;
												if(min > weights[next])
												{
													indexMin = next;
													min = weights[next];
												}
											}
										}
										indexCurrent = indexMin;
									}

									path.Push(indexAssumed);
									cycles.Add(path);

									traverse.Clear();
									break;
								}
							}
						}
					}
				}
			}

			var result = new List<int[]>();
			for(var index = 0; index < cycles.Count; index++)
			{
				var set = cycles[index].ToArray();
				var min = set.Min();
				var indexMin = Array.IndexOf(set, min);
				set.Shift(indexMin);
				if(!result.Any(_ => _.SequenceEqual(set)))
				{
					result.Add(set);
				}
			}

			return result.ToArray();
		}

		private void SetCache()
		{
			for(var index = 0; index < Nodes.Length; index++)
			{
				var factory = Nodes[index] as ISalesGroupFactory;
				if(ReferenceEquals(null, factory))
				{
					throw new Exception("factory not found");
				}
				Cahce[index] = factory.Create(this, index);
			}
		}

		/// <summary> returns distance or -1 if unreachable </summary>
		public int IsReachable(Index from, Index to)
		{
			var visit = new HashSet<int>();
			var traverse = new Queue<Index>();
			var weights = new Queue<int>();
			traverse.Enqueue(from);
			weights.Enqueue(0);
			visit.Add((int)from);
			while(traverse.Count > 0)
			{
				var currentIndex = traverse.Dequeue();
				var currentWeight = weights.Dequeue();

				if((int)currentIndex == (int)to)
				{
					return currentWeight;
				}

				for(var index = 0; index < Links.Length; index++)
				{
					if((int)Links[index].Chef == (int)currentIndex)
					{
						var next = Links[index].Sub;
						if(!visit.Contains((int)next))
						{
							traverse.Enqueue(next);
							weights.Enqueue(currentWeight + 1);
							visit.Add((int)next);
						}
					}
				}
			}

			return -1;
		}

		public IEnumerable<Index> Filter(Predicate<ContextEmployee> predicate)
		{
			return new Wrapper<Index>(FilterRoll(predicate));
		}

		private IEnumerator<Index> FilterRoll(Predicate<ContextEmployee> predicate)
		{
			var index = -1;
			predicate = predicate ?? (_ => true);
			while(++index < Nodes.Length)
			{
				if(predicate(Nodes[index]))
				{
					yield return index;
				}
			}
		}
	}
}
