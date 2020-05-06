namespace CDP4Dashboard.Reporting
{
    using System;
    using System.CodeDom.Compiler;
    using System.Linq;
    using System.Windows;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Reporting;

    using DevExpress.DataAccess.ObjectBinding;

    /// <summary>
    /// Interaction logic for ReportDesigner.xaml
    /// </summary>
    public partial class ReportDesigner : Window
    {
        public ReportDesigner(Iteration iteration)
        {
            this.InitializeComponent();

            this.reportDesigner.ActiveDocumentChanged += (sender, args) =>
            {
                if (args.NewValue != null)
                {
                    var localReport = ((DevExpress.Xpf.Reports.UserDesigner.ReportDesignerDocument) args.NewValue).Report;

                    foreach (var component in localReport.ComponentStorage.OfType<ObjectDataSource>().ToList())
                    {
                        localReport.ComponentStorage.Remove(component);
                        localReport.Container?.Remove(component);
                    }

                    var dataSourceName = "MassBudgetDataSource";

                    var dataSource = new ObjectDataSource()
                    {
                        DataSource = ClassicSharpCodeProvider(iteration, dataSourceName),
                        Name = dataSourceName
                    };

                    localReport.DataSource = dataSource;
                }
            };
        }

        /// <summary>
        /// Using the class CSharpCode Provider
        /// </summary>
        public static object ClassicSharpCodeProvider(Iteration iteration, string assemblyName)
        {
            var compiler = new Microsoft.CSharp.CSharpCodeProvider();//.CreateCompiler();

            var parms = new CompilerParameters();

            parms.ReferencedAssemblies.Add("System.dll");
            parms.ReferencedAssemblies.Add("System.Core.dll");
            parms.ReferencedAssemblies.Add("System.Collections.dll");
            parms.ReferencedAssemblies.Add("System.Linq.dll");
            parms.ReferencedAssemblies.Add("System.Windows.dll");

            parms.ReferencedAssemblies.Add("CDP4Common.dll");
            parms.ReferencedAssemblies.Add("CDP4Composition.dll");

            parms.GenerateInMemory = true;
            parms.GenerateExecutable = false; // True = EXE, False = DLL

            var classCode = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CDP4Common.EngineeringModelData;
using CDP4Composition.Reporting;

namespace CDP4Reports
{
   public class MassBudgetDataSource : ICDP4ObjectDataSource
    {

        public class MassBudgetDataRow : IReportViewModel<MassBudgetDataRow>
        {
            public MassBudgetDataRow()
            {
            }

            public MassBudgetDataRow Parent { get; set; }

            public List<MassBudgetDataRow> Children { get; set; }

            [ParameterTypeShortName(""m"")]
            public string _Mass { get; set; }

            [ParameterTypeShortName(""mass_margin"")]
            public string _MassMargin { get; set; }

            [ParameterTypeShortName(""model"")]
            public string Model { get; set; }

            [ParameterTypeShortName(""type"")]
            public string Type { get; set; }

            [ParameterTypeShortName(""supplier"")]
            public string Supplier { get; set; }

            [ParameterTypeShortName(""justification"")]
            public string Justification { get; set; }

            [ParameterTypeShortName(""n_items"")]
            public string _NumberOfItems { get; set; }

            [ParameterTypeShortName(""ReportOrder"")]
            public string ReportOrder { get; set; }

            private List<MassBudgetDataRow> GetChildren()
            {
		    if (this.Children == null) {
                return new List<MassBudgetDataRow>();
		    }
			return this.Children.ToList();
            }

            public MassBudgetDataRow GetEmptyOrderedRow()
            {
                var newRow = Activator.CreateInstance<MassBudgetDataRow>();
                newRow.ReportOrder = this.ReportOrder;
                return newRow;
            }

            public double? Mass
            {
                get
                {	
		    double result;
                    if (double.TryParse(this._Mass, out result))
                    {
                        return result;
                    }

                    return null;
                }
            }

            public double? NumberOfItems
            {
                get
                {
		    double result;
                    if (double.TryParse(this._NumberOfItems, out result))
                    {
                        return result;
                    }

                    return null;
                }
            }

            public double? MassMargin
            {
                get
                {
		    double result;
                    if (double.TryParse(this._MassMargin, out result))
                    {
                        return result / 100;
                    }

                    return null;
                }
            }

            public double? TotalMass
            {
                get
                {
                    var children = this.GetChildren();

                    if (children.Any())
                    {
                        return children.Sum(x => x.TotalMass);
                    }

                    if (this.Mass.HasValue)
                    {
                        return (this.NumberOfItems ?? 1) * this.Mass.Value;
                    }

                    return null;
                }
            }


            public double? UnitDryMassMargin
            {
                get
                {
                    if (this.MassMargin.HasValue && this.Mass.HasValue)
                    {
                        return Math.Round(this.MassMargin.Value * this.Mass.Value, 2);
                    }


                    return null;

                }
            }


            public double? UnitMassWithDryMassMargin
            {
                get
                {
                    if (this.UnitDryMassMargin.HasValue && this.Mass.HasValue)
                    {
                        return this.UnitDryMassMargin.Value + this.Mass.Value;
                    }

                    return null;
                }
            }

            public double? TotalDryMassMargin
            {
                get
                {
                    var children = this.GetChildren();

                    if (children.Any())
                    {
                        return children.Sum(x => x.TotalDryMassMargin);
                    }

                    if (this.TotalMass.HasValue && this.MassMargin.HasValue)
                    {
                        return this.TotalMass.Value * this.MassMargin;
                    }

                    return null;
                }
            }

            public double? TotalMassWithDryMassMargin
            {
                get
                {
                    var children = this.GetChildren();

                    if (children.Any())
                    {
                        return children.Sum(x => x.TotalMassWithDryMassMargin);
                    }

                    if (this.TotalMass.HasValue && this.TotalDryMassMargin.HasValue)
                    {
                        return this.TotalMass.Value + this.TotalDryMassMargin.Value;
                    }

                    return null;
                }
            }

            public double? PercentageOfTotal
            {
                get
                {
                    if (this.TotalMassWithDryMassMargin.HasValue)
                    {
                        var parentTotalMassWithDryMassMargin = this.GetParentTotalMassWithDryMassMargin();

                        if (parentTotalMassWithDryMassMargin.HasValue && parentTotalMassWithDryMassMargin != 0D)
                        {
                            return this.TotalMassWithDryMassMargin.Value / parentTotalMassWithDryMassMargin.Value;
                        }

                        return 1;
                    }

                    return null;
                }
            }

            public double? GetParentTotalMassWithDryMassMargin()
            {
                var parent = this.Parent;

                while (parent != null)
                {
                    if (parent.TotalMassWithDryMassMargin.HasValue)
                    {
                        return Math.Round(parent.TotalMassWithDryMassMargin.Value, 2);
                    }

                    parent = parent.Parent;
                }

                return null;
            }
        }

        public class MassBudgetDataClass : DefinitionUsage<MassBudgetDataRow>
        {
            public MassBudgetDataClass(CategoryHierarchy categoryHierarchy, ElementDefinition elementDefinition) : base(categoryHierarchy, elementDefinition)
            {
            }
        }

        public object CreateDataSource(Iteration iteration)
        {
            var topelement = iteration.TopElement;

            var categoryHierarchy = CategoryHierarchy
                .CreateTopLevelCategoryHierarchy(CategoryHierarchy.GetCategoryByShortName(iteration, ""Project""));

            var moduleGroupCategoryHierarchy = categoryHierarchy
                .AddChildCategory(CategoryHierarchy.GetCategoryByShortName(iteration, ""module_group""))
                .WithGroupFooterText(""Total module wet mass"");

            moduleGroupCategoryHierarchy
                .AddChildCategory(CategoryHierarchy.GetCategoryByShortName(iteration, ""SYS""))
                .WithGroupFooterText(""Total module dry mass"")
                .AddChildCategory(CategoryHierarchy.GetCategoryByShortName(iteration, ""Module""))
                .AddChildCategory(CategoryHierarchy.GetCategoryByShortName(iteration, ""SS""))
                .AddChildCategory(CategoryHierarchy.GetCategoryByShortName(iteration, ""Assembly""))
                .AddChildCategory(CategoryHierarchy.GetCategoryByShortName(iteration, ""EQT""));

            moduleGroupCategoryHierarchy
                .AddChildCategory(CategoryHierarchy.GetCategoryByShortName(iteration, ""PropellantGroup""))
                .WithGroupFooterText(""Total propellant mass"")
                .AddChildCategory(CategoryHierarchy.GetCategoryByShortName(iteration, ""Propellant""));

            moduleGroupCategoryHierarchy
                .AddChildCategory(CategoryHierarchy.GetCategoryByShortName(iteration, ""Pressurant""))
                .WithGroupFooterText(""Total pressurant"");

            var definitionUsage = new MassBudgetDataClass(categoryHierarchy, topelement);
            categoryHierarchy.FillDefinitionUsages(topelement, definitionUsage);
            definitionUsage.IsCalculated = true;

            return definitionUsage;
        }
    }
}";

            var result = compiler.CompileAssemblyFromSource(parms, classCode);

            if (result.Errors.Count > 0)
            {
                Console.WriteLine(" * ** Compilation Errors");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine("- " + error);

                    return null;
                }
            }

            var ass = result.CompiledAssembly;

            //AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(parms.OutputAssembly));

            var inst = ass.CreateInstance("CDP4Reports.MassBudgetDataSource") as ICDP4ObjectDataSource;
            return inst?.CreateDataSource(iteration);
        }
    }
}
