﻿using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using OData2Poco.Core;
using OData2Poco.TestUtility;


namespace OData2Poco.CommandLine.Test
{
   
    [TestFixture]
    public partial class ProgramTests 
    {
        private async Task<Tuple<int, string>> RunCommand(string s)
        {
              return await new ParserUtility().RunCommand(s);
        }

        [Test]
        [TestCaseSource(typeof(TestSample), nameof(TestSample.FileCases))]
        public async Task DefaultSettingTest(string url, string version, int n)
        {
            //Arrange
            var a = $"-r {url} -v";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
            //Assert
            Assert.AreEqual(0, tuble.Item1);
            Assert.IsTrue(output.Contains("public partial class Product"));
            Assert.IsFalse(output.Contains("System.ComponentModel.DataAnnotations")); //-a key /or -k not set
            Assert.IsFalse(output.Contains("System.ComponentModel.DataAnnotations.Schema")); //a- tab / or -t not set
        }

        [Test]
        [TestCaseSource(typeof(TestSample), nameof(TestSample.FileCases))]
        public async Task PocoSettingTest(string url, string version, int n)
        {
            //Arrange
            var a = $"-r {url} -v -a key tab req -n -b";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;           
            //Assert
            Assert.AreEqual(0, tuble.Item1);

            Assert.That(output, Does.Contain("public partial class Product"));
            Assert.That(output, Does.Contain("[Table(\"Products\")]")); //-a tab/ -t
            Assert.That(output, Does.Contain("System.ComponentModel.DataAnnotations.Schema")); //-a tab  
            Assert.That(output, Does.Contain("[Key]"));
            Assert.That(output, Does.Contain("System.ComponentModel.DataAnnotations")); //-a key  
            Assert.That(output, Does.Contain("[Required]"));  
            Assert.That(output, Does.Contain("public virtual Supplier Supplier {get;set;}")); //-n
            Assert.That(output, Does.Contain("int?"));  //-b
            
        }

       
        [Test]
        public async Task PocoWithInheritanceTest()
        {
            //Arrange
            var url = TestSample.TripPin4Flag;
            var a = $"-r {url} -v";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
            //Assert
            Assert.IsTrue(output.Contains("public partial class PublicTransportation : PlanItem"));
        }

        [Test(Description = "If model inheritance is used (the default) check that the propterties of a base calsss are not duplicated inderived classes")]
        public async Task PropertyInheritenceTest()
        {
            //Arrange
            var url = TestSample.TripPin4Flag; 
            var a = $"-r {url} -v";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;

            var lines = output.Split('\n');
            var occurneces = lines.Count(l => l.Contains("public int PlanItemId"));
            //Assert
            // Assert.IsTrue(occurneces == 1); // For inheritance, check that PlanItemId property only occurs in the base class
            Assert.That(occurneces, Is.EqualTo(1));

        }
        [Test]
        public async Task PocoWithBaseClassTest()
        {
            //Arrange
            var url = TestSample.TripPin4Flag; 
            const string myBaseClass = nameof(myBaseClass);

            var a = $"-r {url} -v -i {myBaseClass}";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;

            Assert.IsTrue(output.Contains($"public partial class PublicTransportation : {myBaseClass}"));

            var lines = output.Split('\n');
            var occurneces = lines.Count(l => l.Contains("public int PlanItemId"));
            //Assert
            Assert.IsTrue(occurneces > 1);
        }

        [Test(Description = "If model inheritance is not used, the properties from a base class should by duplicated in the derived classes.")]
        public async Task PropertyDuplicationTest()
        {
            //Arrange
            var url = TestSample.TripPin4Flag; 
            const string myBaseClass = nameof(myBaseClass);

            var a = $"-r {url} -v -i {myBaseClass}";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
            var lines = output.Split('\n');
            var occurneces = lines.Count(l => l.Contains("public int PlanItemId"));
            //Assert
            Assert.IsTrue(occurneces > 1); // If not using model inheritance, check that the PlanItemId property is duplicated in derived classes

        }

        [Test]
        [TestCaseSource(typeof(TestSample), nameof(TestSample.FileCases))]
        public async Task NullableDatatypeTest(string url, string version, int n)
        {
            //Arrange
            var a = $"-r {url} -v  -n -b"; //navigation propertis with complex data type
            //Act
            var tuble = await RunCommand(a);
            //Assert
            var output = tuble.Item2;
            //Assert
            Assert.AreEqual(0, tuble.Item1);
            Assert.IsTrue(output.Contains("public partial class Product"));
            Assert.IsTrue(output.Contains("public virtual Supplier Supplier {get;set;}")); //-n
            Assert.IsTrue(output.Contains("int?"));  //-b

        }

        [Test]
        [TestCaseSource(typeof(TestSample), nameof(TestSample.FileCases))]
        public async Task PocoSettingWithJsonAttributeTest(string url, string version, int n)
        {
            //Arrange
            var a = $"-r {url}  -j -v";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
          
            //Assert
            Assert.AreEqual(0, tuble.Item1); //exit code
            Assert.IsTrue(output.Contains("public partial class Category"));
            Assert.IsTrue(output.Contains("[JsonProperty(PropertyName = \"CategoryID\")]"));
            Assert.IsTrue(output.Contains("Category"));
            Assert.IsTrue(output.Contains(" [JsonProperty(PropertyName = \"CategoryName\")]"));
            Assert.IsTrue(output.Contains("CategoryName"));
        }

