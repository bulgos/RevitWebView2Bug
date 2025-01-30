using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitWebView2Bug
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            InitialiseToolbar(application);

            return Result.Succeeded;
        }

        private void InitialiseToolbar(UIControlledApplication application)
        {
            var launchWebViewButton = CreatePushButtonData(typeof(LaunchWebViewWindow), "Launch WebView");
            var launchWebAuthButton = CreatePushButtonData(typeof(LaunchAuthWindowCommand), "Launch Auth");

            var panelName = "WebView";
            application.CreateRibbonTab(panelName);

            var componentPanel = application.CreateRibbonPanel(panelName, panelName);

            componentPanel.AddStackedItems(launchWebViewButton, launchWebAuthButton);
        }

        private PushButtonData CreatePushButtonData(Type type, string name)
        {
            string fullClassName = type.ToString();
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButtonData = new(name, name, assemblyLocation, fullClassName);

            return pushButtonData;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
