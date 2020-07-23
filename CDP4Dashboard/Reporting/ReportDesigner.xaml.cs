namespace CDP4Dashboard.Reporting
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Windows.Threading;

    using CDP4Common.EngineeringModelData;

    using DevExpress.DataAccess.ObjectBinding;
    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Reports.UserDesigner;
    using DevExpress.XtraReports.UI;

    using Microsoft.Win32;

    using Window = System.Windows.Window;

    /// <summary>
    /// Interaction logic for ReportDesigner.xaml
    /// </summary>
    public partial class ReportDesigner : Window
    {
        private readonly Iteration iteration;

        public ReportDesigner(Iteration iteration)
        {
            this.iteration = iteration;
            this.InitializeComponent();
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
                    DataSource = DataSourceCompiler.ClassicSharpCodeProvider(dataSourceName, this.DataSource, this.iteration)
                };

                localReport.ComponentStorage.Add(dataSource);
                localReport.DataSource = dataSource;
            }
            else
            {
                dataSource.DataSource = DataSourceCompiler.ClassicSharpCodeProvider(dataSourceName, this.DataSource, this.iteration);
            }

            dataSource.RebuildResultSchema();
        }

        //public void RebuildDataSource()
        //{
        //    MessageBox.Show("RebuildDatasource");
        //    var components = this.ComponentStorage.OfType<ObjectDataSource>().ToList();
        //    var dataSource = components.FirstOrDefault(x => x.Name.Equals(this.dataSourceName));
        //    var compiledDataSource = DataSourceCompiler.ClassicSharpCodeProvider(this.dataSourceName, this.DataSourceCode, this.iteration);

        //    if (dataSource == null)
        //    {
        //        dataSource = new ObjectDataSource
        //        {
        //            Name = this.dataSourceName,
        //            DataSource = compiledDataSource
        //        };

        //        this.ComponentStorage.Add(dataSource);
        //        this.DataSource = dataSource;
        //    }
        //    else
        //    {
        //        dataSource.DataSource = compiledDataSource;
        //    }

        //    dataSource.RebuildResultSchema();
        //}

        //public class ParameterValue<T>
        //{
        //    public ParameterValue(T value)
        //    {
        //        this.Value = value;
        //    }

        //    public T Value { get; private set; }
        //}

        //public class Data
        //{
        //    public string SelectedDomain { get; set; }

        //    public string ProductFunction { get; set; }

        //    public decimal Value { get; set; }
        //}

        //private object GetDataSource()
        //{
        //    var calculatedDataSource = new
        //    {
        //        Data = new List<Data>(),
        //        Parameters = new
        //        {
        //            ProductFunction = new List<ParameterValue<string>>
        //            {
        //                new ParameterValue<string>("Product"),
        //                new ParameterValue<string>("Function")
        //            },
        //            SelectedDomains = new List<ParameterValue<string>> 
        //                {
        //                    new ParameterValue<string>("AOGNC"),
        //                    new ParameterValue<string>("COM"),
        //                    new ParameterValue<string>("CPROP"),
        //                    new ParameterValue<string>("DH")
        //                }
        //        }
        //    };

        //    calculatedDataSource.Data.Add(new Data
        //    {
        //        SelectedDomain = "AOGNC",
        //        ProductFunction = "Product",
        //        Value = 1M
        //    });

        //    calculatedDataSource.Data.Add(new Data
        //    {
        //        SelectedDomain = "AOGNC",
        //        ProductFunction = "Function",
        //        Value = 2M
        //    });

        //    calculatedDataSource.Data.Add(new Data
        //    {
        //        SelectedDomain = "COM",
        //        ProductFunction = "Product",
        //        Value = 1.1M
        //    });

        //    calculatedDataSource.Data.Add(new Data
        //    {
        //        SelectedDomain = "COM",
        //        ProductFunction = "Function",
        //        Value = 2.1M
        //    });

        //    calculatedDataSource.Data.Add(new Data
        //    {
        //        SelectedDomain = "CPROP",
        //        ProductFunction = "Product",
        //        Value = 0.1M
        //    });

        //    calculatedDataSource.Data.Add(new Data
        //    {
        //        SelectedDomain = "CPROP",
        //        ProductFunction = "Function",
        //        Value = 0.2M
        //    });

        //    calculatedDataSource.Data.Add(new Data
        //    {
        //        SelectedDomain = "DH",
        //        ProductFunction = "Product",
        //        Value = 10M
        //    });

        //    calculatedDataSource.Data.Add(new Data
        //    {
        //        SelectedDomain = "DH",
        //        ProductFunction = "Function",
        //        Value = 20M
        //    });

        //    return calculatedDataSource;
        //}

        private void LocalReport_DataSourceDemanded(object sender, EventArgs e)
        {
            this.SetDataSource();
        }

        private void OpenReport(object sender, ItemClickEventArgs e)
        {
            var report = new XtraReport();

            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Report files (*.rep4)|*.rep4|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                this.fileName = openFileDialog.FileName;

                report.LoadLayoutFromXml(this.GetReportStream(this.fileName));
                this.reportDesigner.OpenDocument(report);

                var dataSourceStream = this.GetDataSourceStream(this.fileName);

                if (dataSourceStream == null)
                {
                    this.DataSource = DataSourceCompiler.GetDataSource();
                }
                else
                {
                    var reader = new StreamReader(dataSourceStream);
                    this.DataSource = reader.ReadToEnd();
                }

                this.Dispatcher.InvokeAsync(this.SetDataSource, DispatcherPriority.ApplicationIdle);
            }
        }

        private string fileName;
        private string DataSource;

        private void SaveReport(object sender, ItemClickEventArgs e)
        {
            var reportStream = new MemoryStream();
            this.reportDesigner.ActiveDocument.Report.SaveLayoutToXml(reportStream);
            var dataSourceStream = new MemoryStream(Encoding.ASCII.GetBytes(this.DataSource));

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Report files (*.rep4)|*.rep4|All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                this.fileName = saveFileDialog.FileName;

                if (System.IO.File.Exists(this.fileName))
                {
                    System.IO.File.Delete(this.fileName);
                }

                using (var zipFile = ZipFile.Open(this.fileName, ZipArchiveMode.Create))
                {
                    using (var reportEntry = zipFile.CreateEntry("report.repx").Open())
                    {
                        reportStream.Position = 0;
                        reportStream.CopyTo(reportEntry);
                    }

                    using (var reportEntry = zipFile.CreateEntry("datasource.ds").Open())
                    {
                        dataSourceStream.Position = 0;
                        dataSourceStream.CopyTo(reportEntry);
                    }
                }

                this.reportDesigner.ActiveDocument.SetValue(ReportDesignerDocument.HasChangesProperty, false);
            }
        }

        public Stream GetReportStream(string rep4File)
        {
            var zipFile = ZipFile.OpenRead(rep4File);
            return zipFile.Entries.FirstOrDefault(x => x.Name.EndsWith(".repx"))?.Open();
        }

        public Stream GetDataSourceStream(string rep4File)
        {
            var zipFile = ZipFile.OpenRead(rep4File);
            return zipFile.Entries.FirstOrDefault(x => x.Name.EndsWith(".ds"))?.Open();
        }
    }
}
