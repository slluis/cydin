using System;
using NUnit.Framework;
namespace MonoDevelopNunitPlugInBug
{
	[SetUpFixture()]
	public class FixtureSetupTest
	{
		public FixtureSetupTest()
		{
			
		}
		
		[SetUp]
		public void Init()
		{
			throw new NotImplementedException();	
		}
		
		[TearDown]
		public void CloseDown()
		{
			
		}
		
	
	}
	
	[TestFixture()]
	public class Test
	{
		[Test()]
		public void TestCase()
		{
			// pass :)
		}
	}
}

