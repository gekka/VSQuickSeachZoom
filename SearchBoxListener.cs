using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Windows;
using System.Windows.Media;
namespace Gekka.VisualStudio.Extention
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class QuiceSearchZoomListener : IWpfTextViewCreationListener
    {
        public QuiceSearchZoomListener()
        {
        }
        private static EnvDTE.DTE _dte;
        private static EnvDTE.CommandEvents _commandEvents;
        private static string guidFind;
        private static int idFind;
        private IWpfTextView _view;

        [Import]
        internal Microsoft.VisualStudio.Shell.SVsServiceProvider ServiceProvider = null;

        public void TextViewCreated(IWpfTextView textView)
        {
            _view = textView;

            if (_dte == null)
            {
                _dte=ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
                if (_dte != null)
                {
                    _commandEvents = _dte.Events.CommandEvents;
                    _commandEvents.AfterExecute += _commandEvents_AfterExecute;

                    EnvDTE.Command cmd = _dte.Commands.Item("Edit.Find");
                    guidFind = cmd.Guid;
                    idFind=cmd.ID;
                }
            }          
        }

        void _commandEvents_AfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            if (Guid == guidFind && ID == idFind)
            {
               var findUI= FindFindUI(_view.VisualElement,0) as FrameworkElement;
               if (findUI != null)
               {
                   if(findUI.RenderTransform ==null || findUI.RenderTransform== ScaleTransform.Identity)
                   {
                       findUI.RenderTransform = new ScaleTransform(1, 1);
                       findUI.RenderTransformOrigin=new Point(1,0);
                       findUI.MouseWheel += findUI_MouseWheel;
                   }
               }
            }
        }

        void findUI_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            ScaleTransform st = fe.RenderTransform as ScaleTransform;
            if (st != null)
            {
                double d;
              
                if (e.Delta > 0)
                {
                    d = st.ScaleX * 1.25;
                }
                else
                {
                    d = st.ScaleX / 1.25;
                }
                if (0.1 <= d && d <= 100)
                {
                    st.ScaleX = d;
                    st.ScaleY = d;
                }
                e.Handled = true;
            }
        }

        private System.Windows.DependencyObject FindFindUI(System.Windows.DependencyObject d, int deep)
        {

            deep += 1;
            string s = string.Empty.PadLeft(deep, '-');
            int count = 0;
            if (d is Visual)
            {
                count = VisualTreeHelper.GetChildrenCount(d);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject dChild = VisualTreeHelper.GetChild(d, i);
                    System.Windows.Controls.Control c = dChild as System.Windows.Controls.Control;
                    if (c != null)
                    {
                        System.Diagnostics.Debug.WriteLine(s + dChild.GetType().Name.ToString() + "\t" + c.Name);

                        if (c.Name == "FindControl")
                        {
                            return c;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(s + dChild.GetType().Name.ToString());
                    }
                    var ret=FindFindUI(dChild, deep);
                    if (ret != null)
                    {
                        return ret;
                    }

                }
            }
            if (count == 0)
            {
                var logicalChildren=LogicalTreeHelper.GetChildren(d);
                if (logicalChildren != null)
                {
                    foreach (object o in logicalChildren)
                    {
                        var dlogical = o as DependencyObject;
                        if (dlogical != null)
                        {
                           var ret= FindFindUI(dlogical, deep);
                           if (ret != null)
                           {
                               return ret;
                           }
                        }
                    }
                }
            }
            return null;
        }
    }
}
