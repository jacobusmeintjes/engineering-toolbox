using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using MyApp.Modules.Billing.Public;
using MyApp.Modules.Customers.Public;
using MyApp.Modules.Orders.Public;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace MyApp.ArchitectureTests
{
    public class ModularMonolithArchitectureTests
    {
        private static readonly Architecture Architecture = new ArchLoader()
            .LoadAssemblies(
                typeof(IOrdersModule).Assembly,
                typeof(CustomersModule).Assembly,
                typeof(BillingModule).Assembly)
            .Build();

        private static readonly string[] Modules = ["Orders", "Customers", "Billing"];

        /// <summary>
        /// Rule: No class in any module should depend on another module's Internal types (classes or interfaces).
        /// This is the core rule for modular monolith architecture.
        /// </summary>
        [Fact]
        public void No_Module_Should_Depend_On_Other_Modules_Internal_Namespaces()
        {
            foreach (var sourceModule in Modules)
            {
                foreach (var targetModule in Modules.Where(m => m != sourceModule))
                {
                    // Get all types (classes + interfaces) from target module's Internal namespace
                    var targetInternalTypes = Architecture.Types
                        .Where(t => t.Namespace.FullName.StartsWith($"MyApp.Modules.{targetModule}.Internal"))
                        .ToArray();

                    if (targetInternalTypes.Length == 0)
                        continue;

                    // Check each source module class doesn't depend on target Internal types
                    var sourceClasses = Architecture.Classes
                        .Where(c => c.Namespace.FullName.StartsWith($"MyApp.Modules.{sourceModule}"))
                        .ToList();

                    foreach (var sourceClass in sourceClasses)
                    {
                        var violations = sourceClass.Dependencies
                            .Where(d => targetInternalTypes.Any(t => t.FullName == d.Target.FullName))
                            .Select(d => $"  {sourceClass.FullName} depends on {d.Target.FullName} ({d.GetType().Name})")
                            .ToList();

                        Assert.True(violations.Count == 0,
                            $"Module '{sourceModule}' should not depend on '{targetModule}.Internal':\n{string.Join("\n", violations)}");
                    }
                }
            }
        }

        /// <summary>
        /// Rule: Internal namespaces should not reference other modules' non-Public types.
        /// This catches Internal-to-Internal dependencies specifically.
        /// </summary>
        [Fact]
        public void Internal_Code_Should_Not_Reference_Other_Modules_Internal_Code()
        {
            foreach (var sourceModule in Modules)
            {
                foreach (var targetModule in Modules.Where(m => m != sourceModule))
                {
                    var classRule = Classes()
                        .That().ResideInNamespace($"MyApp.Modules.{sourceModule}.Internal")
                        .Should().NotDependOnAny(
                            Types().That().ResideInNamespace($"MyApp.Modules.{targetModule}.Internal"))
                        .Because($"Internal code in '{sourceModule}' should not directly access Internal code in '{targetModule}'");

                    classRule.WithoutRequiringPositiveResults().Check(Architecture);
                }
            }
        }

        /// <summary>
        /// Rule: Public DTOs should be pure data contracts with no dependencies on Internal types.
        /// This ensures DTOs are safe to expose across module boundaries.
        /// </summary>
        [Fact]
        public void Public_DTOs_Should_Not_Reference_Any_Internal_Types()
        {
            foreach (var module in Modules)
            {
                var rule = Types()
                    .That().ResideInNamespace($"MyApp.Modules.{module}.Public.Dtos")
                    .Should().NotDependOnAny(
                        Types().That().ResideInNamespace(".Internal"))
                    .Because($"Public DTOs in '{module}' should be pure data contracts without Internal dependencies");

                rule.WithoutRequiringPositiveResults().Check(Architecture);
            }
        }

        /// <summary>
        /// Rule: Public interfaces should only expose Public types (DTOs), not Internal domain models.
        /// </summary>
        [Fact]
        public void Public_Interfaces_Should_Not_Expose_Internal_Types()
        {
            foreach (var module in Modules)
            {
                var rule = Interfaces()
                    .That().ResideInNamespace($"MyApp.Modules.{module}.Public")
                    .And().HaveNameStartingWith("I")
                    .Should().NotDependOnAny(
                        Types().That().ResideInNamespace($"MyApp.Modules.{module}.Internal"))
                    .Because($"Public interface in '{module}' should only expose DTOs, not Internal types");

                rule.WithoutRequiringPositiveResults().Check(Architecture);
            }
        }
    }
}