        [Test]
        [TestCaseSource(typeof(TestSample), nameof(TestSample.FileCases))]
        public async Task PocoSettingWithJsonAttributeAndCamelCaseTest(string url, string version, int n)
        {
            //Arrange
            var a = $"-r {url}  -j -c camel -v";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
            //Assert
            Assert.IsTrue(output.Contains("public partial class Category"));
            Assert.IsTrue(output.Contains("[JsonProperty(PropertyName = \"CategoryName\")]"), "itshould be CategoryName");
            Assert.IsTrue(output.Contains("categoryName"));
            Assert.IsTrue(output.Contains("[JsonProperty(PropertyName = \"CategoryID\")]"));
            Assert.IsTrue(output.Contains("category"));
        }

        [Test]
        [TestCaseSource(typeof(TestSample), nameof(TestSample.FileCases))]
        public async Task PocoSettingWithJsonAttributePasCaseTest(string url, string version, int n)
        {
            //Arrange
            var a = $"-r {url}  -jc PAS -v";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
            //Assert
            Assert.AreEqual(0, tuble.Item1);
            Assert.IsTrue(output.Contains("[JsonProperty(PropertyName = \"CategoryID\")]"));
            Assert.IsTrue(output.Contains("Category"));
            Assert.IsTrue(output.Contains(" [JsonProperty(PropertyName = \"CategoryName\")]"));
            Assert.IsTrue(output.Contains("CategoryName"));

        }


        [Test]
        [TestCaseSource(typeof(TestSample), nameof(TestSample.FileCases))]
        public async Task PocoSettingEagerTest(string url, string version, int n)
        {
            //Arrange
            var a = $"-r {url} -v -e";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
            Assert.AreEqual(0, tuble.Item1);
            //Assert
            Assert.IsTrue(output.Contains("public partial class Product")); //-v
            Assert.IsTrue(output.Contains("public Supplier Supplier {get;set;}")); //-e

        }

        [Test]
        [TestCaseSource(typeof(TestSample), nameof(TestSample.FileCases))]
        public async Task PocoSettingInheritTest(string url, string version, int n)
        {
            //Arrange
            var a = string.Format("-r {0} -v -i {1}", url, "MyBaseClass,MyInterface");
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
            //Assert
            Assert.AreEqual(0, tuble.Item1);
            Assert.IsTrue(output.Contains("public partial class Product : MyBaseClass,MyInterface")); //-i, -v

        }

        [Test]
        [TestCaseSource(typeof(TestSample), nameof(TestSample.FileCases))]
        public async Task PocoSettingNamespaceTest(string url, string version, int n)
        {
            var a = $"-r {url} -v -m {"MyNamespace1.MyNamespace2"}";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
            //Assert
            Assert.AreEqual(0, tuble.Item1);
            Assert.IsTrue(output.Contains("MyNamespace1.MyNamespace2.")); //-m, -v

        }

        [Test]
        [TestCaseSource(typeof(TestSample), nameof(TestSample.FileCases))]
        public async Task FileWithSettingTest(string url, string version, int n)
        {
            //Arrange
            var a = $"-r {url} -v -a key tab req -n";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
            //Assert
            Assert.AreEqual(0, tuble.Item1);
            Assert.IsTrue(output.Contains("public partial class Product"));
            Assert.IsTrue(output.Contains("[Table")); //-t
            Assert.IsTrue(output.Contains("[Key]")); //-k
            Assert.IsTrue(output.Contains("[Required]"));  //-q
            Assert.IsTrue(output.Contains("virtual")); //-n
        }

        [Test]
        [TestCaseSource(typeof(TestSample), nameof(TestSample.FileCases))]
        public async Task FolderMaybeNotExistTest(string url, string version, int n)
        {
            //Arrange
            var fname = @"xyz.cs";
            var a = $"-r {url} -v -f {fname}";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
            //Assert
            Assert.AreEqual(0, tuble.Item1);
            Assert.IsTrue(output.Contains("public partial class Product")); //-v

        }
        [Test]
        public async Task Enum_issue_7_test()
        {
            //Arrange
            var url = TestSample.TripPin4Flag;
             

            var a = $"-r {url} -v";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
        
            var expected = @"
public enum Feature
	 {
 		Feature1=0,
 		Feature2=1,
 		Feature3=2,
 		Feature4=3 
	}
";
            //Assert
            Assert.That(output, Does.Match(expected.GetRegexPattern()));
          

        }
        [Test]
        public async Task Enum_issue_7_flag_support_test()
        {
            //Arrange
            var url = TestSample.TripPin4Flag;


            var a = $"-r {url} -v";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
            var expected = @"
[Flags] public enum PersonGender
         {
                Male=0,
                Female=1,
                Unknow=2
        }
"; 
            //Assert
            Assert.That(output, Does.Match(expected.GetRegexPattern()));
           

        }

        [Test]
        //[TestCaseSource(typeof(TestSample), nameof(TestSample.UrlNorthwindCases))]
        [TestCaseSource(typeof(TestSample), nameof(TestSample.UrlOdatadCases))]
        public async Task Url_test(string url, string version, int n)
        {
            //Arrange
            var a = $"-r {url} -v ";
            //Act
            var tuble = await RunCommand(a);
            var output = tuble.Item2;
            //Assert
            Assert.AreEqual(0, tuble.Item1);
            Assert.IsTrue(output.Contains("public partial class Product")); //-v

        }

    }

}

