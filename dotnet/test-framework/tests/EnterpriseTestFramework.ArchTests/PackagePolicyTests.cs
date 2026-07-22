using System.Xml.Linq;

namespace EnterpriseFramework.ArchTests;

public class PackagePolicyTests
{
    [Fact]
    public void UnsupportedPackages_AreNotReferenced_InAnyProject()
    {
        var banned = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Newtonsoft.Json",
            "AutoMapper",
            "MediatR"
        };

        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var csprojFiles = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories);

        var violations = new List<string>();

        foreach (var csproj in csprojFiles)
        {
            var doc = XDocument.Load(csproj);
            var refs = doc.Descendants("PackageReference")
                          .Select(x => (string?)x.Attribute("Include"))
                          .Where(x => !string.IsNullOrWhiteSpace(x));

            foreach (var pkg in refs!)
            {
                if (banned.Contains(pkg!))
                    violations.Add($"{Path.GetRelativePath(root, csproj)} -> {pkg}");
            }
        }

        Assert.True(violations.Count == 0,
            "Unsupported packages found:\n" + string.Join(Environment.NewLine, violations));
    }
}