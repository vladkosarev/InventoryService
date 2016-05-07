using System;
using Xunit;
using FsCheck;
using System.Linq;
using System.Collections.Generic;

namespace InventoryService.Tests
{
	public class PropertyTests
	{
		[Fact]
		public void RevRevIsOrig(){
			Prop.ForAll<int[]>(xs => xs.Reverse().Reverse().SequenceEqual(xs))
				.QuickCheckThrowOnFailure();
		}
	}
}

