using Autodesk.Windows;
using System.Windows.Media.Imaging;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using System.IO;

namespace ButtonTabCivil
{
    public class Application : IExtensionApplication
    {
        public void Initialize()
        {
            ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;
        }

        public void Terminate()
        {
            ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;
        }

        private void ComponentManager_ItemInitialized(object sender, EventArgs e)
        {
            // Unsubscribe after the ribbon is initialized to avoid duplicate tabs
            ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;

            AddRibbonTab();

        }

        public void AddRibbonTab()
        {
            // Get the ribbon control
            RibbonControl ribbonControl = ComponentManager.Ribbon;

            // Create a new tab
            RibbonTab ribbonTab = new RibbonTab
            {
                Title = "Jacobian Dev",
                Id = "{4C785661-A9B7-4DF9-8903-7269B833F579}"
            };

            ribbonControl.Tabs.Add(ribbonTab);

            // Create a panel
            RibbonPanelSource panelSource = new RibbonPanelSource
            {
                Title = "Jacobian"
            };

            RibbonPanel ribbonPanel = new RibbonPanel
            {
                Source = panelSource
            };

            ribbonTab.Panels.Add(ribbonPanel);

            string thisAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Jacobian32.ico"));
            BitmapImage bitImage = new BitmapImage(uri);

            RibbonButton ribbonButton = new RibbonButton
            {
                Text = "First Plugin",
                ShowText = true,
                ShowImage = true,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                LargeImage = bitImage,
                Size = RibbonItemSize.Large,
                CommandHandler = new RibbonCommandHandler()
            };

            panelSource.Items.Add(ribbonButton);
        }

        public class RibbonCommandHandler : System.Windows.Input.ICommand
        {
            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                try
                {
                    // Get the current AutoCAD and Civil 3D documents
                    var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                    var cdoc = CivilApplication.ActiveDocument;

                    if (cdoc == null)
                    {
                        Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("No Civil 3D document is open.");
                        return;
                    }

                    // Start a transaction
                    using (var transaction = doc.TransactionManager.StartTransaction())
                    {
                        // Get the alignment names
                        var alignmentNames = cdoc.GetAlignmentIds()
                            .Cast<ObjectId>()
                            .Select(id => transaction.GetObject(id, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead) as Alignment)
                            .Where(alignment => alignment != null)
                            .Select(alignment => alignment.Name)
                            .ToList();

                        // Display the alignment names
                        string message = alignmentNames.Any()
                            ? "Alignments in the current document:\n" + string.Join("\n", alignmentNames)
                            : "No alignments found in the document.";

                        Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(message);

                        transaction.Commit();
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog($"Error: {ex.Message}");
                }

            }
        }

    }
}
