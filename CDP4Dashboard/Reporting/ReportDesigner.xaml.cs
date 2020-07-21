namespace CDP4Dashboard.Reporting
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Reporting;

    using DevExpress.Data.Filtering;
    using DevExpress.DataAccess.ObjectBinding;
    using DevExpress.Diagram.Core.Native;
    using DevExpress.Mvvm.Native;
    using DevExpress.XtraReports.UI;

    using NetOffice.MSProjectApi;
    using NetOffice.OutlookApi;

    using Window = System.Windows.Window;

    /// <summary>
    /// Interaction logic for ReportDesigner.xaml
    /// </summary>
    public partial class ReportDesigner : Window
    {


        public class ArrayJoin : ICustomFunctionOperator
        {

            // Evaluates the function on the client.
            object ICustomFunctionOperator.Evaluate(params object[] operands)
            {
                return "'" + string.Join(", ", (string[])operands[0]) + "'";
            }

            string ICustomFunctionOperator.Name
            {
                get { return nameof(ArrayJoin); }
            }

            Type ICustomFunctionOperator.ResultType(params Type[] operands)
            {
                return typeof(string);
            }
        }

        public ReportDesigner(Iteration iteration)
        {
            this.InitializeComponent();
            CriteriaOperator.RegisterCustomFunction(new ArrayJoin());
            this.reportDesigner.ActiveDocumentChanged += (sender, args) =>
            {
                if (args.NewValue != null)
                {
                    var activeDocument = (DevExpress.Xpf.Reports.UserDesigner.ReportDesignerDocument)args.NewValue;
                    var localReport = activeDocument.Report;

                    //localReport.DataSourceDemanded += this.LocalReport_DataSourceDemanded;

                    this.SetDataSource();
                }
            };
        }

        private void SetDataSource()
        {
            var activeDocument = this.reportDesigner.ActiveDocument;
            var localReport = activeDocument.Report;

            var components = localReport.ComponentStorage.OfType<ObjectDataSource>().ToList();
            var dataSourceName = "MassBudgetDataSource";
            var dataSource = components.FirstOrDefault(x => x.Name.Equals(dataSourceName));

            if (dataSource == null)
            {
                dataSource = new ObjectDataSource
                {
                    Name = dataSourceName,
                    DataSource = this.GetDataSource()
                };

                localReport.ComponentStorage.Add(dataSource);
                localReport.DataSource = dataSource;
            }
            else
            {
                dataSource.DataSource = this.GetDataSource();
            }

            dataSource.RebuildResultSchema();
        }

        public class ParameterValue<T>
        {
            public ParameterValue(T value)
            {
                this.Value = value;
            }

            public T Value { get; private set; }
        }

        public class Data
        {
            public string SelectedDomain { get; set; }

            public string ProductFunction { get; set; }

            public decimal Value { get; set; }
        }

        private object GetDataSource()
        {
            var calculatedDataSource = new
            {
                Data = new List<Data>(),
                Parameters = new
                {
                    ProductFunction = new List<ParameterValue<string>>
                    {
                        new ParameterValue<string>("Product"),
                        new ParameterValue<string>("Function")
                    },
                    SelectedDomains = new List<ParameterValue<string>> 
                        {
                            new ParameterValue<string>("AOGNC"),
                            new ParameterValue<string>("COM"),
                            new ParameterValue<string>("CPROP"),
                            new ParameterValue<string>("DH")
                        }
                }
            };

            calculatedDataSource.Data.Add(new Data
            {
                SelectedDomain = "AOGNC",
                ProductFunction = "Product",
                Value = 1M
            });

            calculatedDataSource.Data.Add(new Data
            {
                SelectedDomain = "AOGNC",
                ProductFunction = "Function",
                Value = 2M
            });

            calculatedDataSource.Data.Add(new Data
            {
                SelectedDomain = "COM",
                ProductFunction = "Product",
                Value = 1.1M
            });

            calculatedDataSource.Data.Add(new Data
            {
                SelectedDomain = "COM",
                ProductFunction = "Function",
                Value = 2.1M
            });

            calculatedDataSource.Data.Add(new Data
            {
                SelectedDomain = "CPROP",
                ProductFunction = "Product",
                Value = 0.1M
            });

            calculatedDataSource.Data.Add(new Data
            {
                SelectedDomain = "CPROP",
                ProductFunction = "Function",
                Value = 0.2M
            });

            calculatedDataSource.Data.Add(new Data
            {
                SelectedDomain = "DH",
                ProductFunction = "Product",
                Value = 10M
            });

            calculatedDataSource.Data.Add(new Data
            {
                SelectedDomain = "DH",
                ProductFunction = "Function",
                Value = 20M
            });

            return calculatedDataSource;
        }

        private void LocalReport_DataSourceDemanded(object sender, EventArgs e)
        {
            this.SetDataSource();
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
