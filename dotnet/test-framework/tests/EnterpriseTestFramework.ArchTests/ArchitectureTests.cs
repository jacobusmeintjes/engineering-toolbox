using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Slices;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using EnterpriseFramework.Core.FitnessFunctions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace EnterpriseFramework.ArchTests
{
    public class ArchitectureTests
    {
        // Load once, statically — reused across all tests in the class for performance
        private static readonly Architecture architecture = new ArchLoader().LoadAssemblies(typeof(EnterpriseFramework.Core.Class1).Assembly).Build();
        private const string CoreNamespace = "EnterpriseFramework.Core";

        [Fact]
        [FitnessFunction(FitnessCategory.Structural, FitnessCadence.Continuous,
        owner: "framework-team",
        rationale: "Core domain must not depend on infrastructure to maintain clean architecture and testability")]
        public void Domain_Should_Not_DependOn_Infrastructure()
        {
            IArchRule rule = Types()
                .That()
                .ResideInNamespace(CoreNamespace)
                .Should()
                .NotDependOnAny(Types().That().ResideInNamespace("MyFramework.Infrastructure"));

            rule.Check(architecture);
        }

        [Fact]
        [FitnessFunction(FitnessCategory.Structural, FitnessCadence.Continuous,
        owner: "framework-team",
        rationale: "Cyclic namespace dependencies make incremental change unsafe")]
        public void NoCyclicDependencies()
        {
            IArchRule rule = SliceRuleDefinition.Slices().Matching($"{CoreNamespace}.(*)..").Should().BeFreeOfCycles();

            rule.Check(architecture);
        }

        [Fact]
        [FitnessFunction(FitnessCategory.Structural, FitnessCadence.Continuous,
        owner: "framework-team",
        rationale: "Type names outside their namespace's naming convention obscure architectural role (query predicate, wire contract, integration adapter), undermining comprehension without reading implementation")]
        public void NamingConventionsAreFollowed()
        {
            Assert.Multiple(
                () => Types().That().ResideInNamespace($"{CoreNamespace}.Specifications").Should().HaveNameEndingWith("Specification").Check(architecture),
                () => Types().That().ResideInNamespace($"{CoreNamespace}.Models").Should().HaveNameEndingWith("Dto").Check(architecture),
                () => Types().That().ResideInNamespace($"{CoreNamespace}.Clients").Should().HaveNameEndingWith("Client").Check(architecture)
            );
        }

        [Fact]
        [FitnessFunction(FitnessCategory.Structural, FitnessCadence.Continuous,
        owner: "framework-team",
        rationale: "Unsupported dependencies compromise security, introduce incompatible APIs, and create licensing risks")]
        public void TechnologyStackDoesNotContainUnsupportedTechnologies()
        {
            IArchRule rule = Types()
                .That()
                .ResideInNamespace(CoreNamespace)
                .Should()
                .NotDependOnAny(
                    Types()
                    .That()
                    .ResideInAssembly("Newtonsoft.Json")
                    .Or().ResideInNamespace("AutoMapper")
                    .Or().ResideInNamespace("MediatR")
                );
            rule.Check(architecture);
        }

        [Fact]
        [FitnessFunction(FitnessCategory.Structural, FitnessCadence.Continuous,
        owner: "framework-team",
        rationale: "Public types must implement required extension interfaces to maintain contract compliance and interoperability")]
        public void InheritanceInterfaceContractTests()
        {
            //IArchRule rule = Classes().That().ArePublic().And().ResideInNamespace("MyFramework.Plugins").Should().ImplementInterface(typeof(IPlugin));
            //rule.Check(architecture);
        }

        [Fact]
        [FitnessFunction(FitnessCategory.Structural, FitnessCadence.Continuous,
        owner: "framework-team",
        rationale: "Sealing public classes prevents uncontrolled inheritance, protecting invariants and enabling safer refactoring")]
        public void ImmutabilitySealedTests()
        {
            IArchRule rule = Classes()
                .That()
                .ArePublic()
                .And().ResideInNamespaceMatching($"{CoreNamespace}..")
                .Should()
                .BeSealed()
                .WithoutRequiringPositiveResults();


            // Evaluate instead of Check to inspect results yourself
            var result = rule.Evaluate(architecture).Where(c => !c.Passed).ToList();

            Assert.Empty(result);
        }

        [Fact]
        [FitnessFunction(FitnessCategory.Structural, FitnessCadence.Continuous,
        owner: "framework-team",
        rationale: "Public fields expose internal state, violating encapsulation and enabling uncontrolled mutations")]
        public void NoPublicSettersOnConfigOrDTOs()
        {
            //IArchRule rule = Fields().That().ArePublic().And().ResideInNamespace(CoreNamespace).Should().NotExist();
            //rule.Check(architecture);
        }
    }
}
