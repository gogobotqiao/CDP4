namespace CDP4Dashboard.Reporting
{
    using CDP4Composition.Reporting;

    using DevExpress.XtraReports.UI;

    public class CDP4Report : XtraReport
    {
        //public static Iteration iteration;

        public CDP4Report()
        {
            //this.DataSourceDemanded += this.XtraReport1_DataSourceDemanded;
        }

        private void XtraReport1_DataSourceDemanded(object sender, System.EventArgs e)
        {
            //if (this.DataSource is ICDP4ObjectDataSource CDP4DataSource)
            //{
            //    CDP4DataSource.CreatDataSource(TempStaticIterationClass.Iteration);
            //}
        }
    }
}
